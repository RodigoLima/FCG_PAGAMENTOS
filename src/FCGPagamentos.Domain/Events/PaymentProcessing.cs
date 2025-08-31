using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentProcessing : Event
{
    public Guid PaymentId { get; private set; }
    public DateTime StartedAt { get; private set; }

    public PaymentProcessing(Guid paymentId, long version) : base(paymentId.ToString(), version)
    {
        PaymentId = paymentId;
        StartedAt = DateTime.UtcNow;
    }

    protected PaymentProcessing() { } // Para EF
}
