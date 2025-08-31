namespace FCGPagamentos.Domain.Models;

public abstract class Event
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Type { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public long Version { get; private set; }
    public string AggregateId { get; private set; }

    protected Event(string aggregateId, long version)
    {
        AggregateId = aggregateId;
        Version = version;
        Type = GetType().Name;
        OccurredAt = DateTime.UtcNow;
    }

    protected Event() { } // Para EF
}
