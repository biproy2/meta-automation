using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

/// <summary>
/// A customer who contacted us via WhatsApp or Messenger.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }

    /// <summary>Facebook Messenger Page-Scoped User ID (PSID)</summary>
    public string? MessengerPsid { get; set; }

    /// <summary>WhatsApp number in E.164 format e.g. +8801XXXXXXXXX</summary>
    public string? WhatsAppNumber { get; set; }

    public MessageChannel PreferredChannel { get; set; } = MessageChannel.WhatsApp;
    public bool IsActive { get; set; } = true;

    // Navigation properties (EF Core builds the SQL JOINs from these)
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}
