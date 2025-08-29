using FCGPagamentos.Domain.Enums;
namespace FCGPagamentos.Application.DTOs;
public record PaymentDto(Guid Id, Guid UserId, Guid GameId, decimal Amount, string Currency, PaymentStatus Status, DateTime CreatedAt, DateTime? ProcessedAt);
