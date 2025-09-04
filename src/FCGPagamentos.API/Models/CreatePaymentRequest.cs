using FCGPagamentos.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FCGPagamentos.API.Models;

public class CreatePaymentRequest
{
    [Required(ErrorMessage = "UserId é obrigatório")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "GameId é obrigatório")]
    public string GameId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount deve ser maior que zero")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Currency é obrigatório")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency deve ter exatamente 3 caracteres")]
    public string Currency { get; set; } = string.Empty;

    [Required(ErrorMessage = "Method é obrigatório")]
    public PaymentMethod Method { get; set; }
}
