using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Services;

/// <summary>
/// Pathao Courier API integration.
/// Docs: https://developer.pathao.com
/// Auth flow: POST /aladdin/api/v1/issue-token → returns access_token → use in Authorization: Bearer header
/// </summary>
public class PathaoService(
    IHttpClientFactory httpClientFactory,
    IOptions<PathaoSettings> options,
    ILogger<PathaoService> logger) : IPathaoService
{
    private readonly PathaoSettings _cfg = options.Value;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // Token cache — Pathao tokens last ~24 hours
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public async Task<Delivery> CreateConsignmentAsync(Order order, CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var client = BuildClient();

        var payload = new
        {
            store_id = _cfg.StoreId,
            merchant_order_id = order.OrderNumber,
            recipient_name = order.CustomerName,
            recipient_phone = order.CustomerPhone,
            recipient_address = order.DeliveryAddress,
            recipient_city = 1,        // Dhaka city ID — use GetCitiesAsync to map names to IDs
            recipient_zone = 1,        // Default zone — use GetZonesAsync in production
            delivery_type = 48,        // 48-hour delivery
            item_type = 2,             // 2 = Parcel
            special_instruction = order.Notes ?? "",
            item_quantity = order.Quantity,
            item_weight = 0.5,
            amount_to_collect = (double)order.TotalAmount,
            item_description = order.ProductName
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/aladdin/api/v1/orders", content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Pathao CreateConsignment {Status}: {Body}", response.StatusCode, body);
            throw new ExternalApiException("Pathao", $"Consignment failed: {body}", (int)response.StatusCode);
        }

        var result = JsonSerializer.Deserialize<PathaoOrderResponse>(body, _json);
        logger.LogInformation("Pathao consignment created: {Id}", result?.Data?.ConsignmentId);

        return new Delivery
        {
            PathaoConsignmentId = result?.Data?.ConsignmentId,
            TrackingCode = result?.Data?.MerchantOrderId,
            RecipientName = order.CustomerName,
            RecipientPhone = order.CustomerPhone,
            RecipientAddress = order.DeliveryAddress,
            RecipientCity = order.City,
            CollectAmount = order.TotalAmount,
            Status = DeliveryStatus.Scheduled,
            PathaoRawResponse = body,
            OrderId = order.Id
        };
    }

    public async Task<DeliveryStatusResult> GetDeliveryStatusAsync(string consignmentId, CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var client = BuildClient();
        var response = await client.GetAsync($"/aladdin/api/v1/orders/{consignmentId}", ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalApiException("Pathao", $"Status check failed: {body}", (int)response.StatusCode);

        var result = JsonSerializer.Deserialize<PathaoStatusResponse>(body, _json);
        return new DeliveryStatusResult(consignmentId, result?.Data?.OrderStatus ?? "Unknown", result?.Data?.ConsignmentId, DateTime.UtcNow);
    }

    public async Task<IEnumerable<PathaoCity>> GetCitiesAsync(CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var response = await BuildClient().GetAsync("/aladdin/api/v1/countries/1/cities", ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<PathaoCitiesResponse>(body, _json);
        return result?.Data?.Data?.Select(c => new PathaoCity(c.CityId, c.CityName)) ?? Enumerable.Empty<PathaoCity>();
    }

    public async Task<IEnumerable<PathaoZone>> GetZonesAsync(int cityId, CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var response = await BuildClient().GetAsync($"/aladdin/api/v1/cities/{cityId}/zones", ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<PathaoZonesResponse>(body, _json);
        return result?.Data?.Data?.Select(z => new PathaoZone(z.ZoneId, z.ZoneName, cityId)) ?? Enumerable.Empty<PathaoZone>();
    }

    public async Task<bool> CancelConsignmentAsync(string consignmentId, CancellationToken ct = default)
    {
        await EnsureTokenAsync(ct);
        var response = await BuildClient().PostAsync($"/aladdin/api/v1/orders/{consignmentId}/cancel", null, ct);
        return response.IsSuccessStatusCode;
    }

    private async Task EnsureTokenAsync(CancellationToken ct)
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry) return;

        var client = httpClientFactory.CreateClient("Pathao");
        var payload = new
        {
            client_id = _cfg.ClientId,
            client_secret = _cfg.ClientSecret,
            username = _cfg.Username,
            password = _cfg.Password,
            grant_type = "password"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/aladdin/api/v1/issue-token", content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new ExternalApiException("Pathao", $"Token request failed: {body}");

        var token = JsonSerializer.Deserialize<PathaoTokenResponse>(body, _json);
        _accessToken = token?.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds((token?.ExpiresIn ?? 3600) - 60);
        logger.LogInformation("Pathao token refreshed, valid until {Expiry}", _tokenExpiry);
    }

    private HttpClient BuildClient()
    {
        var client = httpClientFactory.CreateClient("Pathao");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        return client;
    }

    // ── Private response record types ─────────────────────────
    private record PathaoTokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private record PathaoOrderResponse(PathaoOrderData? Data);
    private record PathaoOrderData(
        [property: JsonPropertyName("consignment_id")] string? ConsignmentId,
        [property: JsonPropertyName("merchant_order_id")] string? MerchantOrderId);

    private record PathaoStatusResponse(PathaoStatusData? Data);
    private record PathaoStatusData(
        [property: JsonPropertyName("consignment_id")] string? ConsignmentId,
        [property: JsonPropertyName("order_status")] string? OrderStatus);

    private record PathaoCitiesResponse(PathaoCitiesData? Data);
    private record PathaoCitiesData(List<PathaoCityItem>? Data);
    private record PathaoCityItem(
        [property: JsonPropertyName("city_id")] int CityId,
        [property: JsonPropertyName("city_name")] string CityName);

    private record PathaoZonesResponse(PathaoZonesData? Data);
    private record PathaoZonesData(List<PathaoZoneItem>? Data);
    private record PathaoZoneItem(
        [property: JsonPropertyName("zone_id")] int ZoneId,
        [property: JsonPropertyName("zone_name")] string ZoneName);
}
