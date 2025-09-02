using Azure.Storage.Queues;
using FCGPagamentos.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Diagnostics;

namespace FCGPagamentos.Infrastructure.Queues;

public class AzureQueuePaymentPublisher : IPaymentProcessingPublisher
{
    private readonly QueueClient _client;
    private readonly ILogger<AzureQueuePaymentPublisher> _logger;

    public AzureQueuePaymentPublisher(IConfiguration cfg, ILogger<AzureQueuePaymentPublisher> logger)
    {
        _client = new QueueClient(
            cfg["AzureStorage:ConnectionString"],
            "payments-to-process",
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });

        _client.CreateIfNotExists();
        _logger = logger;
    }

    public async Task PublishPaymentForProcessingAsync(Guid paymentId, CancellationToken ct)
    {
        try
        {
            var payload = new { payment_id = paymentId };
            var json = JsonSerializer.Serialize(payload);
            
            _logger.LogInformation("Publishing payment to queue - PaymentId: {PaymentId}", paymentId);
            await _client.SendMessageAsync(json, cancellationToken: ct);
            _logger.LogInformation("Payment published successfully - PaymentId: {PaymentId}", paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish payment - PaymentId: {PaymentId}", paymentId);
            throw;
        }
    }
}
