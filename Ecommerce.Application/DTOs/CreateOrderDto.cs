using Ecommerce.Domain.Enums;
using FluentValidation;

namespace Ecommerce.Application.DTOs;

/// <summary>
/// Used to create a new order — either manually by admin or
/// programmatically when a lead is converted.
/// </summary>
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
    /// <summary>Messenger PSID or WhatsApp number — used to send back confirmation</summary>
    public string? ChannelUserId { get; set; }
    public string? Notes { get; set; }
    public Guid? UserId { get; set; }
}

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CustomerPhone).NotEmpty()
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Phone must be 10-15 digits, optional leading +.");
        RuleFor(x => x.DeliveryAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
        RuleFor(x => x.DeliveryCharge).GreaterThanOrEqualTo(0);
    }
}
