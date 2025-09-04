namespace FCGPagamentos.Application.DTOs;

public sealed record PaymentRequestedMessage(
    Guid PaymentId,
    string CorrelationId,
    string UserId,
    string GameId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    DateTime OccurredAt,
    string Version = "1.0"
);
