using Azure.Storage.Queues;
using FCGPagamentos.Application.Abstractions;
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
            "payments-requests",
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });

        _client.CreateIfNotExists();
        _logger = logger;
    }

    public async Task PublishRequestedAsync(Guid paymentId, Guid userId, Guid gameId, decimal amount, string currency, CancellationToken ct)
    {
        try
        {
            var payload = new { PaymentId = paymentId, UserId = userId, GameId = gameId, Amount = amount, Currency = currency };
            var json = JsonSerializer.Serialize(payload);
            await _client.SendMessageAsync(json, cancellationToken: ct);
            
            _logger.LogInformation("Payment message published to queue successfully. PaymentId: {PaymentId}", paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish payment message to queue. PaymentId: {PaymentId}", paymentId);
            throw;
        }
    }
}
