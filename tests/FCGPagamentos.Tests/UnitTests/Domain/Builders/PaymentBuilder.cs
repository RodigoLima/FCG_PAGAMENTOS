using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.Enums;
using FCGPagamentos.Domain.ValueObjects;

namespace FCGPagamentos.Tests.UnitTests.Domain.Builders;

public class PaymentBuilder
{
    private string _userId = Guid.NewGuid().ToString();
    private string _gameId = Guid.NewGuid().ToString();
    private string _correlationKey = Guid.NewGuid().ToString();
    private Money _value = new Money(100.00m, "BRL");
    private PaymentMethod _method = PaymentMethod.Pix;
    private DateTime _createdAt = new DateTime(2025, 9, 2, 10, 0, 0);

    public PaymentBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public PaymentBuilder WithGameId(string gameId)
    {
        _gameId = gameId;
        return this;
    }

    public PaymentBuilder WithCorrelationKey(string correlationKey)
    {
        _correlationKey = correlationKey;
        return this;
    }

    public PaymentBuilder WithValue(Money value)
    {
        _value = value;
        return this;
    }

    public PaymentBuilder WithMethod(PaymentMethod method)
    {
        _method = method;
        return this;
    }

    public PaymentBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public Payment Build()
    {
        return new Payment(_userId, _gameId, _correlationKey, _value, _method, _createdAt);
    }
}
