using FCGPagamentos.Domain.Models;
using FCGPagamentos.Domain.ValueObjects;
using System.Text.Json;

namespace FCGPagamentos.Domain.Events;

public class PaymentCreated : Event
{
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public Money Value { get; private set; }

    public PaymentCreated(Guid paymentId, Guid userId, Guid gameId, Money value, long version) 
        : base(paymentId.ToString(), version)
    {
        UserId = userId;
        GameId = gameId;
        Value = value;
    }

    protected PaymentCreated() { } // Para EF
}

public class PaymentProcessed : Event
{
    public DateTime ProcessedAt { get; private set; }

    public PaymentProcessed(Guid paymentId, DateTime processedAt, long version) 
        : base(paymentId.ToString(), version)
    {
        ProcessedAt = processedAt;
    }

    protected PaymentProcessed() { } // Para EF
}

public class PaymentFailed : Event
{
    public DateTime FailedAt { get; private set; }
    public string? Reason { get; private set; }

    public PaymentFailed(Guid paymentId, DateTime failedAt, string? reason, long version) 
        : base(paymentId.ToString(), version)
    {
        FailedAt = failedAt;
        Reason = reason;
    }

    protected PaymentFailed() { } // Para EF
}
