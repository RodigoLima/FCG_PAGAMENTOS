using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentProcessing : Event
{
    public Guid PaymentId { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTime StartedAt { get; private set; }

    public PaymentProcessing(Guid paymentId, string correlationId, DateTime startedAt, long version) : base(paymentId.ToString(), version)
    {
        PaymentId = paymentId;
        CorrelationId = correlationId;
        StartedAt = startedAt;
    }

    protected PaymentProcessing() { } // Para EF
}
