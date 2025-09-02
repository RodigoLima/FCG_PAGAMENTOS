using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentCreated : Event
{
    public Guid PaymentId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string GameId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string PaymentMethod { get; private set; } = string.Empty;

    public PaymentCreated(
        Guid paymentId, 
        string userId, 
        string gameId,
        string correlationId,
        decimal amount, 
        string currency, 
        string paymentMethod,
        long version) : base(paymentId.ToString(), version)
    {
        PaymentId = paymentId;
        UserId = userId;
        GameId = gameId;
        CorrelationId = correlationId;
        Amount = amount;
        Currency = currency;
        PaymentMethod = paymentMethod;
    }

    protected PaymentCreated() { } // Para EF
}
