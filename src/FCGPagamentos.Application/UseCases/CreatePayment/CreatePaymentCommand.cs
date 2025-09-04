using FCGPagamentos.Domain.Enums;

namespace FCGPagamentos.Application.UseCases.CreatePayment;
public record CreatePaymentCommand(Guid Id, string UserId, string GameId, string CorrelationId, decimal Amount, string Currency, PaymentMethod Method);
