using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

/// <summary>
/// All API credentials for one tenant.
/// Each tenant has their own WhatsApp, Messenger, Pathao, Shopify credentials.
/// </summary>
public class TenantSettings : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // WhatsApp
    public string? WhatsAppPhoneNumberId { get; set; }
    public string? WhatsAppAccessToken { get; set; }
    public string? WhatsAppWebhookToken { get; set; }

    // Messenger
    public string? MessengerPageToken { get; set; }
    public string? MessengerAppSecret { get; set; }
    public string? MessengerWebhookToken { get; set; }

    // Pathao
    public string? PathaoClientId { get; set; }
    public string? PathaoClientSecret { get; set; }
    public string? PathaoUsername { get; set; }
    public string? PathaoPassword { get; set; }
    public string? PathaoStoreId { get; set; }
    public string? PathaoApiBaseUrl { get; set; }

    // Shopify
    public string? ShopifyStoreUrl { get; set; }
    public string? ShopifyAccessToken { get; set; }
    public string? ShopifyWebhookSecret { get; set; }
    public string ShopifyApiVersion { get; set; } = "2024-01";

    // Delivery
    public DeliveryProvider DeliveryProvider { get; set; } = DeliveryProvider.Manual;
}
