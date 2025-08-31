using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentProcessing : Event
{
    public Guid PaymentId { get; }
    public DateTime StartedAt { get; }

    public PaymentProcessing(Guid paymentId)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        StartedAt = DateTime.UtcNow;
        AggregateId = paymentId.ToString();
        OccurredAt = DateTime.UtcNow;
        Version = 1;
    }
}
