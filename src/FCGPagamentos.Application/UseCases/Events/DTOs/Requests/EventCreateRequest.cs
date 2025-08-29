namespace FCGPagamentos.Application.Payments.DTOs.Requests;
public class EventCreateRequest
{
  public long Id { get; set; }
  public string Type { get; set; } = "";
  public string Payload { get; set; } = "";
  public DateTime OcurredAt { get; set; } = new DateTime();
}
