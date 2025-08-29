namespace FCGPagamentos.Domain.ValueObjects;

public class Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "BRL";

    private Money() { } // EF precisa disso

    public Money(decimal amount, string currency = "BRL")
    {
        Amount = amount;
        Currency = currency;
    }
}
