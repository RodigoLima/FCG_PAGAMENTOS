using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Diagnostics;

namespace FCGPagamentos.API.Services;

public static class ObservabilityService
{
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        // Registra o serviço de configuração
        services.AddSingleton<IObservabilityConfigurationService, ObservabilityConfigurationService>();
        
        // Configura Application Insights
        services.AddApplicationInsights(configuration);
        
        // Configura OpenTelemetry
        services.AddOpenTelemetry(configuration);
        
        return services;
    }

    private static IServiceCollection AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
    {
        var configService = new ObservabilityConfigurationService(configuration);
        
        if (configService.IsApplicationInsightsConfigured())
        {
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = configService.GetApplicationInsightsConnectionString();
                options.EnableAdaptiveSampling = true;
                options.EnableQuickPulseMetricStream = true;
                options.EnableDependencyTrackingTelemetryModule = true;
                options.EnableRequestTrackingTelemetryModule = true;
                options.EnableEventCounterCollectionModule = true;
                options.EnablePerformanceCounterCollectionModule = true;
            });
            
            Console.WriteLine("✅ Application Insights: CONFIGURADO com connection string");
        }
        else
        {
            services.AddApplicationInsightsTelemetry();
            Console.WriteLine("⚠️ Application Insights: CONFIGURADO sem connection string (auto-detection)");
        }
        
        return services;
    }

    private static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var configService = new ObservabilityConfigurationService(configuration);
        
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: "FCGPagamentos.API", serviceVersion: "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    ["service.instance.id"] = Environment.MachineName,
                    ["service.version"] = "1.0.0"
                }))
            .WithTracing(tracing => ConfigureTracing(tracing, configService))
            .WithMetrics(metrics => ConfigureMetrics(metrics, configService));

        return services;
    }

    private static void ConfigureTracing(TracerProviderBuilder tracing, IObservabilityConfigurationService config)
    {
        // ASP.NET Core Instrumentation
        tracing.AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequest = (activity, httpRequest) =>
            {
                activity.SetTag("http.request.body.size", httpRequest.ContentLength?.ToString());
                activity.SetTag("http.request.method", httpRequest.Method);
                activity.SetTag("http.request.url", $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.Path}{httpRequest.QueryString}");
            };
            options.EnrichWithHttpResponse = (activity, httpResponse) =>
            {
                activity.SetTag("http.response.body.size", httpResponse.ContentLength?.ToString());
                activity.SetTag("http.response.status_code", httpResponse.StatusCode.ToString());
            };
        });

        // HTTP Client Instrumentation
        tracing.AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
            {
                activity.SetTag("http.client.request.uri", httpRequestMessage.RequestUri?.ToString());
                activity.SetTag("http.client.request.method", httpRequestMessage.Method.ToString());
            };
            options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
            {
                activity.SetTag("http.client.response.status_code", httpResponseMessage.StatusCode.ToString());
            };
        });

        // Console Exporter (apenas para debug)
        if (config.IsConsoleExporterEnabled())
        {
            tracing.AddConsoleExporter();
            Console.WriteLine("🔧 OpenTelemetry Tracing: Console Exporter HABILITADO");
        }

        // Status do Application Insights
        if (config.IsApplicationInsightsConfigured())
        {
            Console.WriteLine("✅ OpenTelemetry Tracing: Integrado com Application Insights");
        }
        else
        {
            Console.WriteLine("⚠️ OpenTelemetry Tracing: Sem Application Insights");
        }
    }

    private static void ConfigureMetrics(MeterProviderBuilder metrics, IObservabilityConfigurationService config)
    {
        // ASP.NET Core Metrics
        metrics.AddAspNetCoreInstrumentation();

        // HTTP Client Metrics
        metrics.AddHttpClientInstrumentation();

        // Runtime Metrics (se disponível)
        try
        {
            metrics.AddRuntimeInstrumentation();
            Console.WriteLine("✅ OpenTelemetry Metrics: Runtime instrumentation HABILITADO");
        }
        catch
        {
            Console.WriteLine("⚠️ OpenTelemetry Metrics: Runtime instrumentation NÃO DISPONÍVEL");
        }

        // Status do Application Insights
        if (config.IsApplicationInsightsConfigured())
        {
            Console.WriteLine("✅ OpenTelemetry Metrics: Integrado com Application Insights");
        }
        else
        {
            Console.WriteLine("⚠️ OpenTelemetry Metrics: Sem Application Insights");
        }
    }
}
