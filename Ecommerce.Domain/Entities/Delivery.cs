using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class Delivery : BaseEntity
{
    public Guid TenantId { get; set; }
    public string? PathaoConsignmentId { get; set; }
    public string? TrackingCode { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string RecipientAddress { get; set; } = string.Empty;
    public string RecipientCity { get; set; } = string.Empty;
    public decimal CollectAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Weight { get; set; } = 0.5m;
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public DateTime? PickupTime { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? RawResponse { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
