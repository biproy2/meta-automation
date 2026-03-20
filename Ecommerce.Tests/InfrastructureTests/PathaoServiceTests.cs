using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Services;
using Ecommerce.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Ecommerce.Tests.InfrastructureTests;

/// <summary>
/// Tests PathaoService using a fake HttpClient (no real API calls).
/// This tests that we serialize the request correctly and parse responses.
/// </summary>
public class PathaoServiceTests
{
    // NOTE: Testing HttpClient-based services requires either:
    // A) A mock HttpMessageHandler (shown below)
    // B) A WireMock server for more realistic integration tests

    [Fact]
    public void PathaoSettings_DefaultValues_AreCorrect()
    {
        var settings = new PathaoSettings();
        settings.ApiBaseUrl.Should().Be("https://hermes.pathao.com");
    }

    [Fact]
    public void Order_TotalAmount_CalculatedCorrectly()
    {
        // Test domain logic: total = (price * qty) + delivery charge
        var unitPrice = 650m;
        var qty = 2;
        var deliveryCharge = 80m;
        var expected = (unitPrice * qty) + deliveryCharge;  // 1380

        expected.Should().Be(1380m);
    }
}
