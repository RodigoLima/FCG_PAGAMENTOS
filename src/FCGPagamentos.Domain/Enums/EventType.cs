namespace FCGPagamentos.Domain.Enums;

public enum EventType
{
    PaymentCreated = 0,
    PaymentQueued = 1,
    PaymentProcessing = 2,
    PaymentApproved = 3,
    PaymentDeclined = 4,
    PaymentFailed = 5
}
