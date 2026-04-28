using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<TenantSettings?> GetSettingsAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<Tenant> AddAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task AddSettingsAsync(TenantSettings settings, CancellationToken ct = default);
    Task UpdateSettingsAsync(TenantSettings settings, CancellationToken ct = default);
    Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken ct = default);
}
