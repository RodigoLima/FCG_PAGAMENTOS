using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.UseCases.CreatePayment;
using FCGPagamentos.Application.UseCases.GetPayment;
using FCGPagamentos.Infrastructure.Clock;
using FCGPagamentos.Infrastructure.Persistence;
using FCGPagamentos.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FCGPagamentos.API.DI;
public static class CompositionRoot
{
    public static IServiceCollection AddAppServices(this IServiceCollection s, IConfiguration cfg)
    {
        s.AddDbContext<AppDbContext>(o => o.UseNpgsql(cfg.GetConnectionString("Postgres")));
        s.AddScoped<IPaymentRepository, PaymentRepository>();
        s.AddScoped<IEventStore, IEventStore>();
        s.AddSingleton<IClock, SystemClock>();

        s.AddScoped<CreatePaymentHandler>();
        s.AddScoped<GetPaymentHandler>();
        s.AddValidatorsFromAssemblyContaining<CreatePaymentValidator>();

        s.AddHealthChecks();
        return s;
    }
}
