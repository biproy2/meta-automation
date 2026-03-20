namespace Ecommerce.Domain.Interfaces;

/// <summary>
/// Contract for Facebook Messenger Send API.
/// </summary>
public interface IMessengerService
{
    Task SendTextMessageAsync(string recipientPsid, string message, CancellationToken ct = default);
    Task SendQuickReplyAsync(string recipientPsid, string text, IEnumerable<string> options, CancellationToken ct = default);
    Task SendProductCardAsync(string recipientPsid, string title, string subtitle, string imageUrl, string orderUrl, CancellationToken ct = default);
    Task SendOrderConfirmationAsync(string recipientPsid, string orderNumber, string productName, decimal total, CancellationToken ct = default);
    Task SendTypingIndicatorAsync(string recipientPsid, CancellationToken ct = default);
}
