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
            IPaymentObservabilityService observability, HttpContext context) =>
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

                var p = await db.Payments.FirstOrDefaultAsync(x => x.Id == id);
                if (p is null) 
                {
                    observability.TrackPaymentFailure(id, 0, "Payment not found for internal processing", correlationId);
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
                
                // Observabilidade de sucesso
                stopwatch.Stop();
                observability.TrackPaymentSuccess(id, p.Value.Amount, correlationId, stopwatch.Elapsed);
                
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                observability.TrackPaymentFailure(id, 0, ex.Message, correlationId);
                throw;
            }
        })
        .ExcludeFromDescription(); // não expor no Swagger


        
        return app;
    }
}
