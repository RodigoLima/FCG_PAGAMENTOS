using FCGPagamentos.Domain.Enums;
using FCGPagamentos.Domain.Models;
using FCGPagamentos.Domain.ValueObjects;

namespace FCGPagamentos.Domain.Entities;
public class Payment: BaseModel
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public Money Value { get; set; } = default!;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Requested;
    public DateTime? ProcessedAt { get; private set; }

    private Payment() { } // EF
    public Payment(Guid userId, Guid gameId, Money value, DateTime now)
    {
        if (value.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        UserId = userId; GameId = gameId; Value = value; CreatedAt = now;
    }

    public void MarkProcessed(DateTime now) { Status = PaymentStatus.Processed; ProcessedAt = now; }
    public void MarkFailed(DateTime now) { Status = PaymentStatus.Failed; ProcessedAt = now; }
}
