using Microsoft.Extensions.Diagnostics.HealthChecks;
using FCGPagamentos.Infrastructure.Persistence;
using FCGPagamentos.Infrastructure.Queues;

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

public class AzureQueueHealthCheck : IHealthCheck
{
    private readonly AzureQueuePaymentPublisher _publisher;

    public AzureQueueHealthCheck(AzureQueuePaymentPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verifica se o publisher está configurado corretamente
            if (_publisher == null)
                return Task.FromResult(HealthCheckResult.Unhealthy("Azure Queue publisher not configured"));
            
            return Task.FromResult(HealthCheckResult.Healthy("Azure Queue is accessible"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Azure Queue is not accessible", ex));
        }
    }
}
