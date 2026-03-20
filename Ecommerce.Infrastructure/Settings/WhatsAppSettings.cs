namespace Ecommerce.Infrastructure.Settings;

/// <summary>
/// Bound from appsettings.json "WhatsApp" section.
/// Registration: services.Configure&lt;WhatsAppSettings&gt;(config.GetSection("WhatsApp"));
/// </summary>
public class WhatsAppSettings
{
    /// <summary>Phone Number ID from Meta Developer Portal → WhatsApp → API Setup</summary>
    public string PhoneNumberId { get; set; } = string.Empty;

    /// <summary>System User Access Token (permanent) from Meta Business Suite</summary>
    public string AccessToken { get; set; } = string.Empty;

    public string ApiBaseUrl { get; set; } = "https://graph.facebook.com/v19.0";

    /// <summary>Any secret string you choose — must match what you set in Meta webhook config</summary>
    public string WebhookVerifyToken { get; set; } = string.Empty;
}
