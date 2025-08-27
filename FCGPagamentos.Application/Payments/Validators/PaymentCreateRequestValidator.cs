using FCGPagamentos.Application.Domain.Payments.Messages;
using FCGPagamentos.Application.Payments.DTOs.Requests;
using FluentValidation;

namespace FCGPagamentos.Application.Domain.Payments.Validators;
public class PaymentCreateRequestValidator : AbstractValidator<PaymentCreateRequest>
{
  public PaymentCreateRequestValidator()
  {
    RuleFor(g => g.UserId)
        .NotEmpty().WithMessage(PaymentMessages.UserId.Required);

    RuleFor(g => g.GameId)
        .NotEmpty().WithMessage(PaymentMessages.GameId.Required);

    RuleFor(g => g.Amount)
        .GreaterThan(0).WithMessage(PaymentMessages.Amount.GreaterThanZero);
  }
}