using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentQueued : Event
{
    public Guid PaymentId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string GameId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;

    public PaymentQueued(
        Guid paymentId, 
        string userId, 
        string gameId,
        string correlationId,
        long version) : base(paymentId.ToString(), version)
    {
        PaymentId = paymentId;
        UserId = userId;
        GameId = gameId;
        CorrelationId = correlationId;
    }

    protected PaymentQueued() { } // Para EF
}
