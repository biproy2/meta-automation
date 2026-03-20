namespace Ecommerce.Infrastructure.Settings;

/// <summary>
/// Bound from appsettings.json "Messenger" section.
/// </summary>
public class MessengerSettings
{
    /// <summary>Facebook Page Access Token from Meta Developer Portal</summary>
    public string PageAccessToken { get; set; } = string.Empty;

    /// <summary>App Secret — used to verify X-Hub-Signature-256 webhook signatures</summary>
    public string AppSecret { get; set; } = string.Empty;

    /// <summary>Any secret string — must match what you set in Meta webhook config</summary>
    public string WebhookVerifyToken { get; set; } = string.Empty;

    public string ApiBaseUrl { get; set; } = "https://graph.facebook.com/v19.0";
}
