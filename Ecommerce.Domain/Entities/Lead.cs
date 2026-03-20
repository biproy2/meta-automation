using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

/// <summary>
/// A potential customer interest captured from Messenger/WhatsApp.
/// A Lead can be converted into a confirmed Order.
/// </summary>
public class Lead : BaseEntity
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }

    /// <summary>Product or service the customer asked about</summary>
    public string ProductInterest { get; set; } = string.Empty;

    /// <summary>Raw message text received from the customer</summary>
    public string? IncomingMessage { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.New;
    public MessageChannel Source { get; set; } = MessageChannel.WhatsApp;

    /// <summary>Messenger PSID or WhatsApp number used to reply back</summary>
    public string? ChannelUserId { get; set; }
    public string? Notes { get; set; }

    // Foreign keys
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Guid? ConvertedOrderId { get; set; }
    public Order? ConvertedOrder { get; set; }
}
