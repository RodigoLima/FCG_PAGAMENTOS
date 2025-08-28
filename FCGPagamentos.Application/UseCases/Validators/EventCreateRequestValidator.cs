using FCGPagamentos.Application.Domain.Event.Messages;
using FCGPagamentos.Application.Payments.DTOs.Requests;
using FluentValidation;

namespace FCGPagamentos.Application.Domain.Events.Validators;
public class EventCreateRequestValidator : AbstractValidator<EventCreateRequest>
{
  public EventCreateRequestValidator()
  {
    RuleFor(g => g.Id)
        .NotEmpty().WithMessage(EventMessages.General.NotFound);

    RuleFor(g => g.Type)
        .NotEmpty().WithMessage(EventMessages.Type.Required);

    RuleFor(g => g.Payload)
        .NotEmpty().WithMessage(EventMessages.Payload.Required);
  }
}