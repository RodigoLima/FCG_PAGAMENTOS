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
            WriteIndented = false,
            IncludeFields = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
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
            AggregateId = "" // Este método genérico não tem contexto de aggregate
        };

        _context.Events.Add(eventStore);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AppendAsync<TEvent>(TEvent payload, DateTime occurredAt, CancellationToken ct)
    {
        // Se for um Event do domínio, extrai as propriedades específicas
        if (payload is Event domainEvent)
        {
            var eventStore = new Infrastructure.Persistence.EventStore
            {
                EventId = domainEvent.Id,
                Type = domainEvent.Type,
                Payload = JsonSerializer.Serialize(payload, _jsonOptions),
                OccurredAt = domainEvent.OccurredAt,
                Version = domainEvent.Version,
                AggregateId = domainEvent.AggregateId
            };

            _context.Events.Add(eventStore);
            await _context.SaveChangesAsync(ct);
        }
        else
        {
            // Para eventos genéricos (não recomendado para Event Sourcing)
            var eventStore = new Infrastructure.Persistence.EventStore
            {
                EventId = Guid.NewGuid(),
                Type = payload?.GetType().Name ?? "Unknown",
                Payload = JsonSerializer.Serialize(payload, _jsonOptions),
                OccurredAt = occurredAt,
                Version = 1,
                AggregateId = ""
            };

            _context.Events.Add(eventStore);
            await _context.SaveChangesAsync(ct);
        }
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
            try
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
                else
                {
                    // Log do tipo não encontrado para debug
                    Console.WriteLine($"Tipo de evento não encontrado: {eventData.Type}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao deserializar evento {eventData.EventId}: {ex.Message}");
            }
        }

        return result;
    }

    public async Task<Event?> GetEventByIdAsync(Guid eventId, CancellationToken ct)
    {
        var eventData = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId, ct);

        if (eventData == null)
            return null;

        var eventType = Type.GetType($"FCGPagamentos.Domain.Events.{eventData.Type}");
        if (eventType == null)
            return null;

        var @event = JsonSerializer.Deserialize(eventData.Payload, eventType, _jsonOptions) as Event;
        return @event;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync(CancellationToken ct)
    {
        var events = await _context.Events
            .OrderBy(e => e.OccurredAt)
            .ThenBy(e => e.Version)
            .ToListAsync(ct);

        var result = new List<Event>();
        foreach (var eventData in events)
        {
            try
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
                else
                {
                    Console.WriteLine($"Tipo de evento não encontrado: {eventData.Type}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao deserializar evento {eventData.EventId}: {ex.Message}");
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
