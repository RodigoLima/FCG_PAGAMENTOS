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
using Serilog;

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

        // Publisher baseado no ambiente
        var isDevelopment = cfg.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";
        var useAzureEmulator = cfg.GetValue<bool>("AzureStorage:UseEmulator", true);
        
        if (isDevelopment && !useAzureEmulator)
        {
            s.AddScoped<IPaymentProcessingPublisher, MockPaymentPublisher>();
        }
        else
        {
            s.AddScoped<IPaymentProcessingPublisher, AzureQueuePaymentPublisher>();
        }

        // Serviços
        s.AddScoped<IPaymentObservabilityService, PaymentObservabilityService>();
        
        // Health checks
        var healthChecks = s.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");
        
        if (isDevelopment && !useAzureEmulator)
        {
            healthChecks.AddCheck<MockQueueHealthCheck>("queue");
        }
        else
        {
            healthChecks.AddCheck<AzureQueueHealthCheck>("queue");
        }

        return s;
    }

    // Método removido - configuração movida para ObservabilityService

    public static IHostBuilder AddSerilog(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "FCGPagamentos.API")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console()
                .WriteTo.File("logs/fcg-pagamentos-.txt", rollingInterval: RollingInterval.Day);

            // Serilog configurado
        });
    }
}
