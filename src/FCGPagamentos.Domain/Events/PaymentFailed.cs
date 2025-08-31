using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

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
