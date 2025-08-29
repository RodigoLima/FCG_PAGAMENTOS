
namespace FCGPagamentos.Application.Domain.Payments.Messages;
public static class PaymentMessages
{
  public static class General
  {
    public const string NotFound = "Pagamento não encontrado.";
  }
  public static class UserId
  {
    public const string Required = "O usuário deve ser informado.";
  }
  public static class GameId
  {
    public const string Required = "O jogo deve ser informado.";
  }
  public static class Amount
  {
    public const string Required = "O preço do jogo deve ser informado.";
    public const string GreaterThanZero = "O preço do jogo deve ser maior que zero.";
  }
}
