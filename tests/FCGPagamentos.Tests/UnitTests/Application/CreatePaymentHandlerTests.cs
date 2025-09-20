using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.UseCases.CreatePayment;
using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.Enums;
using FCGPagamentos.Application.DTOs;

namespace FCGPagamentos.Tests.UnitTests.Application;

public class CreatePaymentHandlerTests
{
    private readonly Mock<IPaymentRepository> _repoMock;
    private readonly Mock<IClock> _clockMock;
    private readonly Mock<IPaymentProcessingPublisher> _publisherMock;
    private readonly CreatePaymentHandler _handler;
    private readonly Faker<CreatePaymentCommand> _commandFaker;
    private readonly DateTime _now = new DateTime(2025, 9, 2, 10, 0, 0);

    public CreatePaymentHandlerTests()
    {
        _repoMock = new Mock<IPaymentRepository>();
        _clockMock = new Mock<IClock>();
        _publisherMock = new Mock<IPaymentProcessingPublisher>();
        _handler = new CreatePaymentHandler(_repoMock.Object, _clockMock.Object, _publisherMock.Object);
        _clockMock.Setup(c => c.UtcNow).Returns(_now);

        _commandFaker = new Faker<CreatePaymentCommand>()
            .CustomInstantiator(f => new CreatePaymentCommand(
                Guid.NewGuid(),
                f.Random.Guid().ToString(),
                f.Random.Guid().ToString(),
                f.Random.Guid().ToString(),
                f.Finance.Amount(),
                f.Finance.Currency().Code,
                f.PickRandom<PaymentMethod>()
            ));
    }

    [Trait("Category", "UnitTest")]
    [Trait("Module", "Handle")]
    [Fact]
    public async Task Handle_ShouldReturnPaymentDto_WhenCommandIsValid()
    {
        // Arrange
        var command = _commandFaker.Generate();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PaymentDto>();
        result!.Amount.Should().Be(command.Amount);
        result.Currency.Should().Be(command.Currency);
        result.Method.Should().Be(command.Method);
        result.UserId.Should().Be(command.UserId);
        result.GameId.Should().Be(command.GameId);
        result.CreatedAt.Should().Be(_now);
    }

    [Trait("Category", "UnitTest")]
    [Trait("Module", "Handle")]
    [Fact]
    public async Task Handle_ShouldSavePayment_WhenCommandIsValid()
    {
        // Arrange
        var command = _commandFaker.Generate();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.AddAsync(
            It.Is<Payment>(p =>
                p.UserId == command.UserId &&
                p.GameId == command.GameId &&
                p.CorrelationId == command.CorrelationId &&
                p.Value.Amount == command.Amount &&
                p.Value.Currency == command.Currency &&
                p.Method == command.Method &&
                p.CreatedAt == _now),
            It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Trait("Category", "UnitTest")]
    [Trait("Module", "Handle")]
    [Fact]
    public async Task Handle_ShouldPublishMessage_WhenCommandIsValid()
    {
        // Arrange
        var command = _commandFaker.Generate();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _publisherMock.Verify(p => p.PublishPaymentForProcessingAsync(
            It.Is<PaymentRequestedMessage>(m =>
                m.PaymentId == result!.Id &&
                m.CorrelationId == command.CorrelationId &&
                m.UserId == command.UserId &&
                m.GameId == command.GameId &&
                m.Amount == command.Amount &&
                m.Currency == command.Currency &&
                m.PaymentMethod == command.Method.ToString() &&
                m.OccurredAt == _now),
            It.IsAny<CancellationToken>()),
        Times.Once);
    }
}