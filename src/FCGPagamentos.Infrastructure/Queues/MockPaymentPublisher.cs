using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.DTOs;
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

    public async Task PublishPaymentForProcessingAsync(PaymentRequestedMessage message, CancellationToken ct)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            
            _logger.LogInformation("MOCK: Publishing payment - PaymentId: {PaymentId}, Amount: {Amount}, Currency: {Currency}, Payload: {Payload}", 
                message.PaymentId, message.Amount, message.Currency, json);
            await Task.Delay(50, ct); // Simula latência de rede
            _logger.LogInformation("MOCK: Payment published successfully - PaymentId: {PaymentId}", message.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MOCK: Failed to publish payment - PaymentId: {PaymentId}", message.PaymentId);
            throw;
        }
    }
}
