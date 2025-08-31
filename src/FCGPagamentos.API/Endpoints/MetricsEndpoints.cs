using OpenTelemetry.Exporter.Prometheus.AspNetCore;

namespace FCGPagamentos.API.Endpoints;

public static class MetricsEndpoints
{
    public static IEndpointRouteBuilder MapMetricsEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoint para métricas do Prometheus
        app.MapGet("/metrics", async (HttpContext context) =>
        {
            // Redireciona para o endpoint padrão do Prometheus
            context.Response.Redirect("/metrics");
            return Results.Empty;
        });

        return app;
    }
}
