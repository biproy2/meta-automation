namespace Ecommerce.Domain.Enums;

public enum LeadStatus
{
    New       = 0,
    Contacted = 1,
    Interested = 2,
    Converted = 3,  // Turned into an Order
    Lost      = 4
}
