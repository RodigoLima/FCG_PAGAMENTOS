using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.Enums;
using FCGPagamentos.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FCGPagamentos.Infrastructure.Persistence;

public class EventStoreRepository : IEventStore
{
    private readonly AppDbContext _context;
    private readonly JsonSerializerOptions _jsonOptions;

    public EventStoreRepository(AppDbContext context)
    {
        _context = context;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task AppendAsync(string type, object payload, DateTime occurredAt, CancellationToken ct)
    {
        // Método genérico não suportado - usar AppendAsync com Event específico
        await Task.CompletedTask;
        throw new NotSupportedException("Use AppendAsync with specific Event type");
    }

    public async Task AppendAsync<TEvent>(TEvent payload, DateTime occurredAt, CancellationToken ct)
    {
        if (payload is Event domainEvent)
        {
            // Extrai PaymentId do AggregateId (formato: "Payment_{paymentId}")
            var paymentIdStr = domainEvent.AggregateId.Replace("Payment_", "");
            if (!Guid.TryParse(paymentIdStr, out var paymentId))
            {
                throw new ArgumentException($"Invalid PaymentId in AggregateId: {domainEvent.AggregateId}");
            }

            // Mapeia o tipo do evento para EventType enum
            var eventType = MapEventType(domainEvent.Type);

            // Cria PaymentEvent
            var paymentEvent = new PaymentEvent(
                paymentId,
                eventType,
                JsonDocument.Parse(JsonSerializer.Serialize(payload, _jsonOptions)),
                domainEvent.OccurredAt
            );

            _context.PaymentEvents.Add(paymentEvent);
            await _context.SaveChangesAsync(ct);
        }
        else
        {
            throw new ArgumentException("Only Event types are supported");
        }
    }

    public async Task<IEnumerable<Event>> GetEventsAsync(string aggregateId, CancellationToken ct)
    {
        // Método não implementado - não usado no sistema atual
        await Task.CompletedTask;
        return Enumerable.Empty<Event>();
    }

    public async Task<Event?> GetEventByIdAsync(Guid eventId, CancellationToken ct)
    {
        // Método não implementado - não usado no sistema atual
        await Task.CompletedTask;
        return null;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync(CancellationToken ct)
    {
        // Método não implementado - não usado no sistema atual
        await Task.CompletedTask;
        return Enumerable.Empty<Event>();
    }

    public async Task<long> GetNextVersionAsync(string aggregateId, CancellationToken ct)
    {
        // Método não implementado - não usado no sistema atual
        await Task.CompletedTask;
        return 1;
    }

    private static EventType MapEventType(string eventType)
    {
        return eventType switch
        {
            "PaymentCreated" => EventType.PaymentCreated,
            "PaymentQueued" => EventType.PaymentQueued,
            "PaymentProcessing" => EventType.PaymentProcessing,
            "PaymentApproved" => EventType.PaymentApproved,
            "PaymentDeclined" => EventType.PaymentDeclined,
            "PaymentFailed" => EventType.PaymentFailed,
            _ => throw new ArgumentException($"Unknown event type: {eventType}")
        };
    }
}
