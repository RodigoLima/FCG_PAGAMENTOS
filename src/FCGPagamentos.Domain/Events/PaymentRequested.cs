using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentRequested : Event
{
    public Guid PaymentId { get; }
    public Guid UserId { get; }
    public Guid GameId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Description { get; }
    public string PaymentMethod { get; }

    public PaymentRequested(
        Guid paymentId, 
        Guid userId, 
        Guid gameId, 
        decimal amount, 
        string currency, 
        string description, 
        string paymentMethod)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        UserId = userId;
        GameId = gameId;
        Amount = amount;
        Currency = currency;
        Description = description;
        PaymentMethod = paymentMethod;
        AggregateId = paymentId.ToString();
        OccurredAt = DateTime.UtcNow;
        Version = 1;
    }
}
