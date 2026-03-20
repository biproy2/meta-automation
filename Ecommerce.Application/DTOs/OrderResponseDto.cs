using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs;

/// <summary>Full order detail returned by GET /api/orders/{id}</summary>
public class OrderResponseDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DeliveryCharge { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public MessageChannel OrderSource { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DeliveryResponseDto? Delivery { get; set; }
}

public class DeliveryResponseDto
{
    public Guid Id { get; set; }
    public string? PathaoConsignmentId { get; set; }
    public string? TrackingCode { get; set; }
    public DeliveryStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? PickupTime { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
