using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Models;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Services;

/// <summary>
/// Manages Leads — inbound messages from WhatsApp/Messenger that might become orders.
/// </summary>
public class LeadService(
    ILeadRepository leadRepo,
    IOrderService orderService,
    IWhatsAppService whatsApp,
    IMessengerService messenger,
    ILogger<LeadService> logger) : ILeadService
{
    public async Task<LeadResponseDto> CreateLeadAsync(LeadDto dto, CancellationToken ct = default)
    {
        var lead = new Lead
        {
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            CustomerEmail = dto.CustomerEmail,
            ProductInterest = dto.ProductInterest,
            IncomingMessage = dto.IncomingMessage,
            Status = LeadStatus.New,
            Source = dto.Source,
            ChannelUserId = dto.ChannelUserId,
            Notes = dto.Notes
        };

        await leadRepo.AddAsync(lead, ct);
        logger.LogInformation("Lead created: {Phone} from {Source}", dto.CustomerPhone, dto.Source);

        // Auto-reply to the customer immediately
        await SendAutoReplyAsync(lead, ct);
        return Map(lead);
    }

    public async Task<LeadResponseDto> GetLeadByIdAsync(Guid id, CancellationToken ct = default)
    {
        var lead = await leadRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Lead), id);
        return Map(lead);
    }

    public async Task<PagedResult<LeadResponseDto>> GetLeadsAsync(
        int page, int pageSize, LeadStatus? status, CancellationToken ct = default)
    {
        var (items, total) = await leadRepo.GetPagedAsync(page, pageSize, status, ct);
        return new PagedResult<LeadResponseDto>
        {
            Items = items.Select(Map),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<LeadResponseDto> UpdateLeadStatusAsync(Guid id, LeadStatus newStatus, CancellationToken ct = default)
    {
        var lead = await leadRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Lead), id);
        lead.Status = newStatus;
        lead.UpdatedAt = DateTime.UtcNow;
        await leadRepo.UpdateAsync(lead, ct);
        return Map(lead);
    }

    public async Task<OrderResponseDto> ConvertLeadToOrderAsync(Guid leadId, CreateOrderDto orderDto, CancellationToken ct = default)
    {
        var lead = await leadRepo.GetByIdAsync(leadId, ct)
            ?? throw new NotFoundException(nameof(Lead), leadId);

        if (lead.Status == LeadStatus.Converted)
            throw new InvalidOperationException("Lead is already converted to an order.");

        orderDto.ChannelUserId ??= lead.ChannelUserId;
        orderDto.OrderSource = lead.Source;

        var order = await orderService.CreateOrderAsync(orderDto, ct);

        lead.Status = LeadStatus.Converted;
        lead.ConvertedOrderId = order.Id;
        lead.UpdatedAt = DateTime.UtcNow;
        await leadRepo.UpdateAsync(lead, ct);

        logger.LogInformation("Lead {LeadId} converted to order {OrderNumber}", leadId, order.OrderNumber);
        return order;
    }

    private async Task SendAutoReplyAsync(Lead lead, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(lead.ChannelUserId)) return;
        var ref_ = lead.Id.ToString()[..8].ToUpper();
        var reply = $"Hi {lead.CustomerName}! 👋 Thanks for your interest in *{lead.ProductInterest}*.\n" +
                    $"Our team will reach you shortly. Reference: {ref_}";
        try
        {
            if (lead.Source == MessageChannel.WhatsApp)
                await whatsApp.SendTextMessageAsync(lead.ChannelUserId, reply, ct);
            else if (lead.Source == MessageChannel.Messenger)
                await messenger.SendTextMessageAsync(lead.ChannelUserId, reply, ct);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Auto-reply failed for lead {LeadId}", lead.Id); }
    }

    private static LeadResponseDto Map(Lead l) => new()
    {
        Id = l.Id,
        CustomerName = l.CustomerName,
        CustomerPhone = l.CustomerPhone,
        ProductInterest = l.ProductInterest,
        IncomingMessage = l.IncomingMessage,
        Status = l.Status,
        StatusName = l.Status.ToString(),
        Source = l.Source,
        CreatedAt = l.CreatedAt,
        ConvertedOrderId = l.ConvertedOrderId
    };
}
