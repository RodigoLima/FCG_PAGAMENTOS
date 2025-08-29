using FluentValidation;
namespace FCGPagamentos.Application.UseCases.CreatePayment;
public class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.GameId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
