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

        
        // Configura Application Insights
        services.AddApplicationInsights(configuration);
        
        // Configura OpenTelemetry
        services.AddOpenTelemetry(configuration);
        
        return services;
    }

    private static IServiceCollection AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ApplicationInsights:ConnectionString"] 
            ?? configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
            ?? configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = connectionString;
                options.EnableAdaptiveSampling = true;
                options.EnableQuickPulseMetricStream = true;
                options.EnableDependencyTrackingTelemetryModule = true;
                options.EnableRequestTrackingTelemetryModule = true;
                options.EnableEventCounterCollectionModule = true;
                options.EnablePerformanceCounterCollectionModule = true;
            });
        }
        else
        {
            services.AddApplicationInsightsTelemetry();
        }
        
        return services;
    }

    private static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: "FCGPagamentos.API", serviceVersion: "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    ["service.instance.id"] = Environment.MachineName,
                    ["service.version"] = "1.0.0"
                }))
            .WithTracing(tracing => ConfigureTracing(tracing, configuration))
            .WithMetrics(metrics => ConfigureMetrics(metrics));

        return services;
    }

    private static void ConfigureTracing(TracerProviderBuilder tracing, IConfiguration configuration)
    {
        // ASP.NET Core Instrumentation
        tracing.AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
        });

        // HTTP Client Instrumentation
        tracing.AddHttpClientInstrumentation();

        // Console Exporter (apenas para debug)
        if (configuration.GetValue<bool>("OpenTelemetry:EnableConsoleExporter", false))
        {
            tracing.AddConsoleExporter();
        }
    }

    private static void ConfigureMetrics(MeterProviderBuilder metrics)
    {
        // ASP.NET Core Metrics
        metrics.AddAspNetCoreInstrumentation();

        // HTTP Client Metrics
        metrics.AddHttpClientInstrumentation();

        // Runtime Metrics (se disponível)
        try
        {
            metrics.AddRuntimeInstrumentation();
        }
        catch
        {
            // Runtime instrumentation não disponível
        }
    }
}
