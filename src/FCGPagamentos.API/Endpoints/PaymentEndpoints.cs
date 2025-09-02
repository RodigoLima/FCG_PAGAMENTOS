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
            BusinessMetricsService metrics, IStructuredLoggingService logging, ITelemetryService telemetry, HttpContext context, CancellationToken ct) =>
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            try
            {
                // Log da requisição
                logging.LogPaymentRequest(cmd.Id, cmd.Amount, correlationId);
                
                // Telemetria da requisição
                telemetry.TrackPaymentRequest(cmd.Id, cmd.Amount, correlationId);
                
                // Métrica de requisição
                metrics.RecordPaymentRequest();
                
                // Validação
                var vr = await v.ValidateAsync(cmd, ct);
                if (!vr.IsValid) 
                {
                    metrics.RecordPaymentFailure();
                    logging.LogPaymentFailure(cmd.Id, cmd.Amount, "Validation failed", correlationId);
                    telemetry.TrackPaymentFailure(cmd.Id, cmd.Amount, "Validation failed", correlationId);
                    return Results.ValidationProblem(vr.ToDictionary());
                }
                
                // Processamento
                var dto = await h.Handle(cmd, ct);
                
                // Log de sucesso
                logging.LogPaymentSuccess(dto.Id, dto.Amount, correlationId);
                
                // Tempo de processamento
                stopwatch.Stop();
                
                // Telemetria de sucesso
                telemetry.TrackPaymentSuccess(dto.Id, dto.Amount, correlationId, stopwatch.Elapsed);
                
                // Métricas de sucesso
                metrics.RecordPaymentSuccess();
                metrics.RecordPaymentAmount(dto.Amount);
                metrics.RecordPaymentProcessingTime(stopwatch.Elapsed.TotalSeconds);
                
                // Retornar 202 para indicar processamento assíncrono
                return Results.Accepted($"/payments/{dto.Id}", dto);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                metrics.RecordPaymentFailure();
                logging.LogPaymentFailure(cmd.Id, cmd.Amount, ex.Message, correlationId);
                telemetry.TrackPaymentFailure(cmd.Id, cmd.Amount, ex.Message, correlationId);
                telemetry.TrackException(ex, new Dictionary<string, string>
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

        // Endpoint para buscar eventos de um pagamento específico
        app.MapGet("/payments/{paymentId:guid}/events", async (Guid paymentId, AppDbContext db, 
            IStructuredLoggingService logging, HttpContext context, CancellationToken ct) =>
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            try
            {
                logging.LogPaymentProcessing(paymentId, $"Buscando eventos do pagamento {paymentId}", correlationId);
                
                var events = await db.Events
                    .Where(e => e.AggregateId == paymentId.ToString())
                    .OrderBy(e => e.Version)
                    .Select(e => new {
                        e.EventId,
                        e.Type,
                        e.Payload,
                        e.OccurredAt,
                        e.Version,
                        e.AggregateId
                    })
                    .ToListAsync(ct);
                
                if (!events.Any())
                {
                    logging.LogPaymentProcessing(paymentId, "Nenhum evento encontrado para o pagamento", correlationId);
                    return Results.NotFound(new { message = "Nenhum evento encontrado para este pagamento" });
                }
                
                logging.LogPaymentProcessing(paymentId, $"Encontrados {events.Count} eventos", correlationId);
                return Results.Ok(events);
            }
            catch (Exception ex)
            {
                logging.LogPaymentFailure(paymentId, 0, ex.Message, correlationId);
                throw;
            }
        })
        .WithName("GetPaymentEvents")
        .WithSummary("Obtém todos os eventos de um pagamento (Event Sourcing)")
        .WithDescription("Recupera o histórico completo de eventos de um pagamento específico para auditoria")
        .WithTags("Events")
        .Produces<object[]>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        // Endpoint para buscar um evento específico pelo ID
        app.MapGet("/events/{eventId:guid}", async (Guid eventId, AppDbContext db, 
            IStructuredLoggingService logging, HttpContext context, CancellationToken ct) =>
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            try
            {
                logging.LogPaymentProcessing(eventId, $"Buscando evento específico {eventId}", correlationId);
                
                var @event = await db.Events
                    .Where(e => e.EventId == eventId)
                    .Select(e => new {
                        e.EventId,
                        e.Type,
                        e.Payload,
                        e.OccurredAt,
                        e.Version,
                        e.AggregateId
                    })
                    .FirstOrDefaultAsync(ct);
                
                if (@event is null)
                {
                    logging.LogPaymentProcessing(eventId, "Evento não encontrado", correlationId);
                    return Results.NotFound(new { message = "Evento não encontrado" });
                }
                
                logging.LogPaymentProcessing(eventId, "Evento encontrado com sucesso", correlationId);
                return Results.Ok(@event);
            }
            catch (Exception ex)
            {
                logging.LogPaymentFailure(eventId, 0, ex.Message, correlationId);
                throw;
            }
        })
        .WithName("GetEventById")
        .WithSummary("Obtém um evento específico pelo ID")
        .WithDescription("Recupera um evento específico do Event Store pelo seu ID único")
        .WithTags("Events")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        // Endpoint para listar todos os eventos (útil para auditoria e debugging)
        app.MapGet("/events", async (AppDbContext db, IStructuredLoggingService logging, 
            HttpContext context, CancellationToken ct) =>
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            try
            {
                logging.LogPaymentProcessing(Guid.Empty, "Listando todos os eventos", correlationId);
                
                // Busca todos os eventos do banco diretamente
                var allEvents = await db.Events
                    .OrderBy(e => e.OccurredAt)
                    .ThenBy(e => e.Version)
                    .Select(e => new {
                        e.EventId,
                        e.Type,
                        e.Payload,
                        e.OccurredAt,
                        e.Version,
                        e.AggregateId
                    })
                    .ToListAsync(ct);
                
                logging.LogPaymentProcessing(Guid.Empty, $"Encontrados {allEvents.Count} eventos no total", correlationId);
                return Results.Ok(allEvents);
            }
            catch (Exception ex)
            {
                logging.LogPaymentFailure(Guid.Empty, 0, ex.Message, correlationId);
                throw;
            }
        })
        .WithName("GetAllEvents")
        .WithSummary("Lista todos os eventos do sistema")
        .WithDescription("Recupera todos os eventos armazenados no Event Store para auditoria e debugging")
        .WithTags("Events")
        .Produces<object[]>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        // Endpoint de debug para verificar eventos no banco
        app.MapGet("/debug/events-raw", async (AppDbContext db, CancellationToken ct) =>
        {
            var rawEvents = await db.Events.ToListAsync(ct);
            return Results.Ok(rawEvents);
        })
        .WithName("DebugRawEvents")
        .WithSummary("Debug: Lista eventos brutos do banco")
        .WithDescription("Endpoint de debug para verificar se os eventos estão sendo salvos no banco")
        .WithTags("Debug");

        app.MapHealthChecks("/health");
        return app;
    }
}
