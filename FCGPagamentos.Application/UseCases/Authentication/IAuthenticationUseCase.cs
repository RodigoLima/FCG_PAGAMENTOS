namespace FCGPagamentos.Application.UseCases.Authentication;
internal interface IAuthenticationUseCase
{
  bool Authenticate(string username, string password);
}
