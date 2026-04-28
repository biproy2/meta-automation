using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Models;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Services;

public class LeadService(
    ILeadRepository leadRepo,
    ITenantRepository tenantRepo,
    IOrderService orderService,
    IWhatsAppService whatsApp,
    IMessengerService messenger,
    ILogger<LeadService> logger) : ILeadService
{
    public async Task<LeadResponseDto> CreateLeadAsync(Guid tenantId, CreateLeadDto dto, CancellationToken ct = default)
    {
        var lead = new Lead
        {
            TenantId = tenantId,
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            CustomerEmail = dto.CustomerEmail,
            ProductInterest = dto.ProductInterest,
            IncomingMessage = dto.IncomingMessage,
            Status = LeadStatus.New,
            Source = dto.Source,
            ChannelUserId = dto.ChannelUserId
        };

        await leadRepo.AddAsync(lead, ct);
        logger.LogInformation("[{TenantId}] Lead created: {Phone}", tenantId, dto.CustomerPhone);

        await SendAutoReplyAsync(tenantId, lead, ct);
        return Map(lead);
    }

    public async Task<LeadResponseDto> GetLeadByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var lead = await leadRepo.GetByIdAsync(id, tenantId, ct)
            ?? throw new NotFoundException(nameof(Lead), id);
        return Map(lead);
    }

    public async Task<PagedResult<LeadResponseDto>> GetLeadsAsync(Guid tenantId, int page, int pageSize, LeadStatus? status, CancellationToken ct = default)
    {
        var (items, total) = await leadRepo.GetPagedAsync(tenantId, page, pageSize, status, ct);
        return new PagedResult<LeadResponseDto> { Items = items.Select(Map), TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<LeadResponseDto> UpdateLeadStatusAsync(Guid tenantId, Guid id, LeadStatus newStatus, CancellationToken ct = default)
    {
        var lead = await leadRepo.GetByIdAsync(id, tenantId, ct)
            ?? throw new NotFoundException(nameof(Lead), id);
        lead.Status = newStatus;
        lead.UpdatedAt = DateTime.UtcNow;
        await leadRepo.UpdateAsync(lead, ct);
        return Map(lead);
    }

    public async Task<OrderResponseDto> ConvertLeadToOrderAsync(Guid tenantId, Guid leadId, CreateOrderDto orderDto, CancellationToken ct = default)
    {
        var lead = await leadRepo.GetByIdAsync(leadId, tenantId, ct)
            ?? throw new NotFoundException(nameof(Lead), leadId);

        if (lead.Status == LeadStatus.Converted)
            throw new InvalidOperationException("Lead already converted.");

        orderDto.ChannelUserId ??= lead.ChannelUserId;
        orderDto.OrderSource = lead.Source;

        var order = await orderService.CreateOrderAsync(tenantId, orderDto, ct);

        lead.Status = LeadStatus.Converted;
        lead.ConvertedOrderId = order.Id;
        lead.UpdatedAt = DateTime.UtcNow;
        await leadRepo.UpdateAsync(lead, ct);

        return order;
    }

    private async Task SendAutoReplyAsync(Guid tenantId, Lead lead, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(lead.ChannelUserId)) return;
        var ref_ = lead.Id.ToString()[..8].ToUpper();
        var reply = $"Hi {lead.CustomerName}! 👋 Thanks for your interest in *{lead.ProductInterest}*.\nOur team will contact you shortly. Ref: {ref_}";
        try
        {
            var settings = await tenantRepo.GetSettingsAsync(tenantId, ct);
            if (settings == null) return;
            if (lead.Source == MessageChannel.WhatsApp && !string.IsNullOrEmpty(settings.WhatsAppAccessToken))
                await whatsApp.SendTextMessageAsync(settings, lead.ChannelUserId, reply, ct);
            else if (lead.Source == MessageChannel.Messenger && !string.IsNullOrEmpty(settings.MessengerPageToken))
                await messenger.SendTextMessageAsync(settings, lead.ChannelUserId, reply, ct);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Auto-reply failed for lead {LeadId}", lead.Id); }
    }

    private static LeadResponseDto Map(Lead l) => new()
    {
        Id = l.Id, CustomerName = l.CustomerName, CustomerPhone = l.CustomerPhone,
        ProductInterest = l.ProductInterest, IncomingMessage = l.IncomingMessage,
        Status = l.Status, StatusName = l.Status.ToString(), Source = l.Source,
        CreatedAt = l.CreatedAt, ConvertedOrderId = l.ConvertedOrderId
    };
}
