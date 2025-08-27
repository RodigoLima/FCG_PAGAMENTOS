using FCGPagamentos.Application.Repository;
using FCGPagamentos.Domain.Entites;
using FCGPagamentos.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace FCGPagamentos.Infrastructure;

public static class InfrastructureBootstrapper
{
  public static void Register(this IServiceCollection services)
  {
    services.AddTransient<IAuthenticationRepository, AuthenticationRepository>();
    // Adicione esta linha:
    services.AddTransient<IRepository<Payment>, PaymentRepository>();
  }
}
