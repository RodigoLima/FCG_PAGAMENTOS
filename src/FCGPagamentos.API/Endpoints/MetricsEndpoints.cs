using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FCGPagamentos.API.Endpoints;

public static class MetricsEndpoints
{
    public static IEndpointRouteBuilder MapMetricsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async (HealthCheckService healthCheckService) =>
        {
            var report = await healthCheckService.CheckHealthAsync();
            
            var checks = new Dictionary<string, object>();
            foreach (var entry in report.Entries)
            {
                checks[entry.Key] = new
                {
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    Exception = entry.Value.Exception?.Message,
                    Data = entry.Value.Data
                };
            }
            
            var statusCode = report.Status == HealthStatus.Healthy ? 200 : 503;
            
            return Results.Json(new
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration.TotalMilliseconds,
                Checks = checks
            }, statusCode: statusCode);
        })
        .ExcludeFromDescription();

        return app;
    }
}
