using FCGPagamentos.Application.UseCases.CreatePayment;
using FCGPagamentos.Application.UseCases.GetPayment;
using FCGPagamentos.API.Services;
using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Domain.Events;
using FluentValidation;
using System.Diagnostics;

namespace FCGPagamentos.API.Endpoints;
public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/payments", async (CreatePaymentCommand cmd, CreatePaymentHandler h, IValidator<CreatePaymentCommand> v, 
            BusinessMetricsService metrics, IStructuredLoggingService logging, HttpContext context, CancellationToken ct) =>
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            try
            {
                // Log da requisição
                logging.LogPaymentRequest(cmd.Id, cmd.Amount, correlationId);
                
                // Métrica de requisição
                metrics.RecordPaymentRequest();
                
                // Validação
                var vr = await v.ValidateAsync(cmd, ct);
                if (!vr.IsValid) 
                {
                    metrics.RecordPaymentFailure();
                    logging.LogPaymentFailure(cmd.Id, cmd.Amount, "Validation failed", correlationId);
                    return Results.ValidationProblem(vr.ToDictionary());
                }
                
                // Processamento
                var dto = await h.Handle(cmd, ct);
                
                // Log de sucesso
                logging.LogPaymentSuccess(dto.Id, dto.Amount, correlationId);
                
                // Métricas de sucesso
                metrics.RecordPaymentSuccess();
                metrics.RecordPaymentAmount(dto.Amount);
                
                // Tempo de processamento
                stopwatch.Stop();
                metrics.RecordPaymentProcessingTime(stopwatch.Elapsed.TotalSeconds);
                
                // Retornar 202 para indicar processamento assíncrono
                return Results.Accepted($"/payments/{dto.Id}", dto);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                metrics.RecordPaymentFailure();
                logging.LogPaymentFailure(cmd.Id, cmd.Amount, ex.Message, correlationId);
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
            IStructuredLoggingService logging, HttpContext context, CancellationToken ct) =>
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            try
            {
                var dto = await h.Handle(new GetPaymentQuery(id), ct);
                
                if (dto is null)
                {
                    logging.LogPaymentProcessing(id, "Payment not found", correlationId);
                    return Results.NotFound();
                }
                
                logging.LogPaymentProcessing(id, "Payment retrieved successfully", correlationId);
                return Results.Ok(dto);
            }
            catch (Exception ex)
            {
                logging.LogPaymentFailure(id, 0, ex.Message, correlationId);
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

        // Endpoint para testar Event Sourcing
        app.MapGet("/payments/{id:guid}/events", async (Guid id, IEventStore eventStore, CancellationToken ct) =>
        {
            var events = await eventStore.GetEventsAsync(id.ToString(), ct);
            return Results.Ok(events);
        })
        .WithName("GetPaymentEvents")
        .WithSummary("Obtém todos os eventos de um pagamento (Event Sourcing)")
        .WithDescription("Recupera o histórico completo de eventos de um pagamento para auditoria")
        .WithTags("Events")
        .Produces<object[]>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        app.MapHealthChecks("/health");
        return app;
    }
}
