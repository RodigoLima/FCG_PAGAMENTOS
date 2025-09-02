using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentQueued : Event
{
    public Guid PaymentId { get; private set; }
    public string OrderId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;

    public PaymentQueued(
        Guid paymentId, 
        string orderId, 
        string correlationId,
        long version) : base(paymentId.ToString(), version)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        CorrelationId = correlationId;
    }

    protected PaymentQueued() { } // Para EF
}
