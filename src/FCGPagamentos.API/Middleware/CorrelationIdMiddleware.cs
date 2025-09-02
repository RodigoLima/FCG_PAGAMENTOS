using System.Diagnostics;

namespace FCGPagamentos.API.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string TraceParentHeader = "traceparent";
    private const string TraceStateHeader = "tracestate";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrGenerateCorrelationId(context);
        
        // Adiciona ao contexto para uso nos endpoints
        context.Items["CorrelationId"] = correlationId;
        
        // Adiciona ao header de resposta
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        
        // Configura o Activity para OpenTelemetry/Application Insights
        if (Activity.Current != null)
        {
            Activity.Current.SetTag("correlation.id", correlationId);
            Activity.Current.SetTag("service.name", "FCGPagamentos.API");
            Activity.Current.SetTag("service.version", "1.0.0");
            
            // Adiciona informações do traceparent se disponível
            if (context.Request.Headers.TryGetValue(TraceParentHeader, out var traceParent))
            {
                Activity.Current.SetTag("traceparent", traceParent.ToString());
            }
            
            if (context.Request.Headers.TryGetValue(TraceStateHeader, out var traceState))
            {
                Activity.Current.SetTag("tracestate", traceState.ToString());
            }
        }
        
        await _next(context);
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Tenta obter do header da requisição
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            return correlationId.ToString();
        }
        
        // Gera um novo se não existir
        return Guid.NewGuid().ToString();
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
