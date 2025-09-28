using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FCGPagamentos.Infrastructure.Persistence;
using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.API.Services;
using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Application.DTOs;
using FCGPagamentos.Domain.ValueObjects;
using FCGPagamentos.Domain.Enums;
using System.Diagnostics;

namespace FCGPagamentos.API.Endpoints;

public static class InternalEndpoints
{
    private static async Task<IResult> ProcessPaymentStatusChange(
        Guid id, 
        AppDbContext db, 
        IEventStore eventStore, 
        HttpRequest req, 
        IConfiguration cfg,
        IPaymentObservabilityService observability, 
        HttpContext context,
        Action<Payment> statusChangeAction)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Observabilidade da requisição
            observability.TrackPaymentRequest(id, 0, correlationId);
            
            // Autorização simples entre serviços (segredo compartilhado)
            var token = req.Headers["x-internal-token"].ToString();
            if (string.IsNullOrEmpty(token) || token != cfg["InternalAuth:Token"])
            {
                observability.TrackPaymentFailure(id, 0, "Unauthorized internal request", correlationId);
                return Results.Unauthorized();
            }

            var payment = await db.Payments.FirstOrDefaultAsync(x => x.Id == id);
            if (payment is null) 
            {
                observability.TrackPaymentFailure(id, 0, "Payment not found for internal processing", correlationId);
                return Results.NotFound();
            }

            // Aplica a mudança de status
            statusChangeAction(payment);
            
            // Salva os eventos não commitados usando Event Sourcing
            foreach (var @event in payment.UncommittedEvents)
            {
                await eventStore.AppendAsync(@event, @event.OccurredAt, CancellationToken.None);
            }
            
            // Marca os eventos como commitados
            payment.MarkEventsAsCommitted();

            await db.SaveChangesAsync();
            
            // Observabilidade de sucesso
            stopwatch.Stop();
            observability.TrackPaymentSuccess(id, payment.Value.Amount, correlationId, stopwatch.Elapsed);
            
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            observability.TrackPaymentFailure(id, 0, ex.Message, correlationId);
            throw;
        }
    }

    public static IEndpointRouteBuilder MapInternal(this IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/payments/{id:guid}/mark-processing", async (
            Guid id, AppDbContext db, IEventStore eventStore, HttpRequest req, IConfiguration cfg,
            IPaymentObservabilityService observability, HttpContext context) =>
        {
            return await ProcessPaymentStatusChange(id, db, eventStore, req, cfg, observability, context, 
                p => p.MarkProcessing(DateTime.UtcNow));
        })
        .ExcludeFromDescription(); // não expor no Swagger

        app.MapPost("/internal/payments/{id:guid}/mark-approved", async (
            Guid id, AppDbContext db, IEventStore eventStore, HttpRequest req, IConfiguration cfg,
            IPaymentObservabilityService observability, HttpContext context) =>
        {
            return await ProcessPaymentStatusChange(id, db, eventStore, req, cfg, observability, context, 
                p => p.MarkApproved(DateTime.UtcNow));
        })
        .ExcludeFromDescription(); // não expor no Swagger

        app.MapPost("/internal/payments/{id:guid}/mark-declined", async (
            Guid id, AppDbContext db, IEventStore eventStore, HttpRequest req, IConfiguration cfg,
            IPaymentObservabilityService observability, HttpContext context) =>
        {
            return await ProcessPaymentStatusChange(id, db, eventStore, req, cfg, observability, context, 
                p => p.MarkDeclined(DateTime.UtcNow, "Payment declined by processor"));
        })
        .ExcludeFromDescription(); // não expor no Swagger

        app.MapPost("/internal/payments/{id:guid}/mark-failed", async (
            Guid id, AppDbContext db, IEventStore eventStore, HttpRequest req, IConfiguration cfg,
            IPaymentObservabilityService observability, HttpContext context) =>
        {
            return await ProcessPaymentStatusChange(id, db, eventStore, req, cfg, observability, context, 
                p => p.MarkFailed(DateTime.UtcNow, "Payment processing failed"));
        })
        .ExcludeFromDescription(); // não expor no Swagger

        // Endpoint para receber eventos de outros microserviços para criar pagamentos
        app.MapPost("/internal/payments", async (
            PaymentRequestedMessage request, 
            AppDbContext db, 
            IEventStore eventStore, 
            HttpRequest req, 
            IConfiguration cfg,
            IPaymentObservabilityService observability, 
            IPaymentProcessingPublisher publisher,
            HttpContext context,
            CancellationToken ct) =>
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? request.CorrelationId ?? Guid.NewGuid().ToString();
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Autorização simples entre serviços
                var token = req.Headers["x-internal-token"].ToString();
                if (string.IsNullOrEmpty(token) || token != cfg["InternalAuth:Token"])
                {
                    observability.TrackPaymentFailure(request.PaymentId, 0, "Unauthorized internal request", correlationId);
                    return Results.Unauthorized();
                }

                // Verifica se o pagamento já existe
                var existingPayment = await db.Payments.FirstOrDefaultAsync(x => x.Id == request.PaymentId);
                if (existingPayment != null)
                {
                    observability.TrackPaymentFailure(request.PaymentId, request.Amount, "Payment already exists", correlationId);
                    return Results.Conflict(new { message = "Payment already exists" });
                }

                // Cria o pagamento
                var money = new Money(request.Amount, request.Currency);
                var paymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod);
                var payment = new Payment(
                    request.UserId, 
                    request.GameId, 
                    correlationId, 
                    money, 
                    paymentMethod, 
                    request.OccurredAt
                );

                // Salva o pagamento primeiro
                db.Payments.Add(payment);
                await db.SaveChangesAsync();
                
                // Depois salva os eventos usando Event Sourcing
                foreach (var @event in payment.UncommittedEvents)
                {
                    await eventStore.AppendAsync(@event, @event.OccurredAt, CancellationToken.None);
                }
                
                payment.MarkEventsAsCommitted();
                
                // Criar mensagem com dados do banco (não da request)
                var message = new PaymentRequestedMessage(
                    payment.Id,
                    payment.CorrelationId,
                    payment.UserId,
                    payment.GameId,
                    payment.Value.Amount,
                    payment.Value.Currency,
                    payment.Method.ToString(),
                    payment.CreatedAt
                );
                
                // Publicar na fila para processamento
                await publisher.PublishPaymentForProcessingAsync(message, ct);
                
                stopwatch.Stop();
                observability.TrackPaymentSuccess(request.PaymentId, request.Amount, correlationId, stopwatch.Elapsed);
                
                return Results.Created($"/internal/payments/{request.PaymentId}", new { id = request.PaymentId });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                observability.TrackPaymentFailure(request.PaymentId, request.Amount, ex.Message, correlationId);
                return Results.Problem($"Erro ao criar pagamento: {ex.Message}");
            }
        })
        .ExcludeFromDescription(); // não expor no Swagger
        
        return app;
    }
}
