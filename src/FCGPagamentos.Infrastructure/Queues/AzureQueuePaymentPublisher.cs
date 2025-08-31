using Azure.Storage.Queues;
using FCGPagamentos.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace FCGPagamentos.Infrastructure.Queues;

public class AzureQueuePaymentPublisher : IPaymentProcessingPublisher
{
    private readonly QueueClient _client;

    public AzureQueuePaymentPublisher(IConfiguration cfg)
    {
        _client = new QueueClient(
            cfg["AzureStorage:ConnectionString"],
            "payments-requests",
            new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });

        _client.CreateIfNotExists();
    }

    public Task PublishRequestedAsync(Guid paymentId, Guid userId, Guid gameId, decimal amount, string currency, CancellationToken ct)
    {
        var payload = new { PaymentId = paymentId, UserId = userId, GameId = gameId, Amount = amount, Currency = currency };
        var json = JsonSerializer.Serialize(payload);
        return _client.SendMessageAsync(json, cancellationToken: ct);
    }
}
