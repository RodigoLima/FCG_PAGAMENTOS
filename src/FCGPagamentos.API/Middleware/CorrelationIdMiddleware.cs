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
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        
        // Configura tags básicas para observabilidade
        if (Activity.Current != null)
        {
            Activity.Current.SetTag("correlation.id", correlationId);
            Activity.Current.SetTag("service.name", "FCGPagamentos.API");
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
