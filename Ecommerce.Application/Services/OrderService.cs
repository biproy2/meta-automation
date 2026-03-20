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
/// Core order business logic.
/// Orchestrates: DB (repository) + WhatsApp/Messenger (notify customer) + Pathao (courier booking).
/// This class has zero knowledge of HTTP, EF Core, or SQL.
/// </summary>
public class OrderService(
    IOrderRepository orderRepo,
    IWhatsAppService whatsApp,
    IMessengerService messenger,
    IPathaoService pathao,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct = default)
    {
        var orderNumber = await orderRepo.GenerateOrderNumberAsync(ct);

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            DeliveryAddress = dto.DeliveryAddress,
            City = dto.City,
            ProductName = dto.ProductName,
            ProductSku = dto.ProductSku,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            TotalAmount = (dto.UnitPrice * dto.Quantity) + dto.DeliveryCharge,
            DeliveryCharge = dto.DeliveryCharge,
            Status = OrderStatus.Pending,
            OrderSource = dto.OrderSource,
            ChannelUserId = dto.ChannelUserId,
            Notes = dto.Notes,
            UserId = dto.UserId
        };

        await orderRepo.AddAsync(order, ct);
        logger.LogInformation("Order {OrderNumber} created for {Customer}", orderNumber, dto.CustomerName);

        // Send confirmation to customer — fire and forget (failure must not break order creation)
        await NotifyOrderCreatedAsync(order, ct);

        return Map(order);
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepo.GetWithDeliveryAsync(id, ct)
            ?? throw new NotFoundException(nameof(Order), id);
        return Map(order);
    }

    public async Task<OrderResponseDto> GetOrderByNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByOrderNumberAsync(orderNumber, ct)
            ?? throw new NotFoundException(nameof(Order), orderNumber);
        return Map(order);
    }

    public async Task<PagedResult<OrderResponseDto>> GetOrdersAsync(
        int page, int pageSize, OrderStatus? status, CancellationToken ct = default)
    {
        var (items, total) = await orderRepo.GetPagedAsync(page, pageSize, status, ct);
        return new PagedResult<OrderResponseDto>
        {
            Items = items.Select(Map),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto dto, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Status = dto.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(dto.Notes))
            order.InternalNotes = dto.Notes;

        await orderRepo.UpdateAsync(order, ct);
        logger.LogInformation("Order {OrderNumber} → {Status}", order.OrderNumber, dto.NewStatus);

        await NotifyStatusChangeAsync(order, dto.NewStatus, ct);
        return Map(order);
    }

    public async Task<OrderResponseDto> DispatchOrderAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepo.GetWithDeliveryAsync(id, ct)
            ?? throw new NotFoundException(nameof(Order), id);

        if (order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Cannot dispatch an order with status '{order.Status}'.");

        var delivery = await pathao.CreateConsignmentAsync(order, ct);
        order.Delivery = delivery;
        order.Status = OrderStatus.Dispatched;
        order.UpdatedAt = DateTime.UtcNow;

        await orderRepo.UpdateAsync(order, ct);

        if (!string.IsNullOrEmpty(delivery.TrackingCode))
            await NotifyDeliveryAsync(order, delivery.TrackingCode, ct);

        return Map(order);
    }

    public async Task DeleteOrderAsync(Guid id, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Order), id);
        await orderRepo.DeleteAsync(order, ct);
    }

    // ── Notification helpers (failures are swallowed — never break main flow) ──
    private async Task NotifyOrderCreatedAsync(Order order, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(order.ChannelUserId)) return;
        try
        {
            if (order.OrderSource == MessageChannel.WhatsApp)
                await whatsApp.SendOrderConfirmationAsync(order.ChannelUserId, order.OrderNumber, order.ProductName, order.TotalAmount, ct);
            else if (order.OrderSource == MessageChannel.Messenger)
                await messenger.SendOrderConfirmationAsync(order.ChannelUserId, order.OrderNumber, order.ProductName, order.TotalAmount, ct);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Order confirm notification failed for {OrderNumber}", order.OrderNumber); }
    }

    private async Task NotifyStatusChangeAsync(Order order, OrderStatus newStatus, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(order.ChannelUserId)) return;
        var msg = newStatus switch
        {
            OrderStatus.Confirmed  => $"✅ Your order *{order.OrderNumber}* is confirmed! We will prepare it shortly.",
            OrderStatus.Processing => $"🔧 Your order *{order.OrderNumber}* is being prepared.",
            OrderStatus.Cancelled  => $"❌ Your order *{order.OrderNumber}* has been cancelled. Contact us for help.",
            _ => null
        };
        if (msg is null) return;
        try
        {
            if (order.OrderSource == MessageChannel.WhatsApp)
                await whatsApp.SendTextMessageAsync(order.ChannelUserId, msg, ct);
            else if (order.OrderSource == MessageChannel.Messenger)
                await messenger.SendTextMessageAsync(order.ChannelUserId, msg, ct);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Status notification failed for {OrderNumber}", order.OrderNumber); }
    }

    private async Task NotifyDeliveryAsync(Order order, string trackingCode, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(order.ChannelUserId)) return;
        try
        {
            if (order.OrderSource == MessageChannel.WhatsApp)
                await whatsApp.SendDeliveryUpdateAsync(order.ChannelUserId, order.OrderNumber, trackingCode, "Dispatched", ct);
            else if (order.OrderSource == MessageChannel.Messenger)
                await messenger.SendTextMessageAsync(order.ChannelUserId, $"🚚 Your order *{order.OrderNumber}* is on the way! Track: {trackingCode}", ct);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Delivery notification failed for {OrderNumber}", order.OrderNumber); }
    }

    private static OrderResponseDto Map(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        CustomerName = o.CustomerName,
        CustomerPhone = o.CustomerPhone,
        DeliveryAddress = o.DeliveryAddress,
        City = o.City,
        ProductName = o.ProductName,
        Quantity = o.Quantity,
        UnitPrice = o.UnitPrice,
        TotalAmount = o.TotalAmount,
        DeliveryCharge = o.DeliveryCharge,
        Status = o.Status,
        StatusName = o.Status.ToString(),
        OrderSource = o.OrderSource,
        Notes = o.Notes,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt,
        Delivery = o.Delivery is null ? null : new DeliveryResponseDto
        {
            Id = o.Delivery.Id,
            PathaoConsignmentId = o.Delivery.PathaoConsignmentId,
            TrackingCode = o.Delivery.TrackingCode,
            Status = o.Delivery.Status,
            StatusName = o.Delivery.Status.ToString(),
            PickupTime = o.Delivery.PickupTime,
            DeliveredAt = o.Delivery.DeliveredAt
        }
    };
}
