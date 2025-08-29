
using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Entites;
public class Payment : BaseModel
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid UserId { get; set; }
  public Guid GameId { get; set; }
  public decimal Amount { get; set; }
  public string Status { get; set; } = "requested";
  public string MessageStatus { get; set; } = "";

  public void MarkProcessed() => Status = "processed";
  public void MarkRefused() => Status = "refused";
  public void MarkAccepted() => Status = "accepted";
}
