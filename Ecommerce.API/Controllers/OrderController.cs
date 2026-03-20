using Ecommerce.Application.Common.Models;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

/// <summary>
/// Order management endpoints.
/// Base route: /api/orders
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    /// <summary>Get paginated list of orders, optionally filtered by status</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderResponseDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] OrderStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await orderService.GetOrdersAsync(page, pageSize, status, ct);
        return Ok(ApiResponse<PagedResult<OrderResponseDto>>.Ok(result));
    }

    /// <summary>Get a single order by its GUID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await orderService.GetOrderByIdAsync(id, ct);
        return Ok(ApiResponse<OrderResponseDto>.Ok(order));
    }

    /// <summary>Get a single order by order number e.g. ORD-20240115-0001</summary>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), 200)]
    public async Task<IActionResult> GetByNumber(string orderNumber, CancellationToken ct)
    {
        var order = await orderService.GetOrderByNumberAsync(orderNumber, ct);
        return Ok(ApiResponse<OrderResponseDto>.Ok(order));
    }

    /// <summary>Create a new order (sends WhatsApp/Messenger confirmation automatically)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        var order = await orderService.CreateOrderAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            ApiResponse<OrderResponseDto>.Ok(order, "Order created successfully."));
    }

    /// <summary>Update order status (e.g. Pending → Confirmed → Processing)</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), 200)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto, CancellationToken ct)
    {
        var order = await orderService.UpdateOrderStatusAsync(id, dto, ct);
        return Ok(ApiResponse<OrderResponseDto>.Ok(order, "Status updated."));
    }

    /// <summary>Dispatch order via Pathao courier (auto-books Pathao consignment)</summary>
    [HttpPost("{id:guid}/dispatch")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Dispatch(Guid id, CancellationToken ct)
    {
        var order = await orderService.DispatchOrderAsync(id, ct);
        return Ok(ApiResponse<OrderResponseDto>.Ok(order, "Order dispatched via Pathao."));
    }

    /// <summary>Soft-delete an order</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await orderService.DeleteOrderAsync(id, ct);
        return NoContent();
    }
}
