
namespace FCGPagamentos.Application.Domain.Event.Messages;
public static class EventMessages
{
  public static class General
  {
    public const string NotFound = "Evento não encontrado.";
  }
  public static class Type
  {
    public const string Required = "O tipo deve ser informado.";
  }
  public static class Payload
  {
    public const string Required = "O 'Payload' deve ser informado.";
  }
}