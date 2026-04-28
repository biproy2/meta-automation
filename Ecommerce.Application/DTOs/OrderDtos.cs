using Ecommerce.Domain.Enums;
using FluentValidation;

namespace Ecommerce.Application.DTOs;

public class CreateOrderDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DeliveryCharge { get; set; }
    public MessageChannel OrderSource { get; set; } = MessageChannel.WhatsApp;
    public string? ChannelUserId { get; set; }
    public string? Notes { get; set; }
}

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CustomerPhone).NotEmpty();
        RuleFor(x => x.DeliveryAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
        RuleFor(x => x.DeliveryCharge).GreaterThanOrEqualTo(0);
    }
}

public class UpdateOrderStatusDto
{
    public OrderStatus NewStatus { get; set; }
    public string? Notes { get; set; }
}

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
    public string? ShopifyOrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DeliveryResponseDto? Delivery { get; set; }
}

public class DeliveryResponseDto
{
    public Guid Id { get; set; }
    public string? TrackingCode { get; set; }
    public DeliveryStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? DeliveredAt { get; set; }
}
