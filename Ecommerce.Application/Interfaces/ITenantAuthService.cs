using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Interfaces;

public interface ITenantAuthService
{
    Task<TenantAuthResponseDto> RegisterAsync(RegisterTenantDto dto, CancellationToken ct = default);
    Task<TenantAuthResponseDto> LoginAsync(LoginTenantDto dto, CancellationToken ct = default);
    Task<TenantSettingsResponseDto> GetSettingsAsync(Guid tenantId, string baseUrl, CancellationToken ct = default);
    Task<TenantSettingsResponseDto> UpdateSettingsAsync(Guid tenantId, UpdateTenantSettingsDto dto, string baseUrl, CancellationToken ct = default);
}
