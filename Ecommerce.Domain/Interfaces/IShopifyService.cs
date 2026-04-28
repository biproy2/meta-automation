using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IShopifyService
{
    Task<ShopifyOrderResult> CreateOrderAsync(TenantSettings settings, Order order, CancellationToken ct = default);
    Task<IEnumerable<ShopifyProduct>> GetProductsAsync(TenantSettings settings, CancellationToken ct = default);
    Task<ShopifyOrderStatus> GetOrderStatusAsync(TenantSettings settings, string shopifyOrderId, CancellationToken ct = default);
}

public record ShopifyOrderResult(string ShopifyOrderId, string OrderNumber, string Status, decimal TotalPrice, string? TrackingNumber);
public record ShopifyProduct(string ProductId, string Title, decimal Price, int Inventory, string? ImageUrl);
public record ShopifyOrderStatus(string ShopifyOrderId, string Status, string FulfillmentStatus, string? TrackingNumber, string? TrackingUrl);
