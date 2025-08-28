namespace FCGPagamentos.Application.Payments.DTOs.Responses;
public class EventResponse
{
  public long Id { get; set; }
  public string Type { get; set; }
  public string Payload { get; set; }
  public DateTime OcurredAt { get; set; }
}
