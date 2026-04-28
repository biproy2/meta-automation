using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Persistence.Repositories;

public class OrderRepository(ApplicationDbContext db) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => await db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId, ct);

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, Guid tenantId, CancellationToken ct = default)
        => await db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.TenantId == tenantId, ct);

    public async Task<Order?> GetWithDeliveryAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => await db.Orders.Include(o => o.Delivery).FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId, ct);

    public async Task<(IEnumerable<Order> Items, int Total)> GetPagedAsync(Guid tenantId, int page, int pageSize, OrderStatus? status, CancellationToken ct = default)
    {
        var query = db.Orders.Include(o => o.Delivery).Where(o => o.TenantId == tenantId).AsQueryable();
        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(o => o.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Order> AddAsync(Order order, CancellationToken ct = default)
    {
        await db.Orders.AddAsync(order, ct);
        await db.SaveChangesAsync(ct);
        return order;
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        db.Orders.Update(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Order order, CancellationToken ct = default)
    {
        order.IsDeleted = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task<string> GenerateOrderNumberAsync(Guid tenantId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var prefix = $"ORD-{today:yyyyMMdd}-";
        var count = await db.Orders.IgnoreQueryFilters().CountAsync(o => o.TenantId == tenantId && o.OrderNumber.StartsWith(prefix), ct);
        return $"{prefix}{(count + 1):D4}";
    }
}
