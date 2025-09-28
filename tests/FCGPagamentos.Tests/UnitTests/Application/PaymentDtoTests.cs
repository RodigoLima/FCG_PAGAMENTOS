using FCGPagamentos.Application.DTOs;
using FCGPagamentos.Domain.Enums;

namespace FCGPagamentos.Tests.UnitTests.Application;

public class PaymentDtoTests
{
    [Trait("Category", "UnitTest")]
    [Trait("Module", "Constructor")]
    [Fact]
    public void Constructor_ShouldCreatePaymentDtoWithCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var gameId = Guid.NewGuid().ToString();
        var amount = 100.50m;
        var currency = "BRL";
        var method = PaymentMethod.Pix;
        var status = PaymentStatus.Pending;
        var createdAt = DateTime.UtcNow;
        var lastUpdateAt = DateTime.UtcNow.AddMinutes(5);
        var processedAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        var paymentDto = new PaymentDto(id, userId, gameId, amount, currency, method, status, createdAt, lastUpdateAt, processedAt);

        // Assert
        paymentDto.Id.Should().Be(id);
        paymentDto.UserId.Should().Be(userId);
        paymentDto.GameId.Should().Be(gameId);
        paymentDto.Amount.Should().Be(amount);
        paymentDto.Currency.Should().Be(currency);
        paymentDto.Method.Should().Be(method);
        paymentDto.Status.Should().Be(status);
        paymentDto.CreatedAt.Should().Be(createdAt);
        paymentDto.LastUpdateAt.Should().Be(lastUpdateAt);
        paymentDto.ProcessedAt.Should().Be(processedAt);
    }
}
