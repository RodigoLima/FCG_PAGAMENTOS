using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FCGPagamentos.API.Services;

public interface IObservabilityConfigurationService
{
    string? GetApplicationInsightsConnectionString();
    bool IsApplicationInsightsConfigured();
    bool IsConsoleExporterEnabled();
    double GetSamplingRatio();
    void LogConfigurationStatus(ILogger logger);
}

public class ObservabilityConfigurationService : IObservabilityConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly string? _connectionString;

    public ObservabilityConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = GetApplicationInsightsConnectionString();
    }

    public string? GetApplicationInsightsConnectionString()
    {
        return _configuration["ApplicationInsights:ConnectionString"] 
            ?? _configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
            ?? _configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
    }

    public bool IsApplicationInsightsConfigured()
    {
        return !string.IsNullOrEmpty(_connectionString);
    }

    public bool IsConsoleExporterEnabled()
    {
        return _configuration.GetValue<bool>("OpenTelemetry:EnableConsoleExporter", false);
    }

    public double GetSamplingRatio()
    {
        return _configuration.GetValue<double>("OpenTelemetry:Tracing:SamplingRatio", 1.0);
    }

    public void LogConfigurationStatus(ILogger logger)
    {
        logger.LogInformation("🔍 === OBSERVABILIDADE CONFIGURATION STATUS ===");
        
        // Application Insights Status
        if (IsApplicationInsightsConfigured())
        {
            logger.LogInformation("✅ Application Insights: CONFIGURADO");
            logger.LogInformation("   Connection String: {ConnectionString}", 
                MaskConnectionString(_connectionString!));
        }
        else
        {
            logger.LogWarning("⚠️ Application Insights: NÃO CONFIGURADO");
            logger.LogWarning("   Variáveis verificadas:");
            logger.LogWarning("   - ApplicationInsights:ConnectionString: {Value1}", 
                string.IsNullOrEmpty(_configuration["ApplicationInsights:ConnectionString"]) ? "VAZIO" : "CONFIGURADO");
            logger.LogWarning("   - APPLICATIONINSIGHTS_CONNECTION_STRING: {Value2}", 
                string.IsNullOrEmpty(_configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]) ? "VAZIO" : "CONFIGURADO");
            logger.LogWarning("   - APPINSIGHTS_INSTRUMENTATIONKEY: {Value3}", 
                string.IsNullOrEmpty(_configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]) ? "VAZIO" : "CONFIGURADO");
        }

        // OpenTelemetry Status
        logger.LogInformation("🔧 OpenTelemetry Configuration:");
        logger.LogInformation("   Console Exporter: {ConsoleExporter}", IsConsoleExporterEnabled() ? "HABILITADO" : "DESABILITADO");
        logger.LogInformation("   Sampling Ratio: {SamplingRatio}", GetSamplingRatio());

        // Environment Info
        var environment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT", "Unknown");
        logger.LogInformation("🌍 Environment: {Environment}", environment);
        logger.LogInformation("🖥️ Machine: {MachineName}", Environment.MachineName);
        
        logger.LogInformation("🔍 === END OBSERVABILIDADE STATUS ===");
    }

    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "VAZIO";

        // Mascara a connection string para logs de segurança
        if (connectionString.Contains("InstrumentationKey="))
        {
            var parts = connectionString.Split(';');
            var maskedParts = parts.Select(part =>
            {
                if (part.StartsWith("InstrumentationKey="))
                {
                    var key = part.Substring("InstrumentationKey=".Length);
                    return $"InstrumentationKey={key.Substring(0, Math.Min(8, key.Length))}...";
                }
                return part;
            });
            return string.Join(";", maskedParts);
        }

        return connectionString.Length > 20 ? connectionString.Substring(0, 20) + "..." : connectionString;
    }
}
