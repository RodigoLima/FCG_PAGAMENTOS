using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;

namespace FCGPagamentos.API.Services;

public interface IPaymentObservabilityService
{
    void TrackPaymentRequest(Guid paymentId, decimal amount, string correlationId);
    void TrackPaymentSuccess(Guid paymentId, decimal amount, string correlationId, TimeSpan duration);
    void TrackPaymentFailure(Guid paymentId, decimal amount, string error, string correlationId);
    void TrackException(Exception exception, Dictionary<string, string>? properties = null);
}

public class PaymentObservabilityService : IPaymentObservabilityService
{
    private readonly ILogger<PaymentObservabilityService> _logger;
    private readonly TelemetryClient _telemetryClient;

    public PaymentObservabilityService(ILogger<PaymentObservabilityService> logger, TelemetryClient telemetryClient)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    public void TrackPaymentRequest(Guid paymentId, decimal amount, string correlationId)
    {
        // Log estruturado
        _logger.LogInformation(
            "Payment request received - PaymentId: {PaymentId}, Amount: {Amount}, CorrelationId: {CorrelationId}",
            paymentId, amount, correlationId);

        // Telemetria para Application Insights
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
        
        // Request Telemetry para aparecer na Pesquisa de Transação
        var requestTelemetry = new RequestTelemetry
        {
            Name = $"POST /payments/{paymentId}",
            Url = new Uri($"https://api-fcg-payments.azurewebsites.net/payments/{paymentId}"),
            ResponseCode = "202",
            Success = true,
            Duration = TimeSpan.FromMilliseconds(100)
        };
        requestTelemetry.Properties["PaymentId"] = paymentId.ToString();
        requestTelemetry.Properties["Amount"] = amount.ToString("F2");
        requestTelemetry.Properties["CorrelationId"] = correlationId;
        requestTelemetry.Properties["Operation"] = "CreatePayment";
        _telemetryClient.TrackRequest(requestTelemetry);
    }

    public void TrackPaymentSuccess(Guid paymentId, decimal amount, string correlationId, TimeSpan duration)
    {
        // Log estruturado
        _logger.LogInformation(
            "Payment processed successfully - PaymentId: {PaymentId}, Amount: {Amount}, CorrelationId: {CorrelationId}",
            paymentId, amount, correlationId);

        // Telemetria para Application Insights
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
        
        // Dependency Telemetry para aparecer na Pesquisa de Transação
        var dependencyTelemetry = new DependencyTelemetry
        {
            Name = "PaymentProcessing",
            Data = $"ProcessPayment_{paymentId}",
            Type = "Payment",
            Success = true,
            Duration = duration
        };
        dependencyTelemetry.Properties["PaymentId"] = paymentId.ToString();
        dependencyTelemetry.Properties["Amount"] = amount.ToString("F2");
        dependencyTelemetry.Properties["CorrelationId"] = correlationId;
        dependencyTelemetry.Properties["Operation"] = "ProcessPayment";
        _telemetryClient.TrackDependency(dependencyTelemetry);
    }

    public void TrackPaymentFailure(Guid paymentId, decimal amount, string error, string correlationId)
    {
        // Log estruturado
        _logger.LogError(
            "Payment processing failed - PaymentId: {PaymentId}, Amount: {Amount}, Error: {Error}, CorrelationId: {CorrelationId}",
            paymentId, amount, error, correlationId);

        // Telemetria para Application Insights
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
