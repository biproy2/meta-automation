using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface ITenantService
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TenantSettings?> GetSettingsAsync(Guid tenantId, CancellationToken ct = default);
}
