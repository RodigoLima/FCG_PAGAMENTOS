using System.Text.Json;
using System.Text.Json.Serialization;
using FCGPagamentos.Domain.Enums;

namespace FCGPagamentos.API.Models;

public class InternalPaymentRequest
{
    public Guid PaymentId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    
    [JsonConverter(typeof(PaymentMethodConverter))]
    public PaymentMethod PaymentMethod { get; set; }
    
    public DateTime OccurredAt { get; set; }
    public string Version { get; set; } = "1.0";
}

public class PaymentMethodConverter : JsonConverter<PaymentMethod>
{
    public override PaymentMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetInt32();
            if (Enum.IsDefined(typeof(PaymentMethod), value))
            {
                return (PaymentMethod)value;
            }
            throw new JsonException($"Invalid PaymentMethod value: {value}");
        }
        
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (int.TryParse(stringValue, out var intValue) && Enum.IsDefined(typeof(PaymentMethod), intValue))
            {
                return (PaymentMethod)intValue;
            }
            
            if (Enum.TryParse<PaymentMethod>(stringValue, true, out var enumValue))
            {
                return enumValue;
            }
            
            throw new JsonException($"Invalid PaymentMethod value: {stringValue}");
        }
        
        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, PaymentMethod value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}
