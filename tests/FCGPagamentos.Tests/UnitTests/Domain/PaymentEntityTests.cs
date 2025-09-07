using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.Events;
using FCGPagamentos.Domain.Enums;
using FCGPagamentos.Domain.ValueObjects;

namespace FCGPagamentos.Tests.UnitTests.Domain;

public class PaymentEntityTests
{
  [Trait("Category", "UnitTest")]
  [Trait("Module", "Constructor")]
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
    Assert.Equal(PaymentStatus.Pending, payment.Status);
    Assert.Equal(1, payment.Version);
    Assert.NotEmpty(payment.UncommittedEvents);
    Assert.IsType<PaymentCreated>(payment.UncommittedEvents.First());
  }
  [Trait("Category", "UnitTest")]
  [Trait("Module", "Mark")]
  [Fact]
  public void MarkProcessing_ShouldChangeStatusAndAddEvent()
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
    Assert.Equal(PaymentStatus.Processing, payment.Status);
    Assert.Equal(2, payment.Version);
    Assert.Single(payment.UncommittedEvents);
    Assert.IsType<PaymentProcessing>(payment.UncommittedEvents.First());
  }
  [Trait("Category", "UnitTest")]
  [Trait("Module", "Mark")]
  [Fact]
  public void MarkApproved_ShouldChangeStatusAndAddEvent()
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
    var aproved = DateTime.UtcNow.AddMinutes(5);
    payment.MarkApproved(aproved);

    // Assert
    Assert.Equal(PaymentStatus.Approved, payment.Status);
    Assert.Equal(2, payment.Version);
    Assert.Single(payment.UncommittedEvents);
    Assert.IsType<PaymentApproved>(payment.UncommittedEvents.First());
  }
  [Trait("Category", "UnitTest")]
  [Trait("Module", "Mark")]
  [Fact]
  public void MarkDeclined_ShouldChangeStatusAndAddEvent()
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
    var declined = DateTime.UtcNow.AddMinutes(5);
    payment.MarkDeclined(declined);

    // Assert
    Assert.Equal(PaymentStatus.Declined, payment.Status);
    Assert.Equal(2, payment.Version);
    Assert.Single(payment.UncommittedEvents);
    Assert.IsType<PaymentDeclined>(payment.UncommittedEvents.First());
  }
  [Trait("Category", "UnitTest")]
  [Trait("Module", "Mark")]
  [Fact]
  public void MarkFailed_ShouldChangeStatusAndAddEvent()
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
    var declined = DateTime.UtcNow.AddMinutes(5);
    payment.MarkFailed(declined);

    // Assert
    Assert.Equal(PaymentStatus.Failed, payment.Status);
    Assert.Equal(2, payment.Version);
    Assert.Single(payment.UncommittedEvents);
    Assert.IsType<PaymentFailed>(payment.UncommittedEvents.First());
  }
}