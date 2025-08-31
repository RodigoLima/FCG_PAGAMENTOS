using FCGPagamentos.Application.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FCGPagamentos.Infrastructure.Queues;

public class MockPaymentPublisher : IPaymentProcessingPublisher
{
    private readonly ILogger<MockPaymentPublisher> _logger;

    public MockPaymentPublisher(ILogger<MockPaymentPublisher> logger)
    {
        _logger = logger;
    }

    public async Task PublishRequestedAsync(Guid paymentId, Guid userId, Guid gameId, decimal amount, string currency, CancellationToken ct)
    {
        try
        {
            var payload = new { PaymentId = paymentId, UserId = userId, GameId = gameId, Amount = amount, Currency = currency };
            var json = JsonSerializer.Serialize(payload);
            
            _logger.LogInformation("MOCK: Payment message would be published to queue. PaymentId: {PaymentId}, Payload: {Payload}", 
                paymentId, json);
            
            // Simula uma pequena latência de rede
            await Task.Delay(50, ct);
            
            _logger.LogInformation("MOCK: Payment message published successfully. PaymentId: {PaymentId}", paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MOCK: Failed to publish payment message. PaymentId: {PaymentId}", paymentId);
            throw;
        }
    }
}
