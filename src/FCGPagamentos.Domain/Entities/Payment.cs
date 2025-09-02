using FCGPagamentos.Domain.Enums;
using FCGPagamentos.Domain.Models;
using FCGPagamentos.Domain.ValueObjects;
using FCGPagamentos.Domain.Events;

namespace FCGPagamentos.Domain.Entities;
public class Payment: BaseModel, IAggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public Money Value { get; set; } = default!;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Requested;
    public DateTime? ProcessedAt { get; private set; }

    // Event Sourcing
    public long Version { get; private set; }
    private readonly List<Event> _uncommittedEvents = new();

    public IReadOnlyCollection<Event> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    private Payment() { } // EF

    public Payment(Guid userId, Guid gameId, Money value, DateTime now)
    {
        if (value.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        
        UserId = userId; 
        GameId = gameId; 
        Value = value; 
        CreatedAt = now;
        Version = 1;

        // Adiciona evento de criação
        AddEvent(new PaymentRequested(Id, userId, gameId, value.Amount, value.Currency, 
            $"Pagamento para jogo {gameId}", "CreditCard", Version));
    }

    public void MarkProcessed(DateTime now) 
    { 
        Status = PaymentStatus.Processed; 
        ProcessedAt = now; 
        Version++;
        AddEvent(new PaymentCompleted(Id, Guid.NewGuid().ToString(), Version));
    }

    public void MarkFailed(DateTime now, string? reason = null) 
    { 
        Status = PaymentStatus.Failed; 
        ProcessedAt = now; 
        Version++;
        AddEvent(new PaymentFailed(Id, now, reason, Version));
    }

    private void AddEvent(Event @event)
    {
        _uncommittedEvents.Add(@event);
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

    private void ApplyEvent(Event @event)
    {
        switch (@event)
        {
            case PaymentRequested e:
                // Estado já está correto, só atualizar versão
                break;
            case PaymentCompleted e:
                Status = PaymentStatus.Processed;
                ProcessedAt = e.CompletedAt;
                break;
            case PaymentFailed e:
                Status = PaymentStatus.Failed;
                ProcessedAt = e.FailedAt;
                break;
        }
    }
}
