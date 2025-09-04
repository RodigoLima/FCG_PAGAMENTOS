using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Application.Abstractions;

public interface IEventStore
{
    Task AppendAsync(string type, object payload, DateTime occurredAt, CancellationToken ct);
    Task AppendAsync<TEvent>(TEvent payload, DateTime occurredAt, CancellationToken ct);
    
    // Event Sourcing methods
    Task<IEnumerable<Event>> GetEventsAsync(string aggregateId, CancellationToken ct);
    Task<Event?> GetEventByIdAsync(Guid eventId, CancellationToken ct);
    Task<IEnumerable<Event>> GetAllEventsAsync(CancellationToken ct);
    Task<long> GetNextVersionAsync(string aggregateId, CancellationToken ct);
}
