using System.Text;
using System.Text.Json;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Services;

public class MessengerService(IHttpClientFactory httpClientFactory, ILogger<MessengerService> logger) : IMessengerService
{
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public async Task SendTextMessageAsync(TenantSettings s, string recipientPsid, string message, CancellationToken ct = default)
    {
        var body = new { recipient = new { id = recipientPsid }, message = new { text = message } };
        await PostAsync(s, body, ct);
        logger.LogInformation("Messenger sent to PSID {Psid}", Mask(recipientPsid));
    }

    public async Task SendOrderConfirmationAsync(TenantSettings s, string recipientPsid, string orderNumber, string productName, decimal total, CancellationToken ct = default)
    {
        var msg = $"🎉 Order Confirmed!\n📦 {orderNumber}\n🛍️ {productName}\n💰 {total:N0}\nThank you! 🙏";
        await SendTextMessageAsync(s, recipientPsid, msg, ct);
    }

    public async Task SendTypingIndicatorAsync(TenantSettings s, string recipientPsid, CancellationToken ct = default)
    {
        var body = new { recipient = new { id = recipientPsid }, sender_action = "typing_on" };
        await PostAsync(s, body, ct);
    }

    private async Task PostAsync(TenantSettings s, object payload, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Messenger");
        var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        var response = await client.PostAsync($"/me/messages?access_token={s.MessengerPageToken}", content, ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new ExternalApiException("Messenger", $"{response.StatusCode}: {err}", (int)response.StatusCode);
        }
    }

    private static string Mask(string p) => p.Length > 8 ? p[..4] + "****" + p[^4..] : "****";
}
