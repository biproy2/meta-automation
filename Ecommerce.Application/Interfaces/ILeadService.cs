using Ecommerce.Application.Common.Models;
using Ecommerce.Application.DTOs;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Interfaces;

public interface ILeadService
{
    Task<LeadResponseDto> CreateLeadAsync(LeadDto dto, CancellationToken ct = default);
    Task<LeadResponseDto> GetLeadByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<LeadResponseDto>> GetLeadsAsync(int page, int pageSize, LeadStatus? status, CancellationToken ct = default);
    Task<LeadResponseDto> UpdateLeadStatusAsync(Guid id, LeadStatus newStatus, CancellationToken ct = default);
    /// <summary>Convert a qualified lead directly into a confirmed order</summary>
    Task<OrderResponseDto> ConvertLeadToOrderAsync(Guid leadId, CreateOrderDto orderDto, CancellationToken ct = default);
}
