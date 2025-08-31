using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentCompleted : Event
{
    public Guid PaymentId { get; }
    public DateTime CompletedAt { get; }
    public string TransactionId { get; }

    public PaymentCompleted(Guid paymentId, string transactionId)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        CompletedAt = DateTime.UtcNow;
        TransactionId = transactionId;
        AggregateId = paymentId.ToString();
        OccurredAt = DateTime.UtcNow;
        Version = 1;
    }
}
