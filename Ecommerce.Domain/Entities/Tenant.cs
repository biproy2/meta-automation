using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

/// <summary>
/// A tenant = one client/business using your automation service.
/// Each tenant has their own WhatsApp, Shopify, Pathao credentials.
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>Business name e.g. "Gadgetry"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-friendly slug e.g. "gadgetry" — used in webhook URLs</summary>
    public string Slug { get; set; } = string.Empty;

    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public TenantPlan Plan { get; set; } = TenantPlan.Free;

    // Navigation
    public TenantSettings? Settings { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}
