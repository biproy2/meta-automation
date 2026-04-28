using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IPathaoService
{
    Task<Delivery> CreateConsignmentAsync(TenantSettings settings, Order order, CancellationToken ct = default);
    Task<DeliveryStatusResult> GetDeliveryStatusAsync(TenantSettings settings, string consignmentId, CancellationToken ct = default);
    Task<bool> CancelConsignmentAsync(TenantSettings settings, string consignmentId, CancellationToken ct = default);
}

public record DeliveryStatusResult(string ConsignmentId, string Status, string? TrackingCode, DateTime UpdatedAt);
