namespace Ecommerce.Domain.Enums;

/// <summary>
/// Full lifecycle of an order from first contact to completion.
/// Stored as INT in the database.
/// </summary>
public enum OrderStatus
{
    Pending    = 0,   // Just received via WhatsApp/Messenger
    Confirmed  = 1,   // Agent confirmed with customer by phone
    Processing = 2,   // Being packed / prepared in warehouse
    Dispatched = 3,   // Handed to Pathao courier
    InTransit  = 4,   // Pathao has picked up the parcel
    Delivered  = 5,   // Successfully delivered to customer
    Cancelled  = 6,   // Cancelled before dispatch
    Returned   = 7    // Returned by customer after delivery
}
