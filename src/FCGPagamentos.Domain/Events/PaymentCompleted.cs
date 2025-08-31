using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentCompleted : Event
{
    public Guid PaymentId { get; private set; }
    public DateTime CompletedAt { get; private set; }
    public string TransactionId { get; private set; } = string.Empty;

    public PaymentCompleted(Guid paymentId, string transactionId, long version) : base(paymentId.ToString(), version)
    {
        PaymentId = paymentId;
        CompletedAt = DateTime.UtcNow;
        TransactionId = transactionId;
    }

    protected PaymentCompleted() { } // Para EF
}
