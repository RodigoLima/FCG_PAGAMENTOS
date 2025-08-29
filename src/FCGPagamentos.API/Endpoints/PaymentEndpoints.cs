using FCGPagamentos.Application.UseCases.CreatePayment;
using FCGPagamentos.Application.UseCases.GetPayment;
using FluentValidation;

namespace FCGPagamentos.API.Endpoints;
public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/payments", async (CreatePaymentCommand cmd, CreatePaymentHandler h, IValidator<CreatePaymentCommand> v, CancellationToken ct) =>
        {
            var vr = await v.ValidateAsync(cmd, ct);
            if (!vr.IsValid) return Results.ValidationProblem(vr.ToDictionary());
            var dto = await h.Handle(cmd, ct);
            // Retornar 202/201; vou usar 202 para indicar processamento assíncrono futuro
            return Results.Accepted($"/payments/{dto.Id}", dto);
        });

        app.MapGet("/payments/{id:guid}", async (Guid id, GetPaymentHandler h, CancellationToken ct) =>
        {
            var dto = await h.Handle(new GetPaymentQuery(id), ct);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        });

        app.MapHealthChecks("/health");
        return app;
    }
}
