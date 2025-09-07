// tests/FCGPagamentos.Tests/UnitTests/Domain/PaymentCreatedTests.cs
using Bogus;
using FCGPagamentos.Domain.Enums;
using FCGPagamentos.Domain.Events;
using FCGPagamentos.Domain.ValueObjects;
using System;
using System.Reflection;
using Xunit;

namespace FCGPagamentos.Tests.UnitTests.Domain;

public class PaymentCreatedTests
{
  [Trait("Category", "UnitTest")]
  [Trait("Module", "Constructor")]
  [Fact]
  public void Constructor_ShouldInitializeAllPropertiesCorrectly()
  {
    // Arrange
    var paymentId = Guid.NewGuid();
    var userId = Guid.NewGuid().ToString();
    var gameId = Guid.NewGuid().ToString();
    var correlationId = Guid.NewGuid().ToString();
    var vaue = new Money(150.75m, "BRL");
    var currency = "BRL";
    var paymentMethod = Guid.NewGuid().ToString();
    var method = PaymentMethod.Pix.ToString();
    var now = DateTime.UtcNow;
    var version = 1;

    // Act
    var paymentCreatedEvent = new PaymentCreated(
      paymentId, 
      userId, 
      gameId, 
      correlationId,
      vaue.Amount, 
      currency,
      method,
      version
    );

    // Assert
    Assert.Equal(paymentId.ToString(), paymentCreatedEvent.AggregateId);
    Assert.Equal(version, paymentCreatedEvent.Version);
    Assert.Equal("PaymentCreated", paymentCreatedEvent.Type);
    Assert.Equal(paymentId, paymentCreatedEvent.PaymentId);
    Assert.Equal(userId, paymentCreatedEvent.UserId);
    Assert.Equal(gameId, paymentCreatedEvent.GameId);
    Assert.Equal(vaue.Amount, paymentCreatedEvent.Amount);
    Assert.Equal(vaue.Currency, paymentCreatedEvent.Currency);
    Assert.True(paymentCreatedEvent.OccurredAt > DateTime.MinValue);
  }
  [Trait("Category", "UnitTest")]
  [Trait("Module", "Constructor")]
  [Fact]
  public void DefaultConstructor_ShouldBePresentForEF()
  {
    // Act 3ff35ad9-fae5-4995-a390-0f8faf1b07b4
    var paymentCreatedEvent = (PaymentCreated)Activator.CreateInstance(typeof(PaymentCreated),
        BindingFlags.NonPublic | BindingFlags.Instance, null, null, null)!;

    // Assert
    Assert.NotNull(paymentCreatedEvent);
    Assert.Equal(string.Empty, paymentCreatedEvent?.AggregateId);
    Assert.Equal(0.0m, paymentCreatedEvent?.Amount);
    Assert.Equal(string.Empty, paymentCreatedEvent?.CorrelationId);
    Assert.Equal(string.Empty, paymentCreatedEvent?.Currency);
    Assert.Equal(string.Empty, paymentCreatedEvent?.GameId);
    Assert.Equal(string.Empty, paymentCreatedEvent?.UserId);
    Assert.Equal(0, paymentCreatedEvent?.Version);
    Assert.NotNull(paymentCreatedEvent?.Id);
  }
}