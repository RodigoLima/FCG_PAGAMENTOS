using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using FCGPagamentos.Infrastructure.Persistence;
using FCGPagamentos.Infrastructure.Queues;
using Amazon.SQS;
using Amazon;

namespace FCGPagamentos.API.Services;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public DatabaseHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database is accessible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is not accessible", ex);
        }
    }
}

public class SqsHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public SqsHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var accessKey = _configuration["AWS:AccessKey"];
            var secretKey = _configuration["AWS:SecretKey"];
            var region = _configuration["AWS:Region"] ?? "us-east-1";
            var queueName = _configuration["AWS:SQS:QueueName"] ?? "payments-to-process";

            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            {
                return HealthCheckResult.Unhealthy("AWS credentials not configured");
            }

            var sqsClient = new AmazonSQSClient(accessKey, secretKey, RegionEndpoint.GetBySystemName(region));
            
            // Tenta obter a URL da fila pelo nome
            var accountId = _configuration["AWS:AccountId"];
            string queueUrl;
            
            if (!string.IsNullOrEmpty(accountId))
            {
                queueUrl = $"https://sqs.{region}.amazonaws.com/{accountId}/{queueName}";
            }
            else
            {
                // Se não tiver AccountId, tenta obter a URL da fila pelo nome
                var getQueueUrlResponse = await sqsClient.GetQueueUrlAsync(queueName, cancellationToken);
                queueUrl = getQueueUrlResponse.QueueUrl;
            }
            
            var response = await sqsClient.GetQueueAttributesAsync(queueUrl, new List<string> { "All" }, cancellationToken);
            
            if (response != null)
            {
                return HealthCheckResult.Healthy("AWS SQS queue is accessible");
            }

            return HealthCheckResult.Unhealthy("AWS SQS queue is not accessible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("AWS SQS is not accessible", ex);
        }
    }
}
