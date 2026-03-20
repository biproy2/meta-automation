using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

/// <summary>
/// A Pathao courier consignment linked to an Order.
/// Created when the order is dispatched via Pathao API.
/// </summary>
public class Delivery : BaseEntity
{
    /// <summary>Pathao consignment ID returned after booking</summary>
    public string? PathaoConsignmentId { get; set; }

    /// <summary>Tracking code shared with the customer</summary>
    public string? TrackingCode { get; set; }

    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string RecipientAddress { get; set; } = string.Empty;
    public string RecipientCity { get; set; } = string.Empty;

    public decimal CollectAmount { get; set; }  // Cash on Delivery amount
    public decimal DeliveryFee { get; set; }
    public decimal Weight { get; set; } = 0.5m; // KG

    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

    public string? PathaoZoneId { get; set; }
    public string? PathaoAreaId { get; set; }

    public DateTime? PickupTime { get; set; }
    public DateTime? DeliveredAt { get; set; }

    /// <summary>Full JSON response from Pathao API — kept for debugging</summary>
    public string? PathaoRawResponse { get; set; }

    // Foreign key
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
