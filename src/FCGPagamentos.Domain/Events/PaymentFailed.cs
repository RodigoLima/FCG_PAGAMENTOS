using FCGPagamentos.Domain.Models;

namespace FCGPagamentos.Domain.Events;

public class PaymentFailed : Event
{
    public Guid PaymentId { get; }
    public DateTime FailedAt { get; }
    public string ErrorMessage { get; }
    public string ErrorCode { get; }

    public PaymentFailed(Guid paymentId, string errorMessage, string errorCode)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        AggregateId = paymentId.ToString();
        OccurredAt = DateTime.UtcNow;
        Version = 1;
    }
}
