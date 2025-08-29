namespace FCGPagamentos.Application.Events;

public sealed record PaymentRequested(
    Guid PaymentId,
    Guid UserId,
    Guid GameId,
    decimal Amount,
    string Currency,
    DateTime OccurredAt,
    string Version = "1.0"
);
