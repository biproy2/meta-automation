using System.Text;
using System.Text.Json;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Services;

/// <summary>
/// Calls WhatsApp Cloud API using per-tenant credentials.
/// Each tenant has their own PhoneNumberId and AccessToken.
/// </summary>
public class WhatsAppService(IHttpClientFactory httpClientFactory, ILogger<WhatsAppService> logger) : IWhatsAppService
{
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task SendTextMessageAsync(TenantSettings s, string toPhone, string message, CancellationToken ct = default)
    {
        var body = new { messaging_product = "whatsapp", to = toPhone, type = "text", text = new { body = message } };
        await PostAsync(s, body, ct);
        logger.LogInformation("WhatsApp sent to {Phone}", Mask(toPhone));
    }

    public async Task SendOrderConfirmationAsync(TenantSettings s, string toPhone, string orderNumber, string productName, decimal total, CancellationToken ct = default)
    {
        var msg = $"🎉 *Order Confirmed!*\n📦 {orderNumber}\n🛍️ {productName}\n💰 {total:N0}\nThank you! 🙏";
        await SendTextMessageAsync(s, toPhone, msg, ct);
    }

    public async Task SendDeliveryUpdateAsync(TenantSettings s, string toPhone, string orderNumber, string trackingCode, CancellationToken ct = default)
    {
        var msg = $"🚚 *Dispatched!*\n📦 {orderNumber}\n🔍 Track: *{trackingCode}*";
        await SendTextMessageAsync(s, toPhone, msg, ct);
    }

    public async Task MarkMessageAsReadAsync(TenantSettings s, string messageId, CancellationToken ct = default)
    {
        var body = new { messaging_product = "whatsapp", status = "read", message_id = messageId };
        await PostAsync(s, body, ct);
    }

    private async Task PostAsync(TenantSettings s, object payload, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("WhatsApp");
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {s.WhatsAppAccessToken}");
        var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/{s.WhatsAppPhoneNumberId}/messages", content, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new ExternalApiException("WhatsApp", $"{response.StatusCode}: {err}", (int)response.StatusCode);
        }
    }

    private static string Mask(string p) => p.Length > 6 ? p[..4] + "****" + p[^4..] : "****";
}
