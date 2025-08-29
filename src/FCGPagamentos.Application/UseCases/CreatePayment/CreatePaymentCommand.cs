namespace FCGPagamentos.Application.UseCases.CreatePayment;
public record CreatePaymentCommand(Guid UserId, Guid GameId, decimal Amount, string Currency = "BRL");
