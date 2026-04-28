using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IMessengerService
{
    Task SendTextMessageAsync(TenantSettings settings, string recipientPsid, string message, CancellationToken ct = default);
    Task SendOrderConfirmationAsync(TenantSettings settings, string recipientPsid, string orderNumber, string productName, decimal total, CancellationToken ct = default);
    Task SendTypingIndicatorAsync(TenantSettings settings, string recipientPsid, CancellationToken ct = default);
}
