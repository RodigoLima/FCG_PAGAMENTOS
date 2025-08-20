using FCGPagamentos.Application.Repository;

namespace FCGPagamentos.Infrastructure.Repository;
internal class AuthenticationRepository : IAuthenticationRepository
{
  // Esta camada iria acessar banco de dados e tals
  bool IAuthenticationRepository.Authenticate(string username, string password) => true;
}
