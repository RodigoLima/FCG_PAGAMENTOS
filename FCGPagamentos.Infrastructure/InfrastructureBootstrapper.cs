using FCGPagamentos.Application.IRepository.Base;
using FCGPagamentos.Application.Repository;
using FCGPagamentos.Domain.Entites;
using FCGPagamentos.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace FCGPagamentos.Infrastructure;

public static class InfrastructureBootstrapper
{
  public static void Register(this IServiceCollection services)
  {
    services.AddTransient<IPaymentRepository, PaymentRepository>();
    services.AddTransient<IEventRepository, EventRepository>();
  }
}
