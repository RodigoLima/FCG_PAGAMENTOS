using FCGPagamentos.Application.Abstractions;
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
        var eventStore = new Infrastructure.Persistence.EventStore
        {
            EventId = Guid.NewGuid(),
            Type = type,
            Payload = JsonSerializer.Serialize(payload, _jsonOptions),
            OccurredAt = occurredAt,
            Version = 1,
            AggregateId = ""
        };

        _context.Events.Add(eventStore);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AppendAsync<TEvent>(TEvent payload, DateTime occurredAt, CancellationToken ct)
    {
        var eventStore = new Infrastructure.Persistence.EventStore
        {
            EventId = Guid.NewGuid(),
            Type = payload?.GetType().Name ?? "Unknown",
            Payload = JsonSerializer.Serialize(payload, _jsonOptions),
            OccurredAt = occurredAt,
            Version = 1,
            AggregateId = ""
        };

        // Se for um Event do domínio, extrai as propriedades específicas
        if (payload is Event domainEvent)
        {
            eventStore.EventId = domainEvent.Id;
            eventStore.Version = domainEvent.Version;
            eventStore.AggregateId = domainEvent.AggregateId;
        }

        _context.Events.Add(eventStore);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Event>> GetEventsAsync(string aggregateId, CancellationToken ct)
    {
        var events = await _context.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.Version)
            .ToListAsync(ct);

        var result = new List<Event>();
        foreach (var eventData in events)
        {
            var eventType = Type.GetType($"FCGPagamentos.Domain.Events.{eventData.Type}");
            if (eventType != null)
            {
                var @event = JsonSerializer.Deserialize(eventData.Payload, eventType, _jsonOptions) as Event;
                if (@event != null)
                {
                    result.Add(@event);
                }
            }
        }

        return result;
    }

    public async Task<long> GetNextVersionAsync(string aggregateId, CancellationToken ct)
    {
        var lastVersion = await _context.Events
            .Where(e => e.AggregateId == aggregateId)
            .MaxAsync(e => (long?)e.Version, ct);

        return (lastVersion ?? 0) + 1;
    }
}
