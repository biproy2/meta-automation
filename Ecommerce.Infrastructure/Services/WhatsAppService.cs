using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Services;

/// <summary>
/// Calls WhatsApp Business Cloud API.
/// Docs: https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages
/// All requests go to: POST /v19.0/{phone-number-id}/messages
/// </summary>
public class WhatsAppService(
    IHttpClientFactory httpClientFactory,
    IOptions<WhatsAppSettings> options,
    ILogger<WhatsAppService> logger) : IWhatsAppService
{
    private readonly WhatsAppSettings _cfg = options.Value;
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task SendTextMessageAsync(string toPhone, string message, CancellationToken ct = default)
    {
        var body = new
        {
            messaging_product = "whatsapp",
            to = toPhone,
            type = "text",
            text = new { body = message }
        };
        await PostAsync(body, ct);
        logger.LogInformation("WhatsApp text → {Phone}", Mask(toPhone));
    }

    public async Task SendOrderConfirmationAsync(string toPhone, string orderNumber,
        string productName, decimal total, CancellationToken ct = default)
    {
        var msg = $"🎉 *Order Confirmed!*\n\n" +
                  $"📦 Order No: *{orderNumber}*\n" +
                  $"🛍️ Product: {productName}\n" +
                  $"💰 Total: ৳{total:N0}\n\n" +
                  "We'll notify you when dispatched. Thank you! 🙏";
        await SendTextMessageAsync(toPhone, msg, ct);
    }

    public async Task SendDeliveryUpdateAsync(string toPhone, string orderNumber,
        string trackingCode, string status, CancellationToken ct = default)
    {
        var msg = $"🚚 *Delivery Update*\n\n" +
                  $"📦 Order: *{orderNumber}*\n" +
                  $"📍 Status: {status}\n" +
                  $"🔍 Tracking: *{trackingCode}*\n\n" +
                  "Track your parcel via the Pathao app.";
        await SendTextMessageAsync(toPhone, msg, ct);
    }

    public async Task SendTemplateMessageAsync(string toPhone, string templateName,
        string languageCode, object[] parameters, CancellationToken ct = default)
    {
        var body = new
        {
            messaging_product = "whatsapp",
            to = toPhone,
            type = "template",
            template = new
            {
                name = templateName,
                language = new { code = languageCode },
                components = parameters.Length == 0 ? null : new[]
                {
                    new
                    {
                        type = "body",
                        parameters = parameters.Select(p => new { type = "text", text = p?.ToString() }).ToArray()
                    }
                }
            }
        };
        await PostAsync(body, ct);
    }

    public async Task MarkMessageAsReadAsync(string messageId, CancellationToken ct = default)
    {
        var body = new { messaging_product = "whatsapp", status = "read", message_id = messageId };
        await PostAsync(body, ct);
    }

    private async Task PostAsync(object payload, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("WhatsApp");
        var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/{_cfg.PhoneNumberId}/messages", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("WhatsApp API {Status}: {Error}", response.StatusCode, err);
            throw new ExternalApiException("WhatsApp", $"{response.StatusCode}: {err}", (int)response.StatusCode);
        }
    }

    private static string Mask(string phone) =>
        phone.Length > 6 ? phone[..4] + "****" + phone[^4..] : "****";
}
