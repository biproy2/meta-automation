using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IWhatsAppService
{
    Task SendTextMessageAsync(TenantSettings settings, string toPhone, string message, CancellationToken ct = default);
    Task SendOrderConfirmationAsync(TenantSettings settings, string toPhone, string orderNumber, string productName, decimal total, CancellationToken ct = default);
    Task SendDeliveryUpdateAsync(TenantSettings settings, string toPhone, string orderNumber, string trackingCode, CancellationToken ct = default);
    Task MarkMessageAsReadAsync(TenantSettings settings, string messageId, CancellationToken ct = default);
}
