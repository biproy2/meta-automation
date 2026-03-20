namespace Ecommerce.Domain.Interfaces;

/// <summary>
/// Contract for WhatsApp Business Cloud API.
/// Infrastructure layer provides the real HTTP implementation.
/// Application layer uses this interface — never imports HttpClient directly.
/// </summary>
public interface IWhatsAppService
{
    Task SendTextMessageAsync(string toPhone, string message, CancellationToken ct = default);
    Task SendOrderConfirmationAsync(string toPhone, string orderNumber, string productName, decimal total, CancellationToken ct = default);
    Task SendDeliveryUpdateAsync(string toPhone, string orderNumber, string trackingCode, string status, CancellationToken ct = default);
    Task SendTemplateMessageAsync(string toPhone, string templateName, string languageCode, object[] parameters, CancellationToken ct = default);
    Task MarkMessageAsReadAsync(string messageId, CancellationToken ct = default);
}
