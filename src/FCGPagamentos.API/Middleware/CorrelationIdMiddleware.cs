using System.Diagnostics;

namespace FCGPagamentos.API.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extrai ou gera correlation ID
        var correlationId = GetOrGenerateCorrelationId(context);
        
        // Adiciona ao contexto para uso posterior
        context.Items["CorrelationId"] = correlationId;
        
        // Adiciona ao Activity.Current para OpenTelemetry
        if (Activity.Current != null)
        {
            Activity.Current.SetTag("correlation.id", correlationId);
        }
        
        // Adiciona header de resposta
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        
        await _next(context);
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Tenta extrair do header da requisição
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) && 
            !string.IsNullOrEmpty(correlationId))
        {
            return correlationId;
        }
        
        // Gera novo se não existir
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
