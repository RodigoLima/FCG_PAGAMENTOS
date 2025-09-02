using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentDeclined : Event
{
    public Guid PaymentId { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTime DeclinedAt { get; private set; }
    public string? Reason { get; private set; }

    public PaymentDeclined(
        Guid paymentId, 
        string correlationId,
        DateTime declinedAt, 
        string? reason,
        long version) : base(paymentId.ToString(), version)
    {
        PaymentId = paymentId;
        CorrelationId = correlationId;
        DeclinedAt = declinedAt;
        Reason = reason;
    }

    protected PaymentDeclined() { } // Para EF
}
