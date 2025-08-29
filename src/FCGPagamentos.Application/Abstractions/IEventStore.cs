namespace FCGPagamentos.Application.Abstractions;

public interface IEventStore
{
    Task AppendAsync(string type, object payload, DateTime occurredAt, CancellationToken ct);

    Task AppendAsync<TEvent>(TEvent payload, DateTime occurredAt, CancellationToken ct);
}
