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
public class LeadController(ILeadService leadService) : ControllerBase
{
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId") ?? Guid.Empty.ToString());

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] LeadStatus? status = null, CancellationToken ct = default)
        => Ok(ApiResponse<PagedResult<LeadResponseDto>>.Ok(await leadService.GetLeadsAsync(TenantId, page, pageSize, status, ct)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(ApiResponse<LeadResponseDto>.Ok(await leadService.GetLeadByIdAsync(TenantId, id, ct)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeadDto dto, CancellationToken ct)
    {
        var lead = await leadService.CreateLeadAsync(TenantId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = lead.Id }, ApiResponse<LeadResponseDto>.Ok(lead));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] LeadStatus newStatus, CancellationToken ct)
        => Ok(ApiResponse<LeadResponseDto>.Ok(await leadService.UpdateLeadStatusAsync(TenantId, id, newStatus, ct)));

    [HttpPost("{id:guid}/convert")]
    public async Task<IActionResult> Convert(Guid id, [FromBody] CreateOrderDto dto, CancellationToken ct)
        => Ok(ApiResponse<OrderResponseDto>.Ok(await leadService.ConvertLeadToOrderAsync(TenantId, id, dto, ct), "Lead converted to order."));
}
