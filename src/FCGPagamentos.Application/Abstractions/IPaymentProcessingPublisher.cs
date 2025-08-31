namespace FCGPagamentos.Application.Abstractions;

public interface IPaymentProcessingPublisher
{
    Task PublishRequestedAsync(
        Guid paymentId, Guid userId, Guid gameId,
        decimal amount, string currency,
        CancellationToken ct);
}
