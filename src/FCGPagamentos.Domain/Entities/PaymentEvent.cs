using FCGPagamentos.Domain.Enums;
using FCGPagamentos.Domain.Models;
using System.Text.Json;

namespace FCGPagamentos.Domain.Entities;

public class PaymentEvent : BaseModel
{
    public long Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public EventType EventType { get; private set; }
    public JsonDocument EventPayload { get; private set; } = null!;
    public DateTime OccurredAt { get; private set; }

    private PaymentEvent() { } // EF

    public PaymentEvent(Guid paymentId, EventType eventType, JsonDocument eventPayload, DateTime occurredAt)
    {
        PaymentId = paymentId;
        EventType = eventType;
        EventPayload = eventPayload;
        OccurredAt = occurredAt;
        CreatedAt = occurredAt;
    }
}
