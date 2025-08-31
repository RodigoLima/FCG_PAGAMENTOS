using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;

namespace FCGPagamentos.API.Endpoints;

public static class MetricsEndpoints
{
    public static IEndpointRouteBuilder MapMetricsEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoint para métricas Prometheus
        app.MapGet("/metrics", async (MeterProvider meterProvider) =>
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            
            // Exporta métricas no formato Prometheus
            var metrics = meterProvider.GetMeter("FCGPagamentos");
            
            // Aqui você pode adicionar lógica para exportar métricas específicas
            // Por enquanto, retorna um placeholder
            return Results.Ok("FCGPagamentos metrics endpoint - Prometheus format");
        })
        .WithName("GetMetrics")
        .WithSummary("Endpoint para métricas Prometheus");

        // Health check detalhado
        app.MapGet("/health/detailed", async (HealthCheckService healthCheckService) =>
        {
            var report = await healthCheckService.CheckHealthAsync();
            
            var result = new
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration,
                Entries = report.Entries.Select(e => new
                {
                    Name = e.Key,
                    Status = e.Value.Status.ToString(),
                    Description = e.Value.Description,
                    Duration = e.Value.Duration,
                    Tags = e.Value.Tags
                })
            };
            
            return Results.Ok(result);
        })
        .WithName("GetDetailedHealth")
        .WithSummary("Health check detalhado com todas as dependências");

        return app;
    }
}
