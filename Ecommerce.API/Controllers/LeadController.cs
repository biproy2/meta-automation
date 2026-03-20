using Ecommerce.Application.Common.Models;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LeadController(ILeadService leadService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] LeadStatus? status = null,
        CancellationToken ct = default)
    {
        var result = await leadService.GetLeadsAsync(page, pageSize, status, ct);
        return Ok(ApiResponse<PagedResult<LeadResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LeadResponseDto>.Ok(await leadService.GetLeadByIdAsync(id, ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LeadDto dto, CancellationToken ct)
    {
        var lead = await leadService.CreateLeadAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = lead.Id },
            ApiResponse<LeadResponseDto>.Ok(lead));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] LeadStatus newStatus, CancellationToken ct)
    {
        var lead = await leadService.UpdateLeadStatusAsync(id, newStatus, ct);
        return Ok(ApiResponse<LeadResponseDto>.Ok(lead));
    }

    /// <summary>Convert a lead into a confirmed order in one step</summary>
    [HttpPost("{id:guid}/convert")]
    public async Task<IActionResult> Convert(Guid id, [FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        var order = await leadService.ConvertLeadToOrderAsync(id, dto, ct);
        return Ok(ApiResponse<OrderResponseDto>.Ok(order, "Lead converted to order."));
    }
}
