using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.DTOs;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FCGPagamentos.Infrastructure.Queues;

public class SqsPaymentPublisher : IPaymentProcessingPublisher
{
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly ILogger<SqsPaymentPublisher> _logger;
    private readonly string _queueUrl;

    public SqsPaymentPublisher(
        ISendEndpointProvider sendEndpointProvider, 
        IConfiguration configuration,
        ILogger<SqsPaymentPublisher> logger)
    {
        _sendEndpointProvider = sendEndpointProvider;
        _logger = logger;
        
        var region = configuration["AWS:Region"] ?? "us-east-1";
        var queueName = configuration["AWS:SQS:QueueName"] ?? "payments-to-process";
        
        // MassTransit usa o formato queue: para filas SQS
        _queueUrl = $"queue:{queueName}";
    }

    public async Task PublishPaymentForProcessingAsync(PaymentRequestedMessage message, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Publishing payment to SQS queue '{QueueUrl}' - PaymentId: {PaymentId}, Amount: {Amount}, Currency: {Currency}", 
                _queueUrl, message.PaymentId, message.Amount, message.Currency);
            
            // Envia diretamente para a fila SQS usando o endpoint
            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri(_queueUrl));
            await endpoint.Send(message, ct);
            
            _logger.LogInformation("Payment published successfully to SQS queue '{QueueUrl}' - PaymentId: {PaymentId}", 
                _queueUrl, message.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish payment to SQS queue '{QueueUrl}' - PaymentId: {PaymentId}", 
                _queueUrl, message.PaymentId);
            throw;
        }
    }
}

