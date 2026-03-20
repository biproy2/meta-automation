using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IOrderRepository.
/// Include() = SQL JOIN to load related tables.
/// Skip/Take = SQL OFFSET/FETCH for pagination.
/// </summary>
public class OrderRepository(ApplicationDbContext db) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
        => await db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task<Order?> GetWithDeliveryAsync(Guid id, CancellationToken ct = default)
        => await db.Orders
            .Include(o => o.Delivery)   // LEFT JOIN Deliveries
            .Include(o => o.User)       // LEFT JOIN Users
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<(IEnumerable<Order> Items, int Total)> GetPagedAsync(
        int page, int pageSize, OrderStatus? status, CancellationToken ct = default)
    {
        var query = db.Orders.Include(o => o.Delivery).AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

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
        // Soft delete: set IsDeleted = true, don't actually remove the row
        order.IsDeleted = true;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<string> GenerateOrderNumberAsync(CancellationToken ct = default)
    {
        // Format: ORD-20240115-0042 (date + sequential daily counter)
        var today = DateTime.UtcNow.Date;
        var todayPrefix = $"ORD-{today:yyyyMMdd}-";

        var todayCount = await db.Orders
            .IgnoreQueryFilters()   // Include soft-deleted for accurate count
            .CountAsync(o => o.OrderNumber.StartsWith(todayPrefix), ct);

        return $"{todayPrefix}{(todayCount + 1):D4}";
    }
}
