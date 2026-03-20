using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

/// <summary>
/// An ecommerce order. Created manually or from a converted Lead.
/// </summary>
public class Order : BaseEntity
{
    /// <summary>Human-readable order number e.g. ORD-20240101-0001</summary>
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

    /// <summary>Messenger PSID or WhatsApp number — used to send confirmations</summary>
    public string? ChannelUserId { get; set; }

    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Foreign keys
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Delivery? Delivery { get; set; }
}
