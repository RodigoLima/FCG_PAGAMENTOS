using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FCGPagamentos.API.Services;

public interface IObservabilityDebugService
{
    void LogDebugInfo();
    void TestApplicationInsights();
    void TestOpenTelemetry();
    void LogSystemInfo();
}

public class ObservabilityDebugService : IObservabilityDebugService
{
    private readonly ILogger<ObservabilityDebugService> _logger;
    private readonly TelemetryClient? _telemetryClient;
    private readonly IObservabilityConfigurationService _configService;

    public ObservabilityDebugService(
        ILogger<ObservabilityDebugService> logger,
        IObservabilityConfigurationService configService,
        TelemetryClient? telemetryClient = null)
    {
        _logger = logger;
        _configService = configService;
        _telemetryClient = telemetryClient;
    }

    public void LogDebugInfo()
    {
        _logger.LogInformation("🔍 === OBSERVABILIDADE DEBUG INFO ===");
        
        // Configuração
        _configService.LogConfigurationStatus(_logger);
        
        // System Info
        LogSystemInfo();
        
        // Testes
        TestApplicationInsights();
        TestOpenTelemetry();
        
        _logger.LogInformation("🔍 === END OBSERVABILIDADE DEBUG ===");
    }

    public void TestApplicationInsights()
    {
        _logger.LogInformation("🧪 === TESTING APPLICATION INSIGHTS ===");
        
        if (_telemetryClient == null)
        {
            _logger.LogWarning("❌ TelemetryClient: NÃO DISPONÍVEL");
            return;
        }

        try
        {
            // Teste de Evento
            _telemetryClient.TrackEvent("ObservabilityDebugTest", new Dictionary<string, string>
            {
                ["TestType"] = "Debug",
                ["Timestamp"] = DateTime.UtcNow.ToString("O"),
                ["Machine"] = Environment.MachineName
            });

            // Teste de Métrica
            _telemetryClient.TrackMetric("ObservabilityDebugMetric", 1.0, new Dictionary<string, string>
            {
                ["TestType"] = "Debug"
            });

            // Teste de Trace
            _telemetryClient.TrackTrace("Observability Debug Test - Trace", SeverityLevel.Information, new Dictionary<string, string>
            {
                ["TestType"] = "Debug"
            });

            _logger.LogInformation("✅ Application Insights: Testes enviados com sucesso");
            _logger.LogInformation("   - Evento: ObservabilityDebugTest");
            _logger.LogInformation("   - Métrica: ObservabilityDebugMetric");
            _logger.LogInformation("   - Trace: Observability Debug Test");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Application Insights: Erro ao enviar testes");
        }
        
        _logger.LogInformation("🧪 === END APPLICATION INSIGHTS TEST ===");
    }

    public void TestOpenTelemetry()
    {
        _logger.LogInformation("🧪 === TESTING OPENTELEMETRY ===");
        
        try
        {
            // Teste de Activity
            using var activity = ActivitySource.StartActivity("ObservabilityDebugTest");
            if (activity != null)
            {
                activity.SetTag("test.type", "debug");
                activity.SetTag("test.timestamp", DateTime.UtcNow.ToString("O"));
                activity.SetTag("test.machine", Environment.MachineName);
                
                _logger.LogInformation("✅ OpenTelemetry: Activity criada com sucesso");
                _logger.LogInformation("   - Activity Name: {ActivityName}", activity.DisplayName);
                _logger.LogInformation("   - Activity ID: {ActivityId}", activity.Id);
                _logger.LogInformation("   - Trace ID: {TraceId}", activity.TraceId);
            }
            else
            {
                _logger.LogWarning("⚠️ OpenTelemetry: Activity não foi criada (ActivitySource não configurado)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ OpenTelemetry: Erro ao criar Activity");
        }
        
        _logger.LogInformation("🧪 === END OPENTELEMETRY TEST ===");
    }

    public void LogSystemInfo()
    {
        _logger.LogInformation("🖥️ === SYSTEM INFO ===");
        
        try
        {
            var process = Process.GetCurrentProcess();
            
            _logger.LogInformation("   Machine: {MachineName}", Environment.MachineName);
            _logger.LogInformation("   OS: {OSVersion}", Environment.OSVersion);
            _logger.LogInformation("   .NET Version: {DotNetVersion}", Environment.Version);
            _logger.LogInformation("   Process ID: {ProcessId}", process.Id);
            _logger.LogInformation("   Process Name: {ProcessName}", process.ProcessName);
            _logger.LogInformation("   Working Set: {WorkingSet} MB", process.WorkingSet64 / 1024 / 1024);
            _logger.LogInformation("   Thread Count: {ThreadCount}", process.Threads.Count);
            
            // Environment Variables relacionadas à observabilidade
            _logger.LogInformation("   Environment Variables:");
            var envVars = new[]
            {
                "ASPNETCORE_ENVIRONMENT",
                "APPLICATIONINSIGHTS_CONNECTION_STRING",
                "APPINSIGHTS_INSTRUMENTATIONKEY",
                "WEBSITE_SITE_NAME",
                "WEBSITE_INSTANCE_ID"
            };
            
            foreach (var envVar in envVars)
            {
                var value = Environment.GetEnvironmentVariable(envVar);
                if (!string.IsNullOrEmpty(value))
                {
                    var maskedValue = envVar.Contains("CONNECTION") || envVar.Contains("KEY") 
                        ? MaskSensitiveValue(value) 
                        : value;
                    _logger.LogInformation("     {EnvVar}: {Value}", envVar, maskedValue);
                }
                else
                {
                    _logger.LogInformation("     {EnvVar}: NÃO DEFINIDA", envVar);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao obter informações do sistema");
        }
        
        _logger.LogInformation("🖥️ === END SYSTEM INFO ===");
    }

    private static string MaskSensitiveValue(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 10)
            return "***";
        
        return value.Substring(0, 8) + "..." + value.Substring(value.Length - 4);
    }

    private static readonly ActivitySource ActivitySource = new("FCGPagamentos.API.Debug", "1.0.0");
}
