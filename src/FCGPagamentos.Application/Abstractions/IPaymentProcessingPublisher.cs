using FCGPagamentos.Application.DTOs;

namespace FCGPagamentos.Application.Abstractions;

public interface IPaymentProcessingPublisher
{
    Task PublishPaymentForProcessingAsync(
        PaymentRequestedMessage message,
        CancellationToken ct);
}
