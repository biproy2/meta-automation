using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Persistence.Repositories;

public class TenantRepository(ApplicationDbContext db) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Tenants.Include(t => t.Settings).FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Tenant?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await db.Tenants.FirstOrDefaultAsync(t => t.OwnerEmail == email, ct);

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await db.Tenants.Include(t => t.Settings).FirstOrDefaultAsync(t => t.Slug == slug, ct);

    public async Task<TenantSettings?> GetSettingsAsync(Guid tenantId, CancellationToken ct = default)
        => await db.TenantSettings.FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
        => await db.Tenants.AnyAsync(t => t.Slug == slug, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await db.Tenants.AnyAsync(t => t.OwnerEmail == email, ct);

    public async Task<Tenant> AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        await db.Tenants.AddAsync(tenant, ct);
        await db.SaveChangesAsync(ct);
        return tenant;
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        db.Tenants.Update(tenant);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddSettingsAsync(TenantSettings settings, CancellationToken ct = default)
    {
        await db.TenantSettings.AddAsync(settings, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateSettingsAsync(TenantSettings settings, CancellationToken ct = default)
    {
        db.TenantSettings.Update(settings);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken ct = default)
        => await db.Tenants.Include(t => t.Settings).ToListAsync(ct);
}
