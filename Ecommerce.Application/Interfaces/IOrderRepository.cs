using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<Order?> GetWithDeliveryAsync(Guid id, CancellationToken ct = default);
    Task<(IEnumerable<Order> Items, int Total)> GetPagedAsync(int page, int pageSize, OrderStatus? status, CancellationToken ct = default);
    Task<Order> AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task DeleteAsync(Order order, CancellationToken ct = default);
    Task<string> GenerateOrderNumberAsync(CancellationToken ct = default);
}
