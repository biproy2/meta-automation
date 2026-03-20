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

public class LeadServiceTests
{
    private readonly Mock<ILeadRepository>   _repo   = new();
    private readonly Mock<IOrderService>     _orders = new();
    private readonly Mock<IWhatsAppService>  _wa     = new();
    private readonly Mock<IMessengerService> _ms     = new();
    private readonly Mock<ILogger<LeadService>> _log  = new();

    private LeadService CreateService() => new(_repo.Object, _orders.Object, _wa.Object, _ms.Object, _log.Object);

    [Fact]
    public async Task CreateLead_ValidDto_ReturnsLeadAndSendsAutoReply()
    {
        // Arrange
        _repo.Setup(r => r.AddAsync(It.IsAny<Lead>(), default)).ReturnsAsync((Lead l, CancellationToken _) => l);
        _wa.Setup(w => w.SendTextMessageAsync(It.IsAny<string>(), It.IsAny<string>(), default)).Returns(Task.CompletedTask);

        var svc = CreateService();
        var dto = new LeadDto
        {
            CustomerName = "Fatema Begum",
            CustomerPhone = "+8801987654321",
            ProductInterest = "Silk Saree",
            Source = MessageChannel.WhatsApp,
            ChannelUserId = "+8801987654321"
        };

        // Act
        var result = await svc.CreateLeadAsync(dto);

        // Assert
        result.CustomerName.Should().Be("Fatema Begum");
        result.Status.Should().Be(LeadStatus.New);
        _wa.Verify(w => w.SendTextMessageAsync("+8801987654321", It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task ConvertLead_AlreadyConverted_ThrowsInvalidOperation()
    {
        // Arrange
        var lead = new Lead { Id = Guid.NewGuid(), Status = LeadStatus.Converted };
        _repo.Setup(r => r.GetByIdAsync(lead.Id, default)).ReturnsAsync(lead);

        var svc = CreateService();

        // Act + Assert
        await svc.Invoking(s => s.ConvertLeadToOrderAsync(lead.Id, new CreateOrderDto()))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}
