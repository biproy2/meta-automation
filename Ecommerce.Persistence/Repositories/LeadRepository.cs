using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Persistence.Repositories;

public class LeadRepository(ApplicationDbContext db) : ILeadRepository
{
    public async Task<Lead?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => await db.Leads.FirstOrDefaultAsync(l => l.Id == id && l.TenantId == tenantId, ct);

    public async Task<(IEnumerable<Lead> Items, int Total)> GetPagedAsync(Guid tenantId, int page, int pageSize, LeadStatus? status, CancellationToken ct = default)
    {
        var query = db.Leads.Where(l => l.TenantId == tenantId).AsQueryable();
        if (status.HasValue) query = query.Where(l => l.Status == status.Value);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(l => l.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Lead> AddAsync(Lead lead, CancellationToken ct = default)
    {
        await db.Leads.AddAsync(lead, ct);
        await db.SaveChangesAsync(ct);
        return lead;
    }

    public async Task UpdateAsync(Lead lead, CancellationToken ct = default)
    {
        db.Leads.Update(lead);
        await db.SaveChangesAsync(ct);
    }
}
