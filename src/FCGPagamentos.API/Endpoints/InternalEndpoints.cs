using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FCGPagamentos.Infrastructure.Persistence;

namespace FCGPagamentos.API.Endpoints;

public static class InternalEndpoints
{
    public static IEndpointRouteBuilder MapInternal(this IEndpointRouteBuilder app)
    {
        app.MapPost("/internal/payments/{id:guid}/mark-processed", async (
            Guid id, AppDbContext db, HttpRequest req, IConfiguration cfg) =>
        {
            // Autorização simples entre serviços (segredo compartilhado)
            var token = req.Headers["x-internal-token"].ToString();
            if (string.IsNullOrEmpty(token) || token != cfg["InternalAuth:Token"])
                return Results.Unauthorized();

            var p = await db.Payments.FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return Results.NotFound();

            p.MarkProcessed(DateTime.UtcNow);
            db.Events.Add(new EventLog
            {
                Type = "PaymentProcessed",
                Payload = JsonSerializer.Serialize(new { p.Id }),
                OccurredAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .ExcludeFromDescription(); // não expor no Swagger
        return app;
    }
}
