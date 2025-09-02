using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics;

namespace FCGPagamentos.API.Services;

public interface ITelemetryService
{
    void TrackPaymentRequest(Guid paymentId, decimal amount, string correlationId);
    void TrackPaymentSuccess(Guid paymentId, decimal amount, string correlationId, TimeSpan duration);
    void TrackPaymentFailure(Guid paymentId, decimal amount, string error, string correlationId);
    void TrackCustomEvent(string eventName, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null);
    void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success);
    void TrackException(Exception exception, Dictionary<string, string>? properties = null);
}

public class TelemetryService : ITelemetryService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(TelemetryClient telemetryClient, ILogger<TelemetryService> logger)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    public void TrackPaymentRequest(Guid paymentId, decimal amount, string correlationId)
    {
        var properties = new Dictionary<string, string>
        {
            ["PaymentId"] = paymentId.ToString(),
            ["Amount"] = amount.ToString("F2"),
            ["CorrelationId"] = correlationId,
            ["EventType"] = "PaymentRequest"
        };

        var metrics = new Dictionary<string, double>
        {
            ["Amount"] = (double)amount
        };

        _telemetryClient.TrackEvent("PaymentRequest", properties, metrics);
        
        _logger.LogInformation("Payment request tracked - PaymentId: {PaymentId}, Amount: {Amount}, CorrelationId: {CorrelationId}", 
            paymentId, amount, correlationId);
    }

    public void TrackPaymentSuccess(Guid paymentId, decimal amount, string correlationId, TimeSpan duration)
    {
        var properties = new Dictionary<string, string>
        {
            ["PaymentId"] = paymentId.ToString(),
            ["Amount"] = amount.ToString("F2"),
            ["CorrelationId"] = correlationId,
            ["EventType"] = "PaymentSuccess"
        };

        var metrics = new Dictionary<string, double>
        {
            ["Amount"] = (double)amount,
            ["ProcessingTimeMs"] = duration.TotalMilliseconds
        };

        _telemetryClient.TrackEvent("PaymentSuccess", properties, metrics);
        
        _logger.LogInformation("Payment success tracked - PaymentId: {PaymentId}, Amount: {Amount}, Duration: {Duration}ms, CorrelationId: {CorrelationId}", 
            paymentId, amount, duration.TotalMilliseconds, correlationId);
    }

    public void TrackPaymentFailure(Guid paymentId, decimal amount, string error, string correlationId)
    {
        var properties = new Dictionary<string, string>
        {
            ["PaymentId"] = paymentId.ToString(),
            ["Amount"] = amount.ToString("F2"),
            ["Error"] = error,
            ["CorrelationId"] = correlationId,
            ["EventType"] = "PaymentFailure"
        };

        var metrics = new Dictionary<string, double>
        {
            ["Amount"] = (double)amount
        };

        _telemetryClient.TrackEvent("PaymentFailure", properties, metrics);
        
        _logger.LogError("Payment failure tracked - PaymentId: {PaymentId}, Amount: {Amount}, Error: {Error}, CorrelationId: {CorrelationId}", 
            paymentId, amount, error, correlationId);
    }

    public void TrackCustomEvent(string eventName, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null)
    {
        _telemetryClient.TrackEvent(eventName, properties, metrics);
        
        _logger.LogDebug("Custom event tracked - EventName: {EventName}, Properties: {@Properties}, Metrics: {@Metrics}", 
            eventName, properties, metrics);
    }

    public void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success)
    {
        var dependencyTelemetry = new DependencyTelemetry
        {
            Name = dependencyName,
            Data = commandName,
            Timestamp = startTime,
            Duration = duration,
            Success = success,
            Type = "External"
        };

        _telemetryClient.TrackDependency(dependencyTelemetry);
        
        _logger.LogDebug("Dependency tracked - Name: {Name}, Command: {Command}, Duration: {Duration}ms, Success: {Success}", 
            dependencyName, commandName, duration.TotalMilliseconds, success);
    }

    public void TrackException(Exception exception, Dictionary<string, string>? properties = null)
    {
        var exceptionTelemetry = new ExceptionTelemetry(exception);
        
        if (properties != null)
        {
            foreach (var prop in properties)
            {
                exceptionTelemetry.Properties[prop.Key] = prop.Value;
            }
        }

        _telemetryClient.TrackException(exceptionTelemetry);
        
        _logger.LogError(exception, "Exception tracked - Properties: {@Properties}", properties);
    }
}
