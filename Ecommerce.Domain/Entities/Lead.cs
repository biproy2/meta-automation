using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class Lead : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string ProductInterest { get; set; } = string.Empty;
    public string? IncomingMessage { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public MessageChannel Source { get; set; } = MessageChannel.WhatsApp;
    public string? ChannelUserId { get; set; }
    public string? Notes { get; set; }
    public Guid? ConvertedOrderId { get; set; }
    public Order? ConvertedOrder { get; set; }
}
