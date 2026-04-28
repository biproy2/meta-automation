using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Application.Services;

public class TenantAuthService(
    ITenantRepository tenantRepo,
    IConfiguration config,
    ILogger<TenantAuthService> logger) : ITenantAuthService
{
    public async Task<TenantAuthResponseDto> RegisterAsync(RegisterTenantDto dto, CancellationToken ct = default)
    {
        if (await tenantRepo.EmailExistsAsync(dto.OwnerEmail.ToLower(), ct))
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Email", "Email already registered.")]);

        var slug = GenerateSlug(dto.BusinessName);
        var counter = 1;
        var originalSlug = slug;
        while (await tenantRepo.SlugExistsAsync(slug, ct))
            slug = $"{originalSlug}-{counter++}";

        var tenant = new Tenant
        {
            Name = dto.BusinessName,
            Slug = slug,
            OwnerName = dto.OwnerName,
            OwnerEmail = dto.OwnerEmail.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            Plan = TenantPlan.Free
        };

        await tenantRepo.AddAsync(tenant, ct);

        // Create empty settings
        await tenantRepo.AddSettingsAsync(new TenantSettings { TenantId = tenant.Id }, ct);

        logger.LogInformation("New tenant registered: {Name} ({Slug})", tenant.Name, tenant.Slug);
        return BuildAuthResponse(tenant);
    }

    public async Task<TenantAuthResponseDto> LoginAsync(LoginTenantDto dto, CancellationToken ct = default)
    {
        var tenant = await tenantRepo.GetByEmailAsync(dto.Email.ToLower(), ct)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!tenant.IsActive) throw new UnauthorizedException("Account is deactivated.");
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, tenant.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        return BuildAuthResponse(tenant);
    }

    public async Task<TenantSettingsResponseDto> GetSettingsAsync(Guid tenantId, string baseUrl, CancellationToken ct = default)
    {
        var tenant = await tenantRepo.GetByIdAsync(tenantId, ct)
            ?? throw new NotFoundException(nameof(Tenant), tenantId);
        var settings = await tenantRepo.GetSettingsAsync(tenantId, ct);
        return MapSettingsToDto(tenant, settings, baseUrl);
    }

    public async Task<TenantSettingsResponseDto> UpdateSettingsAsync(Guid tenantId, UpdateTenantSettingsDto dto, string baseUrl, CancellationToken ct = default)
    {
        var tenant = await tenantRepo.GetByIdAsync(tenantId, ct)
            ?? throw new NotFoundException(nameof(Tenant), tenantId);

        var settings = await tenantRepo.GetSettingsAsync(tenantId, ct)
            ?? new TenantSettings { TenantId = tenantId };

        settings.WhatsAppPhoneNumberId = dto.WhatsAppPhoneNumberId;
        settings.WhatsAppAccessToken = dto.WhatsAppAccessToken;
        settings.WhatsAppWebhookToken = dto.WhatsAppWebhookToken;
        settings.MessengerPageToken = dto.MessengerPageToken;
        settings.MessengerAppSecret = dto.MessengerAppSecret;
        settings.MessengerWebhookToken = dto.MessengerWebhookToken;
        settings.PathaoClientId = dto.PathaoClientId;
        settings.PathaoClientSecret = dto.PathaoClientSecret;
        settings.PathaoUsername = dto.PathaoUsername;
        settings.PathaoPassword = dto.PathaoPassword;
        settings.PathaoStoreId = dto.PathaoStoreId;
        settings.PathaoApiBaseUrl = dto.PathaoApiBaseUrl;
        settings.ShopifyStoreUrl = dto.ShopifyStoreUrl;
        settings.ShopifyAccessToken = dto.ShopifyAccessToken;
        settings.ShopifyWebhookSecret = dto.ShopifyWebhookSecret;
        settings.DeliveryProvider = dto.DeliveryProvider;

        if (settings.Id == Guid.Empty)
            await tenantRepo.AddSettingsAsync(settings, ct);
        else
            await tenantRepo.UpdateSettingsAsync(settings, ct);

        return MapSettingsToDto(tenant, settings, baseUrl);
    }

    private TenantAuthResponseDto BuildAuthResponse(Tenant tenant)
    {
        var token = GenerateToken(tenant);
        return new TenantAuthResponseDto
        {
            AccessToken = token,
            Tenant = MapToDto(tenant)
        };
    }

    private string GenerateToken(Tenant tenant)
    {
        var key = Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]
            ?? "DefaultSecretKey-Change-This-In-Production-32chars!");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, tenant.Id.ToString()),
            new Claim(ClaimTypes.Email, tenant.OwnerEmail),
            new Claim(ClaimTypes.Name, tenant.Name),
            new Claim("slug", tenant.Slug),
            new Claim("tenantId", tenant.Id.ToString())
        };
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSlug(string name) =>
        System.Text.RegularExpressions.Regex.Replace(name.ToLower().Trim(), @"[^a-z0-9]+", "-").Trim('-');

    private static TenantResponseDto MapToDto(Tenant t) => new()
    {
        Id = t.Id, Name = t.Name, Slug = t.Slug,
        OwnerEmail = t.OwnerEmail, IsActive = t.IsActive,
        Plan = t.Plan, PlanName = t.Plan.ToString(), CreatedAt = t.CreatedAt
    };

    private static TenantSettingsResponseDto MapSettingsToDto(Tenant tenant, TenantSettings? s, string baseUrl) => new()
    {
        HasWhatsApp = !string.IsNullOrEmpty(s?.WhatsAppPhoneNumberId),
        HasMessenger = !string.IsNullOrEmpty(s?.MessengerPageToken),
        HasPathao = !string.IsNullOrEmpty(s?.PathaoClientId),
        HasShopify = !string.IsNullOrEmpty(s?.ShopifyStoreUrl),
        DeliveryProvider = s?.DeliveryProvider ?? DeliveryProvider.Manual,
        DeliveryProviderName = (s?.DeliveryProvider ?? DeliveryProvider.Manual).ToString(),
        WhatsAppWebhookUrl = $"{baseUrl}/api/webhook/{tenant.Slug}/whatsapp",
        MessengerWebhookUrl = $"{baseUrl}/api/webhook/{tenant.Slug}/messenger",
        ShopifyWebhookUrl = $"{baseUrl}/api/webhook/{tenant.Slug}/shopify/order"
    };
}
