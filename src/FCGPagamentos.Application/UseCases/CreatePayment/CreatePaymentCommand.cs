namespace FCGPagamentos.Application.UseCases.CreatePayment;
public record CreatePaymentCommand(Guid Id, Guid UserId, Guid GameId, decimal Amount, string Currency, string Description, string PaymentMethod);
