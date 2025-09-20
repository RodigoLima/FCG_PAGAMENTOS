using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.Events;
using FCGPagamentos.Domain.Enums;
using FCGPagamentos.Tests.UnitTests.Domain.Builders;

namespace FCGPagamentos.Tests.UnitTests.Domain;

public class PaymentEntityTests
{
    private readonly PaymentBuilder _paymentBuilder;

    public PaymentEntityTests()
    {
        _paymentBuilder = new PaymentBuilder();
    }

    [Trait("Category", "UnitTest")]
    [Trait("Module", "Constructor")]
    [Fact]
    public void Constructor_ShouldCreatePaymentWithRequestedStatusAndOneEvent()
    {
        // Arrange
        var payment = _paymentBuilder.Build();

        // Assert
        payment.Id.Should().NotBe(Guid.Empty);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Version.Should().Be(1);
        payment.UncommittedEvents.Should().HaveCount(2);
        payment.UncommittedEvents.First().Should().BeOfType<PaymentCreated>();
    }

    [Trait("Category", "UnitTest")]
    [Trait("Module", "Mark")]
    [Fact]
    public void MarkProcessing_ShouldChangeStatusAndAddEvent()
    {
        // Arrange
        var payment = _paymentBuilder.Build();
        payment.MarkEventsAsCommitted();
        var processedAt = new DateTime(2025, 9, 2, 10, 5, 0);

        // Act
        payment.MarkProcessing(processedAt);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.Version.Should().Be(2);
        payment.UncommittedEvents.Should().HaveCount(1);
        payment.UncommittedEvents.First().Should().BeOfType<PaymentProcessing>();
    }

    [Trait("Category", "UnitTest")]
    [Trait("Module", "Mark")]
    [Fact]
    public void MarkApproved_ShouldChangeStatusAndAddEvent()
    {
        // Arrange
        var payment = _paymentBuilder.Build();
        payment.MarkEventsAsCommitted();
        var approvedAt = new DateTime(2025, 9, 2, 10, 5, 0);

        // Act
        payment.MarkApproved(approvedAt);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Approved);
        payment.Version.Should().Be(2);
        payment.UncommittedEvents.Should().HaveCount(1);
        payment.UncommittedEvents.First().Should().BeOfType<PaymentApproved>();
    }

    [Trait("Category", "UnitTest")]
    [Trait("Module", "Mark")]
    [Fact]
    public void MarkDeclined_ShouldChangeStatusAndAddEvent()
    {
        // Arrange
        var payment = _paymentBuilder.Build();
        payment.MarkEventsAsCommitted();
        var declinedAt = new DateTime(2025, 9, 2, 10, 5, 0);

        // Act
        payment.MarkDeclined(declinedAt);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Declined);
        payment.Version.Should().Be(2);
        payment.UncommittedEvents.Should().HaveCount(1);
        payment.UncommittedEvents.First().Should().BeOfType<PaymentDeclined>();
    }

    [Trait("Category", "UnitTest")]
    [Trait("Module", "Mark")]
    [Fact]
    public void MarkFailed_ShouldChangeStatusAndAddEvent()
    {
        // Arrange
        var payment = _paymentBuilder.Build();
        payment.MarkEventsAsCommitted();
        var failedAt = new DateTime(2025, 9, 2, 10, 5, 0);

        // Act
        payment.MarkFailed(failedAt);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.Version.Should().Be(2);
        payment.UncommittedEvents.Should().HaveCount(1);
        payment.UncommittedEvents.First().Should().BeOfType<PaymentFailed>();
    }
}