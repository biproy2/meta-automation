using System.Text;
using System.Text.Json;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Services;

/// <summary>
/// Calls Facebook Messenger Send API.
/// Docs: https://developers.facebook.com/docs/messenger-platform/send-messages
/// All requests go to: POST /v19.0/me/messages?access_token={token}
/// </summary>
public class MessengerService(
    IHttpClientFactory httpClientFactory,
    IOptions<MessengerSettings> options,
    ILogger<MessengerService> logger) : IMessengerService
{
    private readonly MessengerSettings _cfg = options.Value;
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public async Task SendTextMessageAsync(string recipientPsid, string message, CancellationToken ct = default)
    {
        var body = new
        {
            recipient = new { id = recipientPsid },
            message = new { text = message }
        };
        await PostAsync(body, ct);
        logger.LogInformation("Messenger text → PSID {Psid}", Mask(recipientPsid));
    }

    public async Task SendQuickReplyAsync(string recipientPsid, string text,
        IEnumerable<string> options, CancellationToken ct = default)
    {
        var body = new
        {
            recipient = new { id = recipientPsid },
            message = new
            {
                text,
                quick_replies = options.Select(o => new
                {
                    content_type = "text",
                    title = o,
                    payload = o.ToUpper().Replace(" ", "_")
                }).ToArray()
            }
        };
        await PostAsync(body, ct);
    }

    public async Task SendProductCardAsync(string recipientPsid, string title, string subtitle,
        string imageUrl, string orderUrl, CancellationToken ct = default)
    {
        var body = new
        {
            recipient = new { id = recipientPsid },
            message = new
            {
                attachment = new
                {
                    type = "template",
                    payload = new
                    {
                        template_type = "generic",
                        elements = new[]
                        {
                            new
                            {
                                title,
                                subtitle,
                                image_url = imageUrl,
                                buttons = new[] { new { type = "web_url", url = orderUrl, title = "Order Now" } }
                            }
                        }
                    }
                }
            }
        };
        await PostAsync(body, ct);
    }

    public async Task SendOrderConfirmationAsync(string recipientPsid, string orderNumber,
        string productName, decimal total, CancellationToken ct = default)
    {
        var msg = $"🎉 Order Confirmed!\n📦 {orderNumber}\n🛍️ {productName}\n💰 ৳{total:N0}\nThank you! 🙏";
        await SendTextMessageAsync(recipientPsid, msg, ct);
    }

    public async Task SendTypingIndicatorAsync(string recipientPsid, CancellationToken ct = default)
    {
        var body = new { recipient = new { id = recipientPsid }, sender_action = "typing_on" };
        await PostAsync(body, ct);
        await Task.Delay(1200, ct);
    }

    private async Task PostAsync(object payload, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Messenger");
        var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/me/messages?access_token={_cfg.PageAccessToken}", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Messenger API {Status}: {Error}", response.StatusCode, err);
            throw new ExternalApiException("Messenger", $"{response.StatusCode}: {err}", (int)response.StatusCode);
        }
    }

    private static string Mask(string psid) =>
        psid.Length > 8 ? psid[..4] + "****" + psid[^4..] : "****";
}
