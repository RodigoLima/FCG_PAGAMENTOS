using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentRequested : Event
{
    public Guid PaymentId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string PaymentMethod { get; private set; } = string.Empty;

    public PaymentRequested(
        Guid paymentId, 
        Guid userId, 
        Guid gameId, 
        decimal amount, 
        string currency, 
        string description, 
        string paymentMethod,
        long version) : base(paymentId.ToString(), version)
    {
        PaymentId = paymentId;
        UserId = userId;
        GameId = gameId;
        Amount = amount;
        Currency = currency;
        Description = description;
        PaymentMethod = paymentMethod;
    }

    protected PaymentRequested() { } // Para EF
}
