using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentApproved : Event
{
    public Guid PaymentId { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTime ApprovedAt { get; private set; }

    public PaymentApproved(
        Guid paymentId, 
        string correlationId,
        DateTime approvedAt, 
        long version) : base(paymentId.ToString(), version)
    {
        PaymentId = paymentId;
        CorrelationId = correlationId;
        ApprovedAt = approvedAt;
    }

    protected PaymentApproved() { } // Para EF
}
