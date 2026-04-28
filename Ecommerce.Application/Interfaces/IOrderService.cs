using Ecommerce.Application.Common.Models;
using Ecommerce.Application.DTOs;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(Guid tenantId, CreateOrderDto dto, CancellationToken ct = default);
    Task<OrderResponseDto> GetOrderByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PagedResult<OrderResponseDto>> GetOrdersAsync(Guid tenantId, int page, int pageSize, OrderStatus? status, CancellationToken ct = default);
    Task<OrderResponseDto> UpdateOrderStatusAsync(Guid tenantId, Guid id, UpdateOrderStatusDto dto, CancellationToken ct = default);
    Task<OrderResponseDto> DispatchOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task DeleteOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}
