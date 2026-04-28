using System.Security.Claims;
using Ecommerce.Application.Common.Models;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class OrderController(IOrderService orderService) : ControllerBase
{
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId") ?? Guid.Empty.ToString());

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] OrderStatus? status = null, CancellationToken ct = default)
        => Ok(ApiResponse<PagedResult<OrderResponseDto>>.Ok(await orderService.GetOrdersAsync(TenantId, page, pageSize, status, ct)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<OrderResponseDto>.Ok(await orderService.GetOrderByIdAsync(TenantId, id, ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        var order = await orderService.CreateOrderAsync(TenantId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, ApiResponse<OrderResponseDto>.Ok(order, "Order created."));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto, CancellationToken ct)
        => Ok(ApiResponse<OrderResponseDto>.Ok(await orderService.UpdateOrderStatusAsync(TenantId, id, dto, ct)));

    [HttpPost("{id:guid}/dispatch")]
    public async Task<IActionResult> Dispatch(Guid id, CancellationToken ct)
        => Ok(ApiResponse<OrderResponseDto>.Ok(await orderService.DispatchOrderAsync(TenantId, id, ct), "Order dispatched."));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await orderService.DeleteOrderAsync(TenantId, id, ct);
        return NoContent();
    }
}
