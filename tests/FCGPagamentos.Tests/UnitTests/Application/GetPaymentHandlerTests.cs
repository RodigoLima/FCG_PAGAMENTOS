using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.UseCases.GetPayment;
using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Tests.UnitTests.Domain.Builders;
using FCGPagamentos.Application.DTOs;

namespace FCGPagamentos.Tests.UnitTests.Application;

public class GetPaymentHandlerTests
{
    private readonly Mock<IPaymentRepository> _repoMock;
    private readonly GetPaymentHandler _handler;
    private readonly PaymentBuilder _paymentBuilder;

    public GetPaymentHandlerTests()
    {
        _repoMock = new Mock<IPaymentRepository>();
        _handler = new GetPaymentHandler(_repoMock.Object);
        _paymentBuilder = new PaymentBuilder();
    }

    [Trait("Category", "UnitTest")]
    [Trait("Module", "Handle")]
    [Fact]
    public async Task Handle_ShouldReturnPaymentDto_WhenPaymentExists()
    {
        // Arrange
        var payment = _paymentBuilder.Build();
        var query = new GetPaymentQuery(payment.Id);
        _repoMock.Setup(r => r.GetAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PaymentDto>();
        result!.Id.Should().Be(payment.Id);
        result.Amount.Should().Be(payment.Value.Amount);
        result.Currency.Should().Be(payment.Value.Currency);
        result.Method.Should().Be(payment.Method);
        result.Status.Should().Be(payment.Status);
        result.UserId.Should().Be(payment.UserId);
        result.GameId.Should().Be(payment.GameId);
        result.CreatedAt.Should().Be(payment.CreatedAt);
    }

    [Trait("Category", "UnitTest")]
    [Trait("Module", "Handle")]
    [Fact]
    public async Task Handle_ShouldReturnNull_WhenPaymentDoesNotExist()
    {
        // Arrange
        var query = new GetPaymentQuery(Guid.NewGuid());
        _repoMock.Setup(r => r.GetAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
