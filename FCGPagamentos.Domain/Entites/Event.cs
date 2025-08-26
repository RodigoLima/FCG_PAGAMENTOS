
using FCGPagamentos.Domain.Models;
using System.Text.Json;

namespace FCGPagamentos.Domain.Entites;
public class Event : BaseModel
{
  public long Id { get; set; }
  public string Type { get; set; }
  public string Payload { get; set; }
  public DateTime OcurredAt { get; set; }
  public Event(string type, object payload)
  {
    Type = type;
    Payload = JsonSerializer.Serialize(payload);
  }
}
