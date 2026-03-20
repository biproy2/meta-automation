using System.Text.Json.Serialization;

namespace Ecommerce.Application.DTOs;

/// <summary>Maps the JSON structure sent by Facebook Messenger webhook.</summary>
public class MessengerWebhookPayload
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("entry")]
    public List<MessengerEntry> Entry { get; set; } = new();
}

public class MessengerEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("messaging")]
    public List<MessengerEvent> Messaging { get; set; } = new();
}

public class MessengerEvent
{
    [JsonPropertyName("sender")]
    public MessengerSender Sender { get; set; } = new();

    [JsonPropertyName("recipient")]
    public MessengerRecipient Recipient { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("message")]
    public MessengerMessage? Message { get; set; }

    [JsonPropertyName("postback")]
    public MessengerPostback? Postback { get; set; }
}

public class MessengerSender
{
    /// <summary>This is the PSID — unique per user per Facebook Page</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class MessengerRecipient
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class MessengerMessage
{
    [JsonPropertyName("mid")]
    public string Mid { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class MessengerPostback
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;
}
