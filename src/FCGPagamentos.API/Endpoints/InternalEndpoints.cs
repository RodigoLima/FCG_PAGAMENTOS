using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FCGPagamentos.Infrastructure.Persistence;
using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.API.Services;
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
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
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

        app.MapGet("/internal/payments/{paymentId:guid}", async (
            Guid paymentId, AppDbContext db, HttpRequest req, IConfiguration cfg,
            IPaymentObservabilityService observability, HttpContext context) =>
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Observabilidade da requisição
                observability.TrackPaymentRequest(paymentId, 0, correlationId);
                
                // Autorização simples entre serviços (segredo compartilhado)
                var token = req.Headers["x-internal-token"].ToString();
                if (string.IsNullOrEmpty(token) || token != cfg["InternalAuth:Token"])
                {
                    observability.TrackPaymentFailure(paymentId, 0, "Unauthorized internal request", correlationId);
                    return Results.Unauthorized();
                }

                var payment = await db.Payments.FirstOrDefaultAsync(x => x.Id == paymentId);
                if (payment is null) 
                {
                    observability.TrackPaymentFailure(paymentId, 0, "Payment not found", correlationId);
                    return Results.NotFound();
                }

                // Retorna informações básicas do pagamento para a function
                var paymentInfo = new
                {
                    Id = payment.Id,
                    Amount = payment.Value.Amount,
                    Currency = payment.Value.Currency,
                    Status = payment.Status.ToString(),
                    Method = payment.Method.ToString(),
                    CreatedAt = payment.CreatedAt,
                    UpdatedAt = payment.UpdatedAt
                };
                
                // Observabilidade de sucesso
                stopwatch.Stop();
                observability.TrackPaymentSuccess(paymentId, payment.Value.Amount, correlationId, stopwatch.Elapsed);
                
                return Results.Ok(paymentInfo);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                observability.TrackPaymentFailure(paymentId, 0, ex.Message, correlationId);
                throw;
            }
        })
        .ExcludeFromDescription(); // não expor no Swagger

        
        return app;
    }
}
