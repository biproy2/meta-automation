using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Models;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Services;

public class OrderService(
    IOrderRepository orderRepo,
    ITenantRepository tenantRepo,
    IWhatsAppService whatsApp,
    IMessengerService messenger,
    IPathaoService pathao,
    IShopifyService shopify,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<OrderResponseDto> CreateOrderAsync(Guid tenantId, CreateOrderDto dto, CancellationToken ct = default)
    {
        var orderNumber = await orderRepo.GenerateOrderNumberAsync(tenantId, ct);
        var order = new Order
        {
            TenantId = tenantId,
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
            Notes = dto.Notes
        };

        await orderRepo.AddAsync(order, ct);
        logger.LogInformation("[{TenantId}] Order {OrderNumber} created", tenantId, orderNumber);

        // Notify customer
        await NotifyOrderCreatedAsync(tenantId, order, ct);

        return Map(order);
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var order = await orderRepo.GetWithDeliveryAsync(id, tenantId, ct)
            ?? throw new NotFoundException(nameof(Order), id);
        return Map(order);
    }

    public async Task<PagedResult<OrderResponseDto>> GetOrdersAsync(Guid tenantId, int page, int pageSize, OrderStatus? status, CancellationToken ct = default)
    {
        var (items, total) = await orderRepo.GetPagedAsync(tenantId, page, pageSize, status, ct);
        return new PagedResult<OrderResponseDto> { Items = items.Select(Map), TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(Guid tenantId, Guid id, UpdateOrderStatusDto dto, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdAsync(id, tenantId, ct)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Status = dto.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(dto.Notes)) order.InternalNotes = dto.Notes;

        await orderRepo.UpdateAsync(order, ct);
        await NotifyStatusChangeAsync(tenantId, order, dto.NewStatus, ct);
        return Map(order);
    }

    public async Task<OrderResponseDto> DispatchOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var order = await orderRepo.GetWithDeliveryAsync(id, tenantId, ct)
            ?? throw new NotFoundException(nameof(Order), id);

        if (order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Cannot dispatch order with status '{order.Status}'.");

        var settings = await tenantRepo.GetSettingsAsync(tenantId, ct)
            ?? throw new NotFoundException("TenantSettings", tenantId);

        Delivery? delivery = null;

        if (settings.DeliveryProvider == DeliveryProvider.Pathao && !string.IsNullOrEmpty(settings.PathaoClientId))
        {
            delivery = await pathao.CreateConsignmentAsync(settings, order, ct);
            order.Delivery = delivery;
        }
        else if (settings.DeliveryProvider == DeliveryProvider.ShopifyShipping && !string.IsNullOrEmpty(settings.ShopifyStoreUrl))
        {
            var shopifyResult = await shopify.CreateOrderAsync(settings, order, ct);
            order.ShopifyOrderId = shopifyResult.ShopifyOrderId;
        }

        order.Status = OrderStatus.Dispatched;
        order.UpdatedAt = DateTime.UtcNow;
        await orderRepo.UpdateAsync(order, ct);

        if (delivery?.TrackingCode != null)
            await NotifyDeliveryAsync(settings, order, delivery.TrackingCode, ct);

        return Map(order);
    }

    public async Task DeleteOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdAsync(id, tenantId, ct)
            ?? throw new NotFoundException(nameof(Order), id);
        await orderRepo.DeleteAsync(order, ct);
    }

    private async Task NotifyOrderCreatedAsync(Guid tenantId, Order order, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(order.ChannelUserId)) return;
        try
        {
            var settings = await tenantRepo.GetSettingsAsync(tenantId, ct);
            if (settings == null) return;
            if (order.OrderSource == MessageChannel.WhatsApp && !string.IsNullOrEmpty(settings.WhatsAppAccessToken))
                await whatsApp.SendOrderConfirmationAsync(settings, order.ChannelUserId, order.OrderNumber, order.ProductName, order.TotalAmount, ct);
            else if (order.OrderSource == MessageChannel.Messenger && !string.IsNullOrEmpty(settings.MessengerPageToken))
                await messenger.SendOrderConfirmationAsync(settings, order.ChannelUserId, order.OrderNumber, order.ProductName, order.TotalAmount, ct);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Order confirmation notification failed for {OrderNumber}", order.OrderNumber); }
    }

    private async Task NotifyStatusChangeAsync(Guid tenantId, Order order, OrderStatus newStatus, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(order.ChannelUserId)) return;
        var msg = newStatus switch
        {
            OrderStatus.Confirmed => $"✅ Your order *{order.OrderNumber}* is confirmed!",
            OrderStatus.Cancelled => $"❌ Your order *{order.OrderNumber}* has been cancelled.",
            _ => null
        };
        if (msg == null) return;
        try
        {
            var settings = await tenantRepo.GetSettingsAsync(tenantId, ct);
            if (settings == null) return;
            if (order.OrderSource == MessageChannel.WhatsApp && !string.IsNullOrEmpty(settings.WhatsAppAccessToken))
                await whatsApp.SendTextMessageAsync(settings, order.ChannelUserId, msg, ct);
            else if (order.OrderSource == MessageChannel.Messenger && !string.IsNullOrEmpty(settings.MessengerPageToken))
                await messenger.SendTextMessageAsync(settings, order.ChannelUserId, msg, ct);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Status notification failed for {OrderNumber}", order.OrderNumber); }
    }

    private async Task NotifyDeliveryAsync(Domain.Entities.TenantSettings settings, Order order, string trackingCode, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(order.ChannelUserId)) return;
        try
        {
            if (order.OrderSource == MessageChannel.WhatsApp && !string.IsNullOrEmpty(settings.WhatsAppAccessToken))
                await whatsApp.SendDeliveryUpdateAsync(settings, order.ChannelUserId, order.OrderNumber, trackingCode, ct);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Delivery notification failed for {OrderNumber}", order.OrderNumber); }
    }

    private static OrderResponseDto Map(Order o) => new()
    {
        Id = o.Id, OrderNumber = o.OrderNumber, CustomerName = o.CustomerName,
        CustomerPhone = o.CustomerPhone, DeliveryAddress = o.DeliveryAddress, City = o.City,
        ProductName = o.ProductName, Quantity = o.Quantity, UnitPrice = o.UnitPrice,
        TotalAmount = o.TotalAmount, DeliveryCharge = o.DeliveryCharge,
        Status = o.Status, StatusName = o.Status.ToString(), OrderSource = o.OrderSource,
        Notes = o.Notes, ShopifyOrderId = o.ShopifyOrderId,
        CreatedAt = o.CreatedAt, UpdatedAt = o.UpdatedAt,
        Delivery = o.Delivery == null ? null : new DeliveryResponseDto
        {
            Id = o.Delivery.Id, TrackingCode = o.Delivery.TrackingCode,
            Status = o.Delivery.Status, StatusName = o.Delivery.Status.ToString(),
            DeliveredAt = o.Delivery.DeliveredAt
        }
    };
}
