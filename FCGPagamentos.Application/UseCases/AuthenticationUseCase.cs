using FCGPagamentos.Application.Repository;

namespace FCGPagamentos.Application.UseCases;

internal class AuthenticationUseCase : IAuthenticationUseCase
{
  private readonly IAuthenticationRepository _authenticationRepository;
  public AuthenticationUseCase(IAuthenticationRepository authenticationRepository)
  {
    _authenticationRepository = authenticationRepository;
  }
  public bool Authenticate(string username, string password)
  {
    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
    {
      throw new ArgumentException("Username and password cannot be empty.");
    }
    return _authenticationRepository.Authenticate(username, password);
  }
}
