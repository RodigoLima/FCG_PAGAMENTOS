using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.DTOs;
using FCGPagamentos.Application.UseCases.CreatePayment;
using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.Enums;

namespace FCGPagamentos.Tests.UnitTests.Application;

public class CreatePaymentHandlerTests
{
  private readonly Mock<IPaymentRepository> _repoMock;
  private readonly Mock<IClock> _clockMock;
  private readonly Mock<IPaymentProcessingPublisher> _publisherMock;
  private readonly CreatePaymentHandler _handler;

  public CreatePaymentHandlerTests()
  {
    _repoMock = new Mock<IPaymentRepository>();
    _clockMock = new Mock<IClock>();
    _publisherMock = new Mock<IPaymentProcessingPublisher>();
    _handler = new CreatePaymentHandler(_repoMock.Object, _clockMock.Object, _publisherMock.Object);
  }

  [Trait("Category", "UnitTest")]
  [Trait("Module", "Handle")]
  [Fact]
  public async Task Handle_ShouldCreatePaymentAndPublishMessage_WhenCommandIsValid()
  {
    // Arrange
    var command = new CreatePaymentCommand(
        Id: Guid.NewGuid(),
        UserId: Guid.NewGuid().ToString(),
        GameId: Guid.NewGuid().ToString(),
        CorrelationId: Guid.NewGuid().ToString(),
        Amount: 100.50m,
        Currency: "BRL",
        Method: PaymentMethod.Pix
    );
    var now = new DateTime(2025, 09, 02);
    _clockMock.Setup(c => c.UtcNow).Returns(now);
    _repoMock.Setup(r => 
      r.AddAsync(It.IsAny<Payment>(), 
      It.IsAny<CancellationToken>())
    )
      .Returns(Task.CompletedTask);

    //new PaymentRequestedMessage(
    //    It.IsAny<Guid>(), "", "", "", 0.0m, "", "", now, ""
    //    ), CancellationToken.None
    //  )

    _publisherMock.Setup(p => p.PublishPaymentForProcessingAsync(
      new PaymentRequestedMessage(
        It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), now, It.IsAny<string>()
        ), CancellationToken.None
      ))
      .Returns(Task.CompletedTask);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.IsType<PaymentDto>(result);
    Assert.Equal(command.Amount, result.Amount);
    _repoMock.Verify(r => r.AddAsync(
        It.Is<Payment>(p => p.UserId == command.UserId && p.Value.Amount == command.Amount),
        It.IsAny<CancellationToken>()), Times.Once);
    _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    _publisherMock.Verify(p => p.PublishPaymentForProcessingAsync(
      It.Is<PaymentRequestedMessage>(m =>
        m.PaymentId == result.Id &&
        m.CorrelationId == command.CorrelationId &&
        m.UserId == command.UserId &&
        m.GameId == command.GameId &&
        m.Amount == command.Amount &&
        m.Currency == command.Currency &&
        m.OccurredAt == now
      ),
      It.IsAny<CancellationToken>()),
    Times.Once);
  }
}