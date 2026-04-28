using Ecommerce.Domain.Enums;
using FluentValidation;

namespace Ecommerce.Application.DTOs;

public class CreateLeadDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string ProductInterest { get; set; } = string.Empty;
    public string? IncomingMessage { get; set; }
    public MessageChannel Source { get; set; } = MessageChannel.WhatsApp;
    public string? ChannelUserId { get; set; }
}

public class CreateLeadDtoValidator : AbstractValidator<CreateLeadDto>
{
    public CreateLeadDtoValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CustomerPhone).NotEmpty();
        RuleFor(x => x.ProductInterest).NotEmpty().MaximumLength(500);
    }
}

public class LeadResponseDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ProductInterest { get; set; } = string.Empty;
    public string? IncomingMessage { get; set; }
    public LeadStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public MessageChannel Source { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ConvertedOrderId { get; set; }
}
