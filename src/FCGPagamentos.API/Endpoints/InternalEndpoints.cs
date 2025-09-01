using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FCGPagamentos.Infrastructure.Persistence;
using FCGPagamentos.Application.Abstractions;

namespace FCGPagamentos.API.Endpoints;

public static class InternalEndpoints
{
    public static IEndpointRouteBuilder MapInternal(this IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/payments/{id:guid}/mark-processed", async (
            Guid id, AppDbContext db, IEventStore eventStore, HttpRequest req, IConfiguration cfg) =>
        {
            // Autorização simples entre serviços (segredo compartilhado)
            var token = req.Headers["x-internal-token"].ToString();
            if (string.IsNullOrEmpty(token) || token != cfg["InternalAuth:Token"])
                return Results.Unauthorized();

            var p = await db.Payments.FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return Results.NotFound();

            p.MarkProcessed(DateTime.UtcNow);
            
            // Salva os eventos não commitados usando Event Sourcing
            foreach (var @event in p.UncommittedEvents)
            {
                await eventStore.AppendAsync(@event, @event.OccurredAt, CancellationToken.None);
            }
            
            // Marca os eventos como commitados
            p.MarkEventsAsCommitted();

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .ExcludeFromDescription(); // não expor no Swagger
        return app;
    }
}
