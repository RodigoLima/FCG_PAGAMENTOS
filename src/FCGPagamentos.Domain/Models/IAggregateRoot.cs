using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Models;

public interface IAggregateRoot
{
    long Version { get; }
    IReadOnlyCollection<Event> UncommittedEvents { get; }
    void MarkEventsAsCommitted();
    void LoadFromHistory(IEnumerable<Event> events);
}
