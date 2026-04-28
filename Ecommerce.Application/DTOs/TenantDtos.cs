using Ecommerce.Domain.Enums;
using FluentValidation;

namespace Ecommerce.Application.DTOs;

/// <summary>Client self-registration request</summary>
public class RegisterTenantDto
{
    public string BusinessName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RegisterTenantDtoValidator : AbstractValidator<RegisterTenantDto>
{
    public RegisterTenantDtoValidator()
    {
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OwnerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Must contain uppercase.")
            .Matches("[0-9]").WithMessage("Must contain digit.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}

public class LoginTenantDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class TenantResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public TenantPlan Plan { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TenantAuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public TenantResponseDto Tenant { get; set; } = new();
}

public class UpdateTenantSettingsDto
{
    // WhatsApp
    public string? WhatsAppPhoneNumberId { get; set; }
    public string? WhatsAppAccessToken { get; set; }
    public string? WhatsAppWebhookToken { get; set; }

    // Messenger
    public string? MessengerPageToken { get; set; }
    public string? MessengerAppSecret { get; set; }
    public string? MessengerWebhookToken { get; set; }

    // Pathao
    public string? PathaoClientId { get; set; }
    public string? PathaoClientSecret { get; set; }
    public string? PathaoUsername { get; set; }
    public string? PathaoPassword { get; set; }
    public string? PathaoStoreId { get; set; }
    public string? PathaoApiBaseUrl { get; set; }

    // Shopify
    public string? ShopifyStoreUrl { get; set; }
    public string? ShopifyAccessToken { get; set; }
    public string? ShopifyWebhookSecret { get; set; }

    // Delivery
    public DeliveryProvider DeliveryProvider { get; set; } = DeliveryProvider.Manual;
}

public class TenantSettingsResponseDto
{
    public bool HasWhatsApp { get; set; }
    public bool HasMessenger { get; set; }
    public bool HasPathao { get; set; }
    public bool HasShopify { get; set; }
    public DeliveryProvider DeliveryProvider { get; set; }
    public string DeliveryProviderName { get; set; } = string.Empty;

    /// <summary>Webhook URLs for this tenant</summary>
    public string WhatsAppWebhookUrl { get; set; } = string.Empty;
    public string MessengerWebhookUrl { get; set; } = string.Empty;
    public string ShopifyWebhookUrl { get; set; } = string.Empty;
}
