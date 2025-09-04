using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.DTOs;
using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.ValueObjects;

namespace FCGPagamentos.Application.UseCases.CreatePayment;

public class CreatePaymentHandler
{
    private readonly IPaymentRepository _repo;
    private readonly IClock _clock;
    private readonly IPaymentProcessingPublisher _publisher; 

    public CreatePaymentHandler(
        IPaymentRepository repo,
        IClock clock,
        IPaymentProcessingPublisher publisher)           
    {
        _repo = repo;
        _clock = clock;
        _publisher = publisher;                          
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand cmd, CancellationToken ct)
    {
        var now = _clock.UtcNow;

        var payment = new Payment(cmd.UserId, cmd.GameId, cmd.CorrelationId, new Money(cmd.Amount, cmd.Currency), cmd.Method, now);
        await _repo.AddAsync(payment, ct);

        await _repo.SaveChangesAsync(ct);

        // Criar mensagem completa para processamento
        var message = new PaymentRequestedMessage(
            payment.Id,
            payment.CorrelationId,
            payment.UserId,
            payment.GameId,
            payment.Value.Amount,
            payment.Value.Currency,
            payment.Method.ToString(),
            now
        );

        // Enfileirar mensagem para processamento após PaymentQueued
        await _publisher.PublishPaymentForProcessingAsync(message, ct);

        return new PaymentDto(payment.Id, payment.UserId, payment.GameId, cmd.Amount, cmd.Currency, cmd.Method,
                              payment.Status, payment.CreatedAt, payment.UpdatedAt, payment.ProcessedAt);
    }
}
