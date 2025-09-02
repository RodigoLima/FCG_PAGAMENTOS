using FCGPagamentos.Application.UseCases.CreatePayment;
using FCGPagamentos.Application.UseCases.GetPayment;
using FCGPagamentos.API.Services;
using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Domain.Events;
using FCGPagamentos.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FCGPagamentos.API.Endpoints;
public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/payments", async (CreatePaymentCommand cmd, CreatePaymentHandler h, IValidator<CreatePaymentCommand> v, 
            IPaymentObservabilityService observability, HttpContext context, CancellationToken ct) =>
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            try
            {
                // Observabilidade da requisição
                observability.TrackPaymentRequest(cmd.Id, cmd.Amount, correlationId);
                

                
                // Validação
                var vr = await v.ValidateAsync(cmd, ct);
                if (!vr.IsValid) 
                {
                    observability.TrackPaymentFailure(cmd.Id, cmd.Amount, "Validation failed", correlationId);
                    return Results.ValidationProblem(vr.ToDictionary());
                }
                
                // Processamento
                var dto = await h.Handle(cmd, ct);
                
                // Tempo de processamento
                stopwatch.Stop();
                
                // Observabilidade de sucesso
                observability.TrackPaymentSuccess(dto.Id, dto.Amount, correlationId, stopwatch.Elapsed);
                

                
                // Retornar 202 para indicar processamento assíncrono
                return Results.Accepted($"/payments/{dto.Id}", dto);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                observability.TrackPaymentFailure(cmd.Id, cmd.Amount, ex.Message, correlationId);
                observability.TrackException(ex, new Dictionary<string, string>
                {
                    ["PaymentId"] = cmd.Id.ToString(),
                    ["CorrelationId"] = correlationId,
                    ["Operation"] = "CreatePayment"
                });
                throw;
            }
        })
        .WithName("CreatePayment")
        .WithSummary("Cria um novo pagamento")
        .WithDescription("Cria um novo pagamento no sistema e retorna o ID do pagamento criado")
        .WithTags("Payments")
        .Produces<FCGPagamentos.Application.DTOs.PaymentDto>(StatusCodes.Status202Accepted)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/payments/{id:guid}", async (Guid id, GetPaymentHandler h, 
            IPaymentObservabilityService observability, HttpContext context, CancellationToken ct) =>
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            try
            {
                var dto = await h.Handle(new GetPaymentQuery(id), ct);
                
                if (dto is null)
                {
                    observability.TrackPaymentFailure(id, 0, "Payment not found", correlationId);
                    return Results.NotFound();
                }
                
                observability.TrackPaymentSuccess(id, dto.Amount, correlationId, TimeSpan.Zero);
                return Results.Ok(dto);
            }
            catch (Exception ex)
            {
                observability.TrackPaymentFailure(id, 0, ex.Message, correlationId);
                throw;
            }
        })
        .WithName("GetPayment")
        .WithSummary("Obtém um pagamento por ID")
        .WithDescription("Recupera as informações de um pagamento específico pelo seu ID")
        .WithTags("Payments")
        .Produces<FCGPagamentos.Application.DTOs.PaymentDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
