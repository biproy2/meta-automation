using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Services;

public class PathaoService(IHttpClientFactory httpClientFactory, ILogger<PathaoService> logger) : IPathaoService
{
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    // Token cache per tenant
    private readonly Dictionary<Guid, (string Token, DateTime Expiry)> _tokenCache = new();

    public async Task<Delivery> CreateConsignmentAsync(TenantSettings s, Order order, CancellationToken ct = default)
    {
        var token = await GetTokenAsync(s, ct);
        var client = BuildClient(s, token);
        var payload = new
        {
            store_id = s.PathaoStoreId,
            merchant_order_id = order.OrderNumber,
            recipient_name = order.CustomerName,
            recipient_phone = order.CustomerPhone,
            recipient_address = order.DeliveryAddress,
            recipient_city = 1,
            recipient_zone = 1,
            delivery_type = 48,
            item_type = 2,
            item_quantity = order.Quantity,
            item_weight = 0.5,
            amount_to_collect = (double)order.TotalAmount,
            item_description = order.ProductName
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/aladdin/api/v1/orders", content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalApiException("Pathao", $"Consignment failed: {body}", (int)response.StatusCode);

        var result = JsonSerializer.Deserialize<PathaoOrderResponse>(body, _json);
        return new Delivery
        {
            TenantId = order.TenantId,
            PathaoConsignmentId = result?.Data?.ConsignmentId,
            TrackingCode = result?.Data?.MerchantOrderId,
            RecipientName = order.CustomerName,
            RecipientPhone = order.CustomerPhone,
            RecipientAddress = order.DeliveryAddress,
            RecipientCity = order.City,
            CollectAmount = order.TotalAmount,
            Status = DeliveryStatus.Scheduled,
            RawResponse = body,
            OrderId = order.Id
        };
    }

    public async Task<DeliveryStatusResult> GetDeliveryStatusAsync(TenantSettings s, string consignmentId, CancellationToken ct = default)
    {
        var token = await GetTokenAsync(s, ct);
        var client = BuildClient(s, token);
        var response = await client.GetAsync($"/aladdin/api/v1/orders/{consignmentId}", ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new ExternalApiException("Pathao", $"Status check failed: {body}");
        var result = JsonSerializer.Deserialize<PathaoStatusResponse>(body, _json);
        return new DeliveryStatusResult(consignmentId, result?.Data?.OrderStatus ?? "Unknown", result?.Data?.ConsignmentId, DateTime.UtcNow);
    }

    public async Task<bool> CancelConsignmentAsync(TenantSettings s, string consignmentId, CancellationToken ct = default)
    {
        var token = await GetTokenAsync(s, ct);
        var client = BuildClient(s, token);
        var response = await client.PostAsync($"/aladdin/api/v1/orders/{consignmentId}/cancel", null, ct);
        return response.IsSuccessStatusCode;
    }

    private async Task<string> GetTokenAsync(TenantSettings s, CancellationToken ct)
    {
        if (_tokenCache.TryGetValue(s.TenantId, out var cached) && DateTime.UtcNow < cached.Expiry)
            return cached.Token;

        var client = httpClientFactory.CreateClient("Pathao");
        client.BaseAddress = new Uri(s.PathaoApiBaseUrl ?? "https://api-hermes.pathao.com");

        var payload = new { client_id = s.PathaoClientId, client_secret = s.PathaoClientSecret, username = s.PathaoUsername, password = s.PathaoPassword, grant_type = "password" };
        var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/aladdin/api/v1/issue-token", content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new ExternalApiException("Pathao", $"Auth failed: {body}");

        var token = JsonSerializer.Deserialize<PathaoTokenResponse>(body, _json);
        var accessToken = token?.AccessToken ?? throw new ExternalApiException("Pathao", "No access token in response");
        _tokenCache[s.TenantId] = (accessToken, DateTime.UtcNow.AddSeconds((token?.ExpiresIn ?? 3600) - 60));
        logger.LogInformation("[Pathao] Token refreshed for tenant {TenantId}", s.TenantId);
        return accessToken;
    }

    private HttpClient BuildClient(TenantSettings s, string token)
    {
        var client = httpClientFactory.CreateClient("Pathao");
        client.BaseAddress = new Uri(s.PathaoApiBaseUrl ?? "https://api-hermes.pathao.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private record PathaoTokenResponse([property: JsonPropertyName("access_token")] string? AccessToken, [property: JsonPropertyName("expires_in")] int ExpiresIn);
    private record PathaoOrderResponse(PathaoOrderData? Data);
    private record PathaoOrderData([property: JsonPropertyName("consignment_id")] string? ConsignmentId, [property: JsonPropertyName("merchant_order_id")] string? MerchantOrderId);
    private record PathaoStatusResponse(PathaoStatusData? Data);
    private record PathaoStatusData([property: JsonPropertyName("consignment_id")] string? ConsignmentId, [property: JsonPropertyName("order_status")] string? OrderStatus);
}
