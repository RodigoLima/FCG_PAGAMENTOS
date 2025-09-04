using FCGPagamentos.Domain.Enums;
using FCGPagamentos.Domain.Models;
using FCGPagamentos.Domain.ValueObjects;
using FCGPagamentos.Domain.Events;

namespace FCGPagamentos.Domain.Entities;
public class Payment: BaseModel, IAggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public string GameId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public Money Value { get; set; } = default!;
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public DateTime? ProcessedAt { get; private set; }

    // Event Sourcing
    public long Version { get; private set; }
    private readonly List<Event> _uncommittedEvents = new();

    public IReadOnlyCollection<Event> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    private Payment() { } // EF

    public Payment(string userId, string gameId, string correlationId, Money value, PaymentMethod method, DateTime now)
    {
        if (value.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(gameId)) throw new ArgumentException("GameId cannot be null or empty", nameof(gameId));
        if (string.IsNullOrWhiteSpace(correlationId)) throw new ArgumentException("CorrelationId cannot be null or empty", nameof(correlationId));
        
        UserId = userId;
        GameId = gameId; 
        CorrelationId = correlationId;
        Value = value; 
        Method = method;
        CreatedAt = now;
        UpdatedAt = now;
        Version = 1;

        // Adiciona eventos de criação e enfileiramento
        AddEvent(new PaymentCreated(Id, userId, gameId, correlationId, value.Amount, value.Currency, method.ToString(), Version));
        AddEvent(new PaymentQueued(Id, userId, gameId, correlationId, Version + 1));
    }
    public void MarkProcessing(DateTime now) 
    { 
        Status = PaymentStatus.Processing; 
        UpdatedAt = now;
        Version++;
        AddEvent(new PaymentProcessing(Id, CorrelationId, now, Version));
    }
    public void MarkApproved(DateTime now) 
    { 
        Status = PaymentStatus.Approved; 
        ProcessedAt = now; 
        UpdatedAt = now;
        Version++;
        AddEvent(new PaymentApproved(Id, CorrelationId, now, Version));
    }
    public void MarkDeclined(DateTime now, string? reason = null) 
    { 
        Status = PaymentStatus.Declined; 
        ProcessedAt = now; 
        UpdatedAt = now;
        Version++;
        AddEvent(new PaymentDeclined(Id, CorrelationId, now, reason, Version));
    }
    public void MarkFailed(DateTime now, string? reason = null) 
    { 
        Status = PaymentStatus.Failed; 
        ProcessedAt = now; 
        UpdatedAt = now;
        Version++;
        AddEvent(new PaymentFailed(Id, CorrelationId, now, reason, Version));
    }
    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }
    public void LoadFromHistory(IEnumerable<Event> events)
    {
        foreach (var @event in events.OrderBy(e => e.Version))
        {
            ApplyEvent(@event);
            Version = @event.Version;
        }
    }
    private void AddEvent(Event @event)
    {
        _uncommittedEvents.Add(@event);
    }
    private void ApplyEvent(Event @event)
    {
        switch (@event)
        {
            case PaymentCreated e:
                Status = PaymentStatus.Pending;
                break;
            case PaymentQueued e:
                Status = PaymentStatus.Pending;
                break;
            case PaymentProcessing e:
                Status = PaymentStatus.Processing;
                break;
            case PaymentApproved e:
                Status = PaymentStatus.Approved;
                ProcessedAt = e.ApprovedAt;
                break;
            case PaymentDeclined e:
                Status = PaymentStatus.Declined;
                ProcessedAt = e.DeclinedAt;
                break;
            case PaymentFailed e:
                Status = PaymentStatus.Failed;
                ProcessedAt = e.FailedAt;
                break;
        }
    }
}
