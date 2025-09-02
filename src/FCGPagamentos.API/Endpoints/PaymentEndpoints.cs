using FCGPagamentos.Application.UseCases.CreatePayment;
using FCGPagamentos.Application.UseCases.GetPayment;
using FCGPagamentos.API.Services;
using FCGPagamentos.API.Models;
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
        app.MapPost("/payments", async (CreatePaymentRequest request, CreatePaymentHandler h, IValidator<CreatePaymentCommand> v, 
            IPaymentObservabilityService observability, HttpContext context, CancellationToken ct) =>
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            var paymentId = Guid.NewGuid();
            
            try
            {
                // Criar comando a partir do request
                var cmd = new CreatePaymentCommand(paymentId, request.OrderId, correlationId, request.Amount, request.Currency, request.Method);
                
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
                
                // Retornar 201 Created conforme especificação
                return Results.Created($"/payments/{dto.Id}", new { payment_id = dto.Id, status = "pending" });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                observability.TrackPaymentFailure(paymentId, request.Amount, ex.Message, correlationId);
                observability.TrackException(ex, new Dictionary<string, string>
                {
                    ["PaymentId"] = paymentId.ToString(),
                    ["CorrelationId"] = correlationId,
                    ["Operation"] = "CreatePayment"
                });
                throw;
            }
        })
        .WithName("CreatePayment")
        .WithSummary("Cria um novo pagamento")
        .WithDescription("Registra a intenção de pagamento (não processa ainda)")
        .WithTags("Payments")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/payments/{payment_id:guid}", async (Guid payment_id, GetPaymentHandler h, 
            IPaymentObservabilityService observability, HttpContext context, CancellationToken ct) =>
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            try
            {
                var dto = await h.Handle(new GetPaymentQuery(payment_id), ct);
                
                if (dto is null)
                {
                    observability.TrackPaymentFailure(payment_id, 0, "Payment not found", correlationId);
                    return Results.NotFound();
                }
                
                observability.TrackPaymentSuccess(payment_id, dto.Amount, correlationId, TimeSpan.Zero);
                
                // Retornar formato conforme especificação
                return Results.Ok(new { 
                    payment_id = dto.Id, 
                    status = dto.Status.ToString().ToLower(), 
                    last_update_at = dto.LastUpdateAt 
                });
            }
            catch (Exception ex)
            {
                observability.TrackPaymentFailure(payment_id, 0, ex.Message, correlationId);
                throw;
            }
        })
        .WithName("GetPayment")
        .WithSummary("Consulta status do pagamento")
        .WithDescription("Consulta o status atual de um pagamento específico")
        .WithTags("Payments")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
