namespace FCGPagamentos.Application.Payments.DTOs.Requests;
public class PaymentCreateRequest
{
  public Guid UserId { get; set; }
  public Guid GameId { get; set; }
  public decimal Amount { get; set; }
}
