namespace FCGPagamentos.Domain.Models;

public abstract class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public long Version { get; set; }
    public string AggregateId { get; set; } = string.Empty;

    protected Event(string aggregateId, long version)
    {
        AggregateId = aggregateId;
        Version = version;
        Type = GetType().Name;
        OccurredAt = DateTime.UtcNow;
    }

    protected Event() { } // Para EF
}
