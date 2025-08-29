namespace FCGPagamentos.Application.Payments.DTOs.Responses;
public class PaymentResponse
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public Guid GameId { get; set; }
  public decimal Amount { get; set; }
  public string Status { get; set; }
  public string MessageStatus { get; set; }
}
