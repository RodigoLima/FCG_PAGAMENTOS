using System.Text.Json;
using FCGPagamentos.Application.Abstractions;

namespace FCGPagamentos.Infrastructure.Persistence.Repositories;
public class EventStore(AppDbContext db) : IEventStore
{
    public async Task AppendAsync(string type, object payload, DateTime occurredAt, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload);
        await db.Events.AddAsync(new EventLog { Type = type, Payload = json, OccurredAt = occurredAt }, ct);
    }

    public Task AppendAsync<TEvent>(TEvent payload, DateTime occurredAt, CancellationToken ct)
        => AppendAsync(typeof(TEvent).Name, payload!, occurredAt, ct);
}
