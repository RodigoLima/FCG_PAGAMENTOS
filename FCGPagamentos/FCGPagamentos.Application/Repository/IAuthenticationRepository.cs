namespace FCGPagamentos.Application.Repository;
public interface IAuthenticationRepository
{
  bool Authenticate(string username, string password);
}