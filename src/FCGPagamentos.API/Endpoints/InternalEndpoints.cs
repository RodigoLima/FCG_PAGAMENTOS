using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FCGPagamentos.Infrastructure.Persistence;
using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.API.Services;
using System.Diagnostics;

namespace FCGPagamentos.API.Endpoints;

public static class InternalEndpoints
{
    public static IEndpointRouteBuilder MapInternal(this IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/payments/{id:guid}/mark-processed", async (
            Guid id, AppDbContext db, IEventStore eventStore, HttpRequest req, IConfiguration cfg,
            IStructuredLoggingService logging, BusinessMetricsService metrics, HttpContext context) =>
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Log da requisição
                logging.LogPaymentProcessing(id, "Internal request: mark-processed", correlationId);
                
                // Autorização simples entre serviços (segredo compartilhado)
                var token = req.Headers["x-internal-token"].ToString();
                if (string.IsNullOrEmpty(token) || token != cfg["InternalAuth:Token"])
                {
                    logging.LogPaymentFailure(id, 0, "Unauthorized internal request", correlationId);
                    return Results.Unauthorized();
                }

                var p = await db.Payments.FirstOrDefaultAsync(x => x.Id == id);
                if (p is null) 
                {
                    logging.LogPaymentFailure(id, 0, "Payment not found for internal processing", correlationId);
                    return Results.NotFound();
                }

                p.MarkProcessed(DateTime.UtcNow);
                
                // Salva os eventos não commitados usando Event Sourcing
                foreach (var @event in p.UncommittedEvents)
                {
                    await eventStore.AppendAsync(@event, @event.OccurredAt, CancellationToken.None);
                }
                
                // Marca os eventos como commitados
                p.MarkEventsAsCommitted();

                await db.SaveChangesAsync();
                
                // Log de sucesso
                stopwatch.Stop();
                logging.LogPaymentSuccess(id, p.Value.Amount, correlationId);
                metrics.RecordPaymentProcessingTime(stopwatch.Elapsed.TotalSeconds);
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logging.LogPaymentFailure(id, 0, ex.Message, correlationId);
                metrics.RecordPaymentFailure();
                throw;
            }
        })
        .ExcludeFromDescription(); // não expor no Swagger

        app.MapPut("/internal/payments/{id:guid}/update-value", async (
            Guid id, decimal newAmount, AppDbContext db, HttpRequest req, IConfiguration cfg,
            IStructuredLoggingService logging, BusinessMetricsService metrics, HttpContext context) =>
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Log da requisição
                logging.LogPaymentProcessing(id, $"Internal request: update-value to {newAmount}", correlationId);
                
                // Autorização simples entre serviços (segredo compartilhado)
                var token = req.Headers["x-internal-token"].ToString();
                if (string.IsNullOrEmpty(token) || token != cfg["InternalAuth:Token"])
                {
                    logging.LogPaymentFailure(id, 0, "Unauthorized internal request", correlationId);
                    return Results.Unauthorized();
                }

                var p = await db.Payments.FirstOrDefaultAsync(x => x.Id == id);
                if (p is null) 
                {
                    logging.LogPaymentFailure(id, 0, "Payment not found for internal update", correlationId);
                    return Results.NotFound();
                }

                // Atualiza o valor do pagamento
                p.Value = new FCGPagamentos.Domain.ValueObjects.Money(newAmount, "BRL");
                
                // O interceptor automaticamente atualizará o UpdatedAt aqui
                await db.SaveChangesAsync();
                
                // Log de sucesso
                stopwatch.Stop();
                logging.LogPaymentSuccess(id, newAmount, correlationId);
                metrics.RecordPaymentProcessingTime(stopwatch.Elapsed.TotalSeconds);
                
                return Results.Ok(new { 
                    Id = p.Id, 
                    Value = p.Value.Amount, 
                    CreatedAt = p.CreatedAt, 
                    UpdatedAt = p.UpdatedAt 
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logging.LogPaymentFailure(id, 0, ex.Message, correlationId);
                metrics.RecordPaymentFailure();
                throw;
            }
        })
        .ExcludeFromDescription(); // não expor no Swagger
        return app;
    }
}
