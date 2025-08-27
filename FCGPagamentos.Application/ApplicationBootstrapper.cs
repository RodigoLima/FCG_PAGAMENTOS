using FCGPagamentos.Application.Repository;
using FCGPagamentos.Application.UseCases.Authentication;
using FCGPagamentos.Application.UseCases.Payments;
using Microsoft.Extensions.DependencyInjection;

namespace FCGPagamentos.Application;

public static class ApplicationBootstrapper
{
  public static void Register(this IServiceCollection services)
  {
    services.AddTransient<IAuthenticationUseCase, AuthenticationUseCase>();
    services.AddTransient<IPaymentsUseCase, PaymentsUseCase>();
  }
}
