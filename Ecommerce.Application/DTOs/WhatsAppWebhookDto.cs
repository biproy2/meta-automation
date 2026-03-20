using System.Text.Json.Serialization;

namespace Ecommerce.Application.DTOs;

/// <summary>
/// Maps the exact JSON structure sent by WhatsApp Cloud API webhook.
/// [JsonPropertyName] handles snake_case → PascalCase conversion.
/// </summary>
public class WhatsAppWebhookPayload
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("entry")]
    public List<WhatsAppEntry> Entry { get; set; } = new();
}

public class WhatsAppEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("changes")]
    public List<WhatsAppChange> Changes { get; set; } = new();
}

public class WhatsAppChange
{
    [JsonPropertyName("value")]
    public WhatsAppValue Value { get; set; } = new();

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;
}

public class WhatsAppValue
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public WhatsAppMetadata? Metadata { get; set; }

    [JsonPropertyName("messages")]
    public List<WhatsAppMessage>? Messages { get; set; }

    [JsonPropertyName("statuses")]
    public List<WhatsAppStatus>? Statuses { get; set; }
}

public class WhatsAppMetadata
{
    [JsonPropertyName("display_phone_number")]
    public string DisplayPhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("phone_number_id")]
    public string PhoneNumberId { get; set; } = string.Empty;
}

public class WhatsAppMessage
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public WhatsAppText? Text { get; set; }
}

public class WhatsAppText
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

public class WhatsAppStatus
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("recipient_id")]
    public string RecipientId { get; set; } = string.Empty;
}
