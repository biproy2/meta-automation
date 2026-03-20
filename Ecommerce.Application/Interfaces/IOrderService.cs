using Ecommerce.Application.Common.Models;
using Ecommerce.Application.DTOs;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct = default);
    Task<OrderResponseDto> GetOrderByIdAsync(Guid id, CancellationToken ct = default);
    Task<OrderResponseDto> GetOrderByNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<PagedResult<OrderResponseDto>> GetOrdersAsync(int page, int pageSize, OrderStatus? status, CancellationToken ct = default);
    Task<OrderResponseDto> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto dto, CancellationToken ct = default);
    Task<OrderResponseDto> DispatchOrderAsync(Guid id, CancellationToken ct = default);
    Task DeleteOrderAsync(Guid id, CancellationToken ct = default);
}
