using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FCGPagamentos.API.Endpoints;

public static class MetricsEndpoints
{
    public static IEndpointRouteBuilder MapMetricsEndpoints(this IEndpointRouteBuilder app)
    {
        // Health check básico
        app.MapGet("/health", async (HealthCheckService healthCheckService) =>
        {
            var report = await healthCheckService.CheckHealthAsync();
            return Results.Ok(new { Status = report.Status.ToString() });
        })
        .ExcludeFromDescription();

        return app;
    }
}
