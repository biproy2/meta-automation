using Ecommerce.Domain.Enums;
using FluentValidation;

namespace Ecommerce.Application.DTOs;

public class UpdateOrderStatusDto
{
    public OrderStatus NewStatus { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Invalid order status value.");
    }
}
