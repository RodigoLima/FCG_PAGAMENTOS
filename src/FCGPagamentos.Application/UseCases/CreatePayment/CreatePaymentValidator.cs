using FluentValidation;
namespace FCGPagamentos.Application.UseCases.CreatePayment;
public class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId é obrigatório");
        RuleFor(x => x.CorrelationId).NotEmpty().WithMessage("CorrelationId é obrigatório");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount deve ser maior que zero");
        RuleFor(x => x.Currency).NotEmpty().Length(3).WithMessage("Currency deve ter 3 caracteres");
        RuleFor(x => x.Method).IsInEnum().WithMessage("Método de pagamento inválido");
    }
}
