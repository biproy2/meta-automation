using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

/// <summary>
/// Multi-tenant webhook endpoints.
/// Each client gets their own webhook URL using their slug.
///
/// WhatsApp:  POST /api/webhook/{slug}/whatsapp
/// Messenger: POST /api/webhook/{slug}/messenger
/// Shopify:   POST /api/webhook/{slug}/shopify/order
///
/// The {slug} identifies WHICH client the message belongs to.
/// </summary>
[ApiController]
[Route("api/webhook")]
public class WebhookController(
    ITenantRepository tenantRepo,
    ILeadService leadService,
    IOrderService orderService) : ControllerBase
{
    // ── WhatsApp Verification (GET) ───────────────────────────
    [HttpGet("{slug}/whatsapp")]
    public async Task<IActionResult> VerifyWhatsApp(
        string slug,
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge,
        CancellationToken ct)
    {
        var settings = await GetSettingsAsync(slug, ct);
        if (settings == null) return NotFound();
        if (mode == "subscribe" && token == settings.WhatsAppWebhookToken)
            return Ok(challenge);
        return Forbid();
    }

    // ── WhatsApp Messages (POST) ──────────────────────────────
    [HttpPost("{slug}/whatsapp")]
    public async Task<IActionResult> WhatsAppWebhook(
        string slug, [FromBody] WhatsAppWebhookPayload payload, CancellationToken ct)
    {
        var tenant = await tenantRepo.GetBySlugAsync(slug, ct);
        if (tenant == null) return NotFound();

        _ = Task.Run(async () =>
        {
            try
            {
                foreach (var entry in payload.Entry)
                foreach (var change in entry.Changes)
                {
                    if (change.Value.Messages == null) continue;
                    foreach (var msg in change.Value.Messages)
                    {
                        if (msg.Type != "text" || msg.Text?.Body == null) continue;
                        await leadService.CreateLeadAsync(tenant.Id, new CreateLeadDto
                        {
                            CustomerName = $"WA {msg.From}",
                            CustomerPhone = msg.From,
                            ProductInterest = "Inquiry",
                            IncomingMessage = msg.Text.Body,
                            Source = MessageChannel.WhatsApp,
                            ChannelUserId = msg.From
                        }, ct);
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[WA Webhook Error] {slug}: {ex.Message}"); }
        }, ct);

        return Ok();
    }

    // ── Messenger Verification (GET) ─────────────────────────
    [HttpGet("{slug}/messenger")]
    public async Task<IActionResult> VerifyMessenger(
        string slug,
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge,
        CancellationToken ct)
    {
        var settings = await GetSettingsAsync(slug, ct);
        if (settings == null) return NotFound();
        if (mode == "subscribe" && token == settings.MessengerWebhookToken)
            return Ok(challenge);
        return Forbid();
    }

    // ── Messenger Messages (POST) ─────────────────────────────
    [HttpPost("{slug}/messenger")]
    public async Task<IActionResult> MessengerWebhook(
        string slug, [FromBody] MessengerWebhookPayload payload, CancellationToken ct)
    {
        if (payload.Object != "page") return Ok();
        var tenant = await tenantRepo.GetBySlugAsync(slug, ct);
        if (tenant == null) return NotFound();

        _ = Task.Run(async () =>
        {
            try
            {
                foreach (var entry in payload.Entry)
                foreach (var evt in entry.Messaging)
                {
                    var text = evt.Message?.Text ?? evt.Postback?.Payload;
                    if (string.IsNullOrEmpty(text)) continue;
                    await leadService.CreateLeadAsync(tenant.Id, new CreateLeadDto
                    {
                        CustomerName = $"Messenger {evt.Sender.Id}",
                        CustomerPhone = "N/A",
                        ProductInterest = "Messenger Inquiry",
                        IncomingMessage = text,
                        Source = MessageChannel.Messenger,
                        ChannelUserId = evt.Sender.Id
                    }, ct);
                }
            }
            catch (Exception ex) { Console.WriteLine($"[MS Webhook Error] {slug}: {ex.Message}"); }
        }, ct);

        return Ok();
    }

    // ── Shopify Order Webhook (POST) ──────────────────────────
    [HttpPost("{slug}/shopify/order")]
    public async Task<IActionResult> ShopifyOrderWebhook(
        string slug, [FromBody] ShopifyWebhookOrderDto payload, CancellationToken ct)
    {
        var tenant = await tenantRepo.GetBySlugAsync(slug, ct);
        if (tenant == null) return NotFound();

        _ = Task.Run(async () =>
        {
            try
            {
                var customerName = payload.Customer != null
                    ? $"{payload.Customer.FirstName} {payload.Customer.LastName}".Trim()
                    : payload.ShippingAddress?.Name ?? "Shopify Customer";

                var customerPhone = payload.Customer?.Phone
                    ?? payload.ShippingAddress?.Phone ?? "N/A";

                await orderService.CreateOrderAsync(tenant.Id, new CreateOrderDto
                {
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    DeliveryAddress = payload.ShippingAddress?.Address1 ?? string.Empty,
                    City = payload.ShippingAddress?.City ?? string.Empty,
                    ProductName = payload.LineItems.FirstOrDefault()?.Title ?? "Shopify Product",
                    Quantity = payload.LineItems.FirstOrDefault()?.Quantity ?? 1,
                    UnitPrice = decimal.TryParse(payload.LineItems.FirstOrDefault()?.Price, out var p) ? p : 0,
                    DeliveryCharge = 0,
                    OrderSource = MessageChannel.Direct,
                    Notes = $"Shopify #{payload.OrderNumber}"
                }, ct);
            }
            catch (Exception ex) { Console.WriteLine($"[Shopify Webhook Error] {slug}: {ex.Message}"); }
        }, ct);

        return Ok();
    }

    // ── Privacy Policy ────────────────────────────────────────
    [HttpGet("/privacy")]
    [Produces("text/html")]
    public ContentResult Privacy() => Content(@"<!DOCTYPE html>
<html><head><title>Privacy Policy</title></head>
<body style='font-family:Arial;max-width:800px;margin:40px auto;padding:20px'>
<h1>Privacy Policy</h1><p>Last updated: April 2026</p>
<h2>Data Collection</h2><p>We collect WhatsApp and Messenger messages to process customer orders.</p>
<h2>Data Use</h2><p>Messages are used only for order processing and delivery updates.</p>
<h2>Data Storage</h2><p>Data is stored securely and never shared with third parties.</p>
<h2>Contact</h2><p>Email: support@meta-automation.onrender.com</p>
</body></html>", "text/html");

    private async Task<Domain.Entities.TenantSettings?> GetSettingsAsync(string slug, CancellationToken ct)
    {
        var tenant = await tenantRepo.GetBySlugAsync(slug, ct);
        return tenant?.Settings;
    }
}
