using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

/// <summary>
/// Contract for Pathao Courier API.
/// </summary>
public interface IPathaoService
{
    Task<Delivery> CreateConsignmentAsync(Order order, CancellationToken ct = default);
    Task<DeliveryStatusResult> GetDeliveryStatusAsync(string consignmentId, CancellationToken ct = default);
    Task<IEnumerable<PathaoCity>> GetCitiesAsync(CancellationToken ct = default);
    Task<IEnumerable<PathaoZone>> GetZonesAsync(int cityId, CancellationToken ct = default);
    Task<bool> CancelConsignmentAsync(string consignmentId, CancellationToken ct = default);
}

public record DeliveryStatusResult(string ConsignmentId, string Status, string? TrackingCode, DateTime UpdatedAt);
public record PathaoCity(int CityId, string CityName);
public record PathaoZone(int ZoneId, string ZoneName, int CityId);
