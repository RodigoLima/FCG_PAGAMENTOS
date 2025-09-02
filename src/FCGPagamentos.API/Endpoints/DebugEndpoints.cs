using FCGPagamentos.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FCGPagamentos.API.Endpoints;

public static class DebugEndpoints
{
    public static IEndpointRouteBuilder MapDebugEndpoints(this IEndpointRouteBuilder app)
    {
        var debugGroup = app.MapGroup("/debug")
            .WithTags("Debug")
            .WithSummary("Endpoints de debug para observabilidade");

        // Endpoint para testar observabilidade
        debugGroup.MapGet("/observability", (
            IObservabilityDebugService debugService,
            IObservabilityConfigurationService configService,
            ITelemetryService telemetryService,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("🔍 Debug endpoint chamado - testando observabilidade");

            // Testa Application Insights
            debugService.TestApplicationInsights();
            
            // Testa OpenTelemetry
            debugService.TestOpenTelemetry();
            
            // Testa TelemetryService
            telemetryService.TrackCustomEvent("DebugTest", new Dictionary<string, string>
            {
                ["TestType"] = "DebugEndpoint",
                ["Timestamp"] = DateTime.UtcNow.ToString("O")
            });

            // Retorna status
            return Results.Ok(new
            {
                Status = "Debug executado com sucesso",
                Timestamp = DateTime.UtcNow,
                ApplicationInsightsConfigured = configService.IsApplicationInsightsConfigured(),
                ConsoleExporterEnabled = configService.IsConsoleExporterEnabled(),
                SamplingRatio = configService.GetSamplingRatio(),
                Message = "Verifique os logs para detalhes dos testes"
            });
        })
        .WithName("DebugObservability")
        .WithSummary("Testa toda a configuração de observabilidade")
        .WithDescription("Executa testes de Application Insights, OpenTelemetry e TelemetryService");

        // Endpoint para informações de configuração
        debugGroup.MapGet("/config", (
            IObservabilityConfigurationService configService,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("🔍 Debug config endpoint chamado");

            return Results.Ok(new
            {
                ApplicationInsightsConfigured = configService.IsApplicationInsightsConfigured(),
                ConsoleExporterEnabled = configService.IsConsoleExporterEnabled(),
                SamplingRatio = configService.GetSamplingRatio(),
                ConnectionStringConfigured = !string.IsNullOrEmpty(configService.GetApplicationInsightsConnectionString()),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                MachineName = Environment.MachineName,
                Timestamp = DateTime.UtcNow
            });
        })
        .WithName("DebugConfig")
        .WithSummary("Retorna informações de configuração de observabilidade")
        .WithDescription("Mostra o status atual da configuração sem executar testes");

        // Endpoint para informações do sistema
        debugGroup.MapGet("/system", (ILogger<Program> logger) =>
        {
            logger.LogInformation("🔍 Debug system endpoint chamado");

            var process = System.Diagnostics.Process.GetCurrentProcess();
            
            return Results.Ok(new
            {
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                DotNetVersion = Environment.Version.ToString(),
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                WorkingSetMB = process.WorkingSet64 / 1024 / 1024,
                ThreadCount = process.Threads.Count,
                EnvironmentVariables = new
                {
                    ASPNETCORE_ENVIRONMENT = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    WEBSITE_SITE_NAME = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"),
                    WEBSITE_INSTANCE_ID = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"),
                    HasApplicationInsightsConnectionString = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")),
                    HasAppInsightsInstrumentationKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY"))
                },
                Timestamp = DateTime.UtcNow
            });
        })
        .WithName("DebugSystem")
        .WithSummary("Retorna informações do sistema")
        .WithDescription("Mostra informações detalhadas do sistema e variáveis de ambiente");

        // Endpoint para testar transações (aparece na Pesquisa de Transação)
        debugGroup.MapPost("/test-transaction", async (
            ITelemetryService telemetryService,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("🔍 Debug transaction endpoint chamado - criando transação de teste");

            var paymentId = Guid.NewGuid();
            var correlationId = Guid.NewGuid().ToString();
            var amount = 100.50m;
            var startTime = DateTime.UtcNow;

            // Simula processamento
            await Task.Delay(100);

            var duration = DateTime.UtcNow - startTime;

            // Rastreia como transação
            telemetryService.TrackPaymentRequest(paymentId, amount, correlationId);
            telemetryService.TrackPaymentSuccess(paymentId, amount, correlationId, duration);

            return Results.Ok(new
            {
                Status = "Transação de teste criada com sucesso",
                PaymentId = paymentId,
                Amount = amount,
                CorrelationId = correlationId,
                Duration = duration.TotalMilliseconds,
                Message = "Esta transação deve aparecer na Pesquisa de Transação do Application Insights",
                Timestamp = DateTime.UtcNow
            });
        })
        .WithName("DebugTestTransaction")
        .WithSummary("Cria uma transação de teste para verificar na Pesquisa de Transação")
        .WithDescription("Cria uma transação completa com Request e Dependency telemetry para aparecer na Pesquisa de Transação");

        return app;
    }
}
