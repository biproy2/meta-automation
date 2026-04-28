using System.Security.Claims;
using Ecommerce.Application.Common.Models;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

/// <summary>
/// Tenant self-service: register, login, manage API settings.
/// Route: /api/tenant
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TenantController(ITenantAuthService tenantAuthService) : ControllerBase
{
    private string BaseUrl => $"{Request.Scheme}://{Request.Host}";
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId") ?? Guid.Empty.ToString());

    /// <summary>Register a new client account (self-service)</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<TenantAuthResponseDto>), 201)]
    public async Task<IActionResult> Register([FromBody] RegisterTenantDto dto, CancellationToken ct)
    {
        var result = await tenantAuthService.RegisterAsync(dto, ct);
        return Created(string.Empty, ApiResponse<TenantAuthResponseDto>.Ok(result, "Account created! Save your access token."));
    }

    /// <summary>Login and get access token</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<TenantAuthResponseDto>), 200)]
    public async Task<IActionResult> Login([FromBody] LoginTenantDto dto, CancellationToken ct)
    {
        var result = await tenantAuthService.LoginAsync(dto, ct);
        return Ok(ApiResponse<TenantAuthResponseDto>.Ok(result));
    }

    /// <summary>Get your webhook URLs and settings status</summary>
    [HttpGet("settings")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TenantSettingsResponseDto>), 200)]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var result = await tenantAuthService.GetSettingsAsync(TenantId, BaseUrl, ct);
        return Ok(ApiResponse<TenantSettingsResponseDto>.Ok(result));
    }

    /// <summary>
    /// Update your WhatsApp, Messenger, Pathao, Shopify credentials.
    /// After updating, you'll get your webhook URLs to paste into Meta/Shopify.
    /// </summary>
    [HttpPut("settings")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TenantSettingsResponseDto>), 200)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateTenantSettingsDto dto, CancellationToken ct)
    {
        var result = await tenantAuthService.UpdateSettingsAsync(TenantId, dto, BaseUrl, ct);
        return Ok(ApiResponse<TenantSettingsResponseDto>.Ok(result, "Settings updated! Use the webhook URLs below."));
    }
}
