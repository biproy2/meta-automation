using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ecommerce.Tests.ApplicationTests;

/// <summary>
/// Unit tests for OrderService.
/// Uses Moq to fake all dependencies — tests ONLY the business logic.
/// No real database, no real HTTP calls.
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repo   = new();
    private readonly Mock<IWhatsAppService> _wa      = new();
    private readonly Mock<IMessengerService> _ms     = new();
    private readonly Mock<IPathaoService>    _pathao = new();
    private readonly Mock<ILogger<OrderService>> _log = new();

    private OrderService CreateService() => new(_repo.Object, _wa.Object, _ms.Object, _pathao.Object, _log.Object);

    [Fact]
    public async Task CreateOrder_ValidDto_ReturnsOrderWithGeneratedNumber()
    {
        // Arrange
        _repo.Setup(r => r.GenerateOrderNumberAsync(default)).ReturnsAsync("ORD-20240115-0001");
        _repo.Setup(r => r.AddAsync(It.IsAny<Order>(), default)).ReturnsAsync((Order o, CancellationToken _) => o);
        _wa.Setup(w => w.SendOrderConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<decimal>(), default)).Returns(Task.CompletedTask);

        var svc = CreateService();
        var dto = new CreateOrderDto
        {
            CustomerName = "Rahim Uddin",
            CustomerPhone = "+8801712345678",
            DeliveryAddress = "House 5, Road 3, Dhanmondi",
            City = "Dhaka",
            ProductName = "Cotton T-Shirt (L)",
            Quantity = 2,
            UnitPrice = 650,
            DeliveryCharge = 80,
            OrderSource = MessageChannel.WhatsApp,
            ChannelUserId = "+8801712345678"
        };

        // Act
        var result = await svc.CreateOrderAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.OrderNumber.Should().Be("ORD-20240115-0001");
        result.TotalAmount.Should().Be((650 * 2) + 80);  // = 1380
        result.Status.Should().Be(OrderStatus.Pending);
        _repo.Verify(r => r.AddAsync(It.IsAny<Order>(), default), Times.Once);
    }

    [Fact]
    public async Task GetOrderById_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        _repo.Setup(r => r.GetWithDeliveryAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Order?)null);
        var svc = CreateService();

        // Act + Assert
        await svc.Invoking(s => s.GetOrderByIdAsync(Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DispatchOrder_WhenStatusIsPending_ThrowsInvalidOperation()
    {
        // Arrange — order is still Pending (not Confirmed or Processing)
        var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.Pending, OrderNumber = "ORD-001" };
        _repo.Setup(r => r.GetWithDeliveryAsync(order.Id, default)).ReturnsAsync(order);
        var svc = CreateService();

        // Act + Assert
        await svc.Invoking(s => s.DispatchOrderAsync(order.Id))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Pending*");
    }

    [Fact]
    public async Task UpdateStatus_WhatsAppOrder_SendsStatusNotification()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            OrderNumber = "ORD-001",
            OrderSource = MessageChannel.WhatsApp,
            ChannelUserId = "+8801712345678"
        };
        _repo.Setup(r => r.GetByIdAsync(order.Id, default)).ReturnsAsync(order);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Order>(), default)).Returns(Task.CompletedTask);
        _wa.Setup(w => w.SendTextMessageAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        var svc = CreateService();

        // Act
        await svc.UpdateOrderStatusAsync(order.Id, new UpdateOrderStatusDto { NewStatus = OrderStatus.Confirmed });

        // Assert: WhatsApp notification was sent
        _wa.Verify(w => w.SendTextMessageAsync("+8801712345678", It.Is<string>(m => m.Contains("confirmed")), default), Times.Once);
    }
}
