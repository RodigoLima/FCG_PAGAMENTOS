namespace FCGPagamentos.Application.Abstractions;

public interface IPaymentProcessingPublisher
{
    Task PublishPaymentForProcessingAsync(
        Guid paymentId,
        CancellationToken ct);
}
