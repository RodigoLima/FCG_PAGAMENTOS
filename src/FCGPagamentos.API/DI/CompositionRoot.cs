using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.UseCases.CreatePayment;
using FCGPagamentos.Application.UseCases.GetPayment;
using FCGPagamentos.Infrastructure.Clock;
using FCGPagamentos.Infrastructure.Persistence;
using FCGPagamentos.Infrastructure.Persistence.Repositories;
using FCGPagamentos.Infrastructure.Queues;
using FCGPagamentos.API.Services;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Amazon;
using Amazon.SQS;
using Amazon.Runtime;

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

        // Configuração do MassTransit com AWS SQS
        var awsAccessKey = cfg["AWS:AccessKey"];
        var awsSecretKey = cfg["AWS:SecretKey"];
        var awsSessionToken = cfg["AWS:SessionToken"];
        var awsRegion = cfg["AWS:Region"] ?? "us-east-1";
        
        s.AddMassTransit(x =>
        {
            x.UsingAmazonSqs((context, cfg) =>
            {
                cfg.Host(awsRegion, h =>
                {
                    if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
                    {
                        if (!string.IsNullOrEmpty(awsSessionToken))
                        {
                            var credentials = new SessionAWSCredentials(awsAccessKey, awsSecretKey, awsSessionToken);
                            h.Credentials(credentials);
                        }
                        else
                        {
                            h.AccessKey(awsAccessKey);
                            h.SecretKey(awsSecretKey);
                        }
                    }
                });
            });
        });

        // Publisher
        s.AddScoped<IPaymentProcessingPublisher, SqsPaymentPublisher>();

        // Serviços
        s.AddScoped<IPaymentObservabilityService, PaymentObservabilityService>();
        
        // Health checks
        s.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<SqsHealthCheck>("queue");

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
