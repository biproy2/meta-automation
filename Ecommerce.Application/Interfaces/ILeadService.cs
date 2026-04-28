using Ecommerce.Application.Common.Models;
using Ecommerce.Application.DTOs;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Interfaces;

public interface ILeadService
{
    Task<LeadResponseDto> CreateLeadAsync(Guid tenantId, CreateLeadDto dto, CancellationToken ct = default);
    Task<LeadResponseDto> GetLeadByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<PagedResult<LeadResponseDto>> GetLeadsAsync(Guid tenantId, int page, int pageSize, LeadStatus? status, CancellationToken ct = default);
    Task<LeadResponseDto> UpdateLeadStatusAsync(Guid tenantId, Guid id, LeadStatus newStatus, CancellationToken ct = default);
    Task<OrderResponseDto> ConvertLeadToOrderAsync(Guid tenantId, Guid leadId, CreateOrderDto orderDto, CancellationToken ct = default);
}
