using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Ecommerce.API.Controllers;

/// <summary>
/// Receives webhook events from WhatsApp Cloud API.
///
/// ── HOW WEBHOOKS WORK ────────────────────────────────────────
/// 1. You register this URL in Meta Developer Portal → WhatsApp → Configuration
/// 2. Meta sends a GET request to verify the endpoint (challenge handshake)
/// 3. After verification, every customer message triggers a POST request here
/// ─────────────────────────────────────────────────────────────
///
/// Route: /api/whatsappwebhook
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WhatsAppWebhookController(
    ILeadService leadService,
    IOptions<WhatsAppSettings> settings) : ControllerBase
{
    // ── STEP 1: Webhook Verification (GET) ────────────────────
    // Meta calls this once when you set up the webhook.
    // You must return the hub.challenge value to prove you own the endpoint.
    [HttpGet]
    public IActionResult Verify(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        if (mode == "subscribe" && token == settings.Value.WebhookVerifyToken)
            return Ok(challenge);   // ← return the challenge as plain text

        return Forbid();
    }

    // ── STEP 2: Receive Messages (POST) ──────────────────────
    // Every customer message arrives here as a JSON payload.
    [HttpPost]
    public async Task<IActionResult> Receive(
        [FromBody] WhatsAppWebhookPayload payload,
        CancellationToken ct)
    {
        // WhatsApp expects a quick 200 OK — process async
        _ = Task.Run(async () =>
        {
            try { await ProcessPayloadAsync(payload, ct); }
            catch (Exception ex)
            {
                // Log but never let this crash the webhook response
                Console.WriteLine($"[WhatsApp Webhook Error] {ex.Message}");
            }
        }, ct);

        return Ok(); // Must respond 200 within 20 seconds or Meta retries
    }

    private async Task ProcessPayloadAsync(WhatsAppWebhookPayload payload, CancellationToken ct)
    {
        foreach (var entry in payload.Entry)
        foreach (var change in entry.Changes)
        {
            var messages = change.Value.Messages;
            if (messages is null) continue;

            foreach (var msg in messages)
            {
                if (msg.Type != "text" || msg.Text?.Body is null) continue;

                // Create a lead for every inbound text message
                await leadService.CreateLeadAsync(new LeadDto
                {
                    CustomerName = $"WhatsApp {msg.From}",  // Real name requires Contacts API
                    CustomerPhone = msg.From,
                    ProductInterest = "Inquiry",             // NLP parsing can improve this
                    IncomingMessage = msg.Text.Body,
                    Source = MessageChannel.WhatsApp,
                    ChannelUserId = msg.From
                }, ct);
            }
        }
    }
}
