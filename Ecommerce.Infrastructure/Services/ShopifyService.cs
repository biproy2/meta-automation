using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Services;

public class ShopifyService(IHttpClientFactory httpClientFactory, ILogger<ShopifyService> logger) : IShopifyService
{
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public async Task<ShopifyOrderResult> CreateOrderAsync(TenantSettings s, Order order, CancellationToken ct = default)
    {
        var client = BuildClient(s);
        var payload = new
        {
            order = new
            {
                line_items = new[] { new { title = order.ProductName, quantity = order.Quantity, price = order.UnitPrice.ToString("F2") } },
                customer = new { first_name = order.CustomerName, phone = order.CustomerPhone },
                shipping_address = new { first_name = order.CustomerName, phone = order.CustomerPhone, address1 = order.DeliveryAddress, city = order.City, country = "US" },
                financial_status = "pending",
                note = $"Order from WhatsApp/Messenger. Ref: {order.OrderNumber}",
                tags = "automation"
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("orders.json", content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new ExternalApiException("Shopify", $"Order creation failed: {body}", (int)response.StatusCode);

        var result = JsonSerializer.Deserialize<ShopifyOrderResponse>(body, _json);
        logger.LogInformation("Shopify order created: #{Id}", result?.Order?.Id);
        return new ShopifyOrderResult(result?.Order?.Id.ToString() ?? "", result?.Order?.OrderNumber?.ToString() ?? "", result?.Order?.FinancialStatus ?? "pending", decimal.TryParse(result?.Order?.TotalPrice, out var p) ? p : 0, result?.Order?.Fulfillments?.FirstOrDefault()?.TrackingNumber);
    }

    public async Task<IEnumerable<ShopifyProduct>> GetProductsAsync(TenantSettings s, CancellationToken ct = default)
    {
        var client = BuildClient(s);
        var response = await client.GetAsync("products.json?limit=50&status=active", ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) throw new ExternalApiException("Shopify", $"Get products failed: {body}");
        var result = JsonSerializer.Deserialize<ShopifyProductsResponse>(body, _json);
        return result?.Products?.Select(p => new ShopifyProduct(p.Id.ToString(), p.Title ?? "", decimal.TryParse(p.Variants?.FirstOrDefault()?.Price, out var price) ? price : 0, p.Variants?.FirstOrDefault()?.InventoryQuantity ?? 0, p.Image?.Src)) ?? Enumerable.Empty<ShopifyProduct>();
    }

    public async Task<ShopifyOrderStatus> GetOrderStatusAsync(TenantSettings s, string shopifyOrderId, CancellationToken ct = default)
    {
        var client = BuildClient(s);
        var response = await client.GetAsync($"orders/{shopifyOrderId}.json", ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) throw new ExternalApiException("Shopify", $"Get order failed: {body}");
        var result = JsonSerializer.Deserialize<ShopifyOrderResponse>(body, _json);
        var fulfillment = result?.Order?.Fulfillments?.FirstOrDefault();
        return new ShopifyOrderStatus(shopifyOrderId, result?.Order?.FinancialStatus ?? "unknown", result?.Order?.FulfillmentStatus ?? "unfulfilled", fulfillment?.TrackingNumber, fulfillment?.TrackingUrl);
    }

    private HttpClient BuildClient(TenantSettings s)
    {
        var client = httpClientFactory.CreateClient("Shopify");
        client.BaseAddress = new Uri($"https://{s.ShopifyStoreUrl}/admin/api/{s.ShopifyApiVersion}/");
        client.DefaultRequestHeaders.Remove("X-Shopify-Access-Token");
        client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", s.ShopifyAccessToken);
        return client;
    }

    private record ShopifyOrderResponse([property: JsonPropertyName("order")] ShopifyOrderData? Order);
    private record ShopifyOrderData([property: JsonPropertyName("id")] long Id, [property: JsonPropertyName("order_number")] int? OrderNumber, [property: JsonPropertyName("financial_status")] string? FinancialStatus, [property: JsonPropertyName("fulfillment_status")] string? FulfillmentStatus, [property: JsonPropertyName("total_price")] string? TotalPrice, [property: JsonPropertyName("fulfillments")] List<ShopifyFulfillment>? Fulfillments);
    private record ShopifyFulfillment([property: JsonPropertyName("tracking_number")] string? TrackingNumber, [property: JsonPropertyName("tracking_url")] string? TrackingUrl);
    private record ShopifyProductsResponse([property: JsonPropertyName("products")] List<ShopifyProductData>? Products);
    private record ShopifyProductData([property: JsonPropertyName("id")] long Id, [property: JsonPropertyName("title")] string? Title, [property: JsonPropertyName("image")] ShopifyImage? Image, [property: JsonPropertyName("variants")] List<ShopifyVariant>? Variants);
    private record ShopifyImage([property: JsonPropertyName("src")] string? Src);
    private record ShopifyVariant([property: JsonPropertyName("price")] string? Price, [property: JsonPropertyName("inventory_quantity")] int InventoryQuantity);
}
