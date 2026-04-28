using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<(IEnumerable<Lead> Items, int Total)> GetPagedAsync(Guid tenantId, int page, int pageSize, LeadStatus? status, CancellationToken ct = default);
    Task<Lead> AddAsync(Lead lead, CancellationToken ct = default);
    Task UpdateAsync(Lead lead, CancellationToken ct = default);
}
