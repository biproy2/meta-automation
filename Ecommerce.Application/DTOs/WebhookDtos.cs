using System.Text.Json.Serialization;

namespace Ecommerce.Application.DTOs;

// ── WhatsApp ──────────────────────────────────────────────────
public class WhatsAppWebhookPayload
{
    [JsonPropertyName("object")] public string Object { get; set; } = string.Empty;
    [JsonPropertyName("entry")] public List<WhatsAppEntry> Entry { get; set; } = new();
}
public class WhatsAppEntry
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("changes")] public List<WhatsAppChange> Changes { get; set; } = new();
}
public class WhatsAppChange
{
    [JsonPropertyName("value")] public WhatsAppValue Value { get; set; } = new();
    [JsonPropertyName("field")] public string Field { get; set; } = string.Empty;
}
public class WhatsAppValue
{
    [JsonPropertyName("messaging_product")] public string MessagingProduct { get; set; } = string.Empty;
    [JsonPropertyName("messages")] public List<WhatsAppMessage>? Messages { get; set; }
}
public class WhatsAppMessage
{
    [JsonPropertyName("from")] public string From { get; set; } = string.Empty;
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    [JsonPropertyName("text")] public WhatsAppText? Text { get; set; }
}
public class WhatsAppText
{
    [JsonPropertyName("body")] public string Body { get; set; } = string.Empty;
}

// ── Messenger ─────────────────────────────────────────────────
public class MessengerWebhookPayload
{
    [JsonPropertyName("object")] public string Object { get; set; } = string.Empty;
    [JsonPropertyName("entry")] public List<MessengerEntry> Entry { get; set; } = new();
}
public class MessengerEntry
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("messaging")] public List<MessengerEvent> Messaging { get; set; } = new();
}
public class MessengerEvent
{
    [JsonPropertyName("sender")] public MessengerSender Sender { get; set; } = new();
    [JsonPropertyName("message")] public MessengerMessage? Message { get; set; }
    [JsonPropertyName("postback")] public MessengerPostback? Postback { get; set; }
}
public class MessengerSender { [JsonPropertyName("id")] public string Id { get; set; } = string.Empty; }
public class MessengerMessage { [JsonPropertyName("text")] public string? Text { get; set; } }
public class MessengerPostback { [JsonPropertyName("payload")] public string Payload { get; set; } = string.Empty; }

// ── Shopify ───────────────────────────────────────────────────
public class ShopifyWebhookOrderDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("order_number")] public int OrderNumber { get; set; }
    [JsonPropertyName("total_price")] public string? TotalPrice { get; set; }
    [JsonPropertyName("line_items")] public List<ShopifyLineItem> LineItems { get; set; } = new();
    [JsonPropertyName("shipping_address")] public ShopifyAddress? ShippingAddress { get; set; }
    [JsonPropertyName("customer")] public ShopifyCustomer? Customer { get; set; }
}
public class ShopifyLineItem
{
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("quantity")] public int Quantity { get; set; }
    [JsonPropertyName("price")] public string Price { get; set; } = string.Empty;
}
public class ShopifyAddress
{
    [JsonPropertyName("address1")] public string? Address1 { get; set; }
    [JsonPropertyName("city")] public string? City { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("country")] public string? Country { get; set; }
}
public class ShopifyCustomer
{
    [JsonPropertyName("first_name")] public string? FirstName { get; set; }
    [JsonPropertyName("last_name")] public string? LastName { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
}
