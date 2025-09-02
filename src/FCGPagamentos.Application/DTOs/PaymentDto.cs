using FCGPagamentos.Domain.Enums;
namespace FCGPagamentos.Application.DTOs;
public record PaymentDto(Guid Id, string OrderId, decimal Amount, string Currency, PaymentMethod Method, PaymentStatus Status, DateTime CreatedAt, DateTime? LastUpdateAt, DateTime? ProcessedAt);
