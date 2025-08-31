using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.UseCases.CreatePayment;
using FCGPagamentos.Application.UseCases.GetPayment;
using FCGPagamentos.Infrastructure.Clock;
using FCGPagamentos.Infrastructure.Persistence;
using FCGPagamentos.Infrastructure.Persistence.Repositories;
using FCGPagamentos.Infrastructure.Queues;
using FCGPagamentos.API.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Serilog;
using System.Diagnostics.Metrics;

namespace FCGPagamentos.API.DI;

public static class CompositionRoot
{
    public static IServiceCollection AddAppServices(this IServiceCollection s, IConfiguration cfg)
    {
        var cs = ConnectionStringProvider.Resolve(cfg);

        // DbContext
        s.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(cs);
        });

        // Repositórios / Adapters
        s.AddScoped<IPaymentRepository, PaymentRepository>();
        s.AddScoped<IEventStore, EventStoreRepository>();    

        // Infra auxiliar
        s.AddSingleton<IClock, SystemClock>();

        // Casos de uso + validação
        s.AddScoped<CreatePaymentHandler>();
        s.AddScoped<GetPaymentHandler>();
        s.AddValidatorsFromAssemblyContaining<CreatePaymentValidator>();

        s.AddScoped<IPaymentProcessingPublisher, AzureQueuePaymentPublisher>();
        s.AddScoped<AzureQueuePaymentPublisher>(); // Registra também a classe concreta para o HealthCheck
        
        // Configuração de logging
        s.AddLogging();

        // Serviços de observabilidade
        s.AddSingleton<BusinessMetricsService>(provider => 
        {
            var meter = new Meter("FCGPagamentos", "1.0.0");
            return new BusinessMetricsService(meter);
        });
        s.AddScoped<IStructuredLoggingService, StructuredLoggingService>();
        
        // Health checks customizados
        s.AddScoped<DatabaseHealthCheck>();
        s.AddScoped<AzureQueueHealthCheck>();

        // Health checks
        s.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<AzureQueueHealthCheck>("azure_queue");

        return s;
    }

    public static IServiceCollection AddObservability(this IServiceCollection s, IConfiguration cfg)
    {
        // Configuração do OpenTelemetry
        s.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: "FCGPagamentos.API", serviceVersion: "1.0.0"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                // Removed EntityFrameworkCore instrumentation as it only has beta versions
                .AddHttpClientInstrumentation()
                // Removed Jaeger exporter as it only has RC versions
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());

        return s;
    }

    public static IHostBuilder AddSerilog(this IHostBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/fcg-pagamentos-.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "FCGPagamentos.API")
            .CreateLogger();

        builder.UseSerilog();

        return builder;
    }
}
