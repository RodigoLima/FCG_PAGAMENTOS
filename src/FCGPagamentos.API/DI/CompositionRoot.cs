using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.UseCases.CreatePayment;
using FCGPagamentos.Application.UseCases.GetPayment;
using FCGPagamentos.Infrastructure.Clock;
using FCGPagamentos.Infrastructure.Persistence;
using FCGPagamentos.Infrastructure.Persistence.Repositories;
using FCGPagamentos.Infrastructure.Queues;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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

        // Event Sourcing
        s.AddScoped<IEventStore, EventStoreRepository>();

        // Repositórios / Adapters
        s.AddScoped<IPaymentRepository, PaymentRepository>();

        // Infra auxiliar
        s.AddSingleton<IClock, SystemClock>();

        // Casos de uso + validação
        s.AddScoped<CreatePaymentHandler>();
        s.AddScoped<GetPaymentHandler>();
        s.AddValidatorsFromAssemblyContaining<CreatePaymentValidator>();

        s.AddScoped<IPaymentProcessingPublisher, AzureQueuePaymentPublisher>();

        // Health checks
        s.AddHealthChecks();

        return s;
    }
}
