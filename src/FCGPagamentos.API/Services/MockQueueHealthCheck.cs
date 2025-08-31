using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FCGPagamentos.API.Services;

public class MockQueueHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Em ambiente de desenvolvimento com mock, sempre retorna saudável
        return Task.FromResult(HealthCheckResult.Healthy("Mock queue is always healthy"));
    }
}
