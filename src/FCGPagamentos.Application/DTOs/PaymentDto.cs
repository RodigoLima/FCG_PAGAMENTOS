using FCGPagamentos.Domain.Enums;
namespace FCGPagamentos.Application.DTOs;
public record PaymentDto(Guid Id, string UserId, string GameId, decimal Amount, string Currency, PaymentMethod Method, PaymentStatus Status, DateTime CreatedAt, DateTime? LastUpdateAt, DateTime? ProcessedAt);
