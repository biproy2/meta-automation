using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Ecommerce.API.Controllers;

/// <summary>
/// Receives webhook events from Facebook Messenger.
///
/// ── HOW WEBHOOKS WORK ────────────────────────────────────────
/// 1. Register in Meta Developer Portal → Messenger → Webhooks
/// 2. Subscribe to: messages, messaging_postbacks events
/// 3. Meta verifies with GET (challenge) then sends POSTs on events
/// ─────────────────────────────────────────────────────────────
///
/// Route: /api/messengerwebhook
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MessengerWebhookController(
    ILeadService leadService,
    IOptions<MessengerSettings> settings) : ControllerBase
{
    // ── Webhook Verification (GET) ────────────────────────────
    [HttpGet]
    public IActionResult Verify(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        if (mode == "subscribe" && token == settings.Value.WebhookVerifyToken)
            return Ok(challenge);
        return Forbid();
    }

    // ── Receive Events (POST) ─────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Receive(
        [FromBody] MessengerWebhookPayload payload,
        CancellationToken ct)
    {
        if (payload.Object != "page") return Ok();

        _ = Task.Run(async () =>
        {
            try { await ProcessPayloadAsync(payload, ct); }
            catch (Exception ex) { Console.WriteLine($"[Messenger Webhook Error] {ex.Message}"); }
        }, ct);

        return Ok();
    }

    private async Task ProcessPayloadAsync(MessengerWebhookPayload payload, CancellationToken ct)
    {
        foreach (var entry in payload.Entry)
        foreach (var evt in entry.Messaging)
        {
            var psid = evt.Sender.Id;
            var text = evt.Message?.Text;
            var postback = evt.Postback?.Payload;

            var messageText = text ?? postback ?? string.Empty;
            if (string.IsNullOrEmpty(messageText)) continue;

            await leadService.CreateLeadAsync(new LeadDto
            {
                CustomerName = $"Messenger {psid}",  // Real name needs Graph API call
                CustomerPhone = "N/A",
                ProductInterest = "Messenger Inquiry",
                IncomingMessage = messageText,
                Source = MessageChannel.Messenger,
                ChannelUserId = psid
            }, ct);
        }
    }
}
