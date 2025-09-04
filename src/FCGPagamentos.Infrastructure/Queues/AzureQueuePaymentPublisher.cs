using Azure.Storage.Queues;
using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;


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

    public async Task PublishPaymentForProcessingAsync(PaymentRequestedMessage message, CancellationToken ct)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            
            _logger.LogInformation("Publishing payment to queue - PaymentId: {PaymentId}, Amount: {Amount}, Currency: {Currency}", 
                message.PaymentId, message.Amount, message.Currency);
            await _client.SendMessageAsync(json, cancellationToken: ct);
            _logger.LogInformation("Payment published successfully - PaymentId: {PaymentId}", message.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish payment - PaymentId: {PaymentId}", message.PaymentId);
            throw;
        }
    }
}
