using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentFailed : Event
{
    public Guid PaymentId { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTime FailedAt { get; private set; }
    public string? Reason { get; private set; }

    public PaymentFailed(Guid paymentId, string correlationId, DateTime failedAt, string? reason, long version) 
        : base(paymentId.ToString(), version)
    {
        PaymentId = paymentId;
        CorrelationId = correlationId;
        FailedAt = failedAt;
        Reason = reason;
    }

    protected PaymentFailed() { } // Para EF
}
