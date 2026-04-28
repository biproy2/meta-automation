using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class Order : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DeliveryCharge { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public MessageChannel OrderSource { get; set; } = MessageChannel.WhatsApp;
    public string? ChannelUserId { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? ShopifyOrderId { get; set; }

    public Delivery? Delivery { get; set; }
}
