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

    public async Task PublishPaymentForProcessingAsync(Guid paymentId, CancellationToken ct)
    {
        try
        {
            var payload = new { payment_id = paymentId };
            var json = JsonSerializer.Serialize(payload);
            
            _logger.LogInformation("MOCK: Publishing payment - PaymentId: {PaymentId}, Payload: {Payload}", paymentId, json);
            await Task.Delay(50, ct); // Simula latência de rede
            _logger.LogInformation("MOCK: Payment published successfully - PaymentId: {PaymentId}", paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MOCK: Failed to publish payment - PaymentId: {PaymentId}", paymentId);
            throw;
        }
    }
}
