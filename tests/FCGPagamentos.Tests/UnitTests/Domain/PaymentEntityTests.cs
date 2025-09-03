// tests/FCGPagamentos.Tests/UnitTests/Domain/PaymentEntityTests.cs
using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.Events;
using FCGPagamentos.Domain.Enums;
using FCGPagamentos.Domain.ValueObjects;
using Xunit;

namespace FCGPagamentos.Tests.UnitTests.Domain;

public class PaymentEntityTests
{
  [Fact]
  public void Constructor_ShouldCreatePaymentWithRequestedStatusAndOneEvent()
  {
    // Arrange
    var userId = Guid.NewGuid().ToString();
    var gameId = Guid.NewGuid().ToString();
    var correlationKey = Guid.NewGuid().ToString();
    var method = PaymentMethod.Pix;
    var value = new Money(100.00m, "BRL");
    var now = DateTime.UtcNow;

    // Act
    var payment = new Payment(userId, gameId, correlationKey, value, method, now);

    // Assert
    Assert.NotEqual(Guid.Empty, payment.Id);
    Assert.Equal(userId, payment.UserId);
    Assert.Equal(PaymentStatus.Processing, payment.Status);
    Assert.Equal(1, payment.Version);
    Assert.Single(payment.UncommittedEvents);
    Assert.IsType<PaymentCreated>(payment.UncommittedEvents.First());
  }

  [Fact]
  public void MarkProcessed_ShouldChangeStatusAndAddEvent()
  {
    // Arrange
    var userId = Guid.NewGuid().ToString();
    var gameId = Guid.NewGuid().ToString();
    var correlationKey = Guid.NewGuid().ToString();
    var method = PaymentMethod.Pix;
    var value = new Money(100.00m, "BRL");
    var now = DateTime.UtcNow;

    var payment = new Payment(userId, gameId, correlationKey, value, method, now);
    payment.MarkEventsAsCommitted();

    // Act
    var processedAt = DateTime.UtcNow.AddMinutes(5);
    payment.MarkProcessing(processedAt);

    // Assert
    Assert.Equal(PaymentStatus.Approved, payment.Status);
    Assert.Equal(processedAt, payment.ProcessedAt);
    Assert.Equal(2, payment.Version);
    Assert.Single(payment.UncommittedEvents);
    Assert.IsType<PaymentCreated>(payment.UncommittedEvents.First());
  }
}