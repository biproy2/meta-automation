namespace Ecommerce.Infrastructure.Settings;

/// <summary>
/// Bound from appsettings.json "Pathao" section.
/// Get your credentials at: https://pathao.com/merchant → API Integration
/// </summary>
public class PathaoSettings
{
    /// <summary>Use sandbox: https://sandbox.pathao.com for testing</summary>
    public string ApiBaseUrl { get; set; } = "https://hermes.pathao.com";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>Store ID from Pathao Merchant Portal → Stores</summary>
    public string StoreId { get; set; } = string.Empty;
}
