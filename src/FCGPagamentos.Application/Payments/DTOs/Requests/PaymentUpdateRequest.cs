namespace FCGPagamentos.Application.Payments.DTOs.Requests;
public class PaymentUpdateRequest
{
  public decimal Amount { get; set; }
  public string Status { get; set; } = "requested";
  public string MessageStatus { get; set; } = "";
}
