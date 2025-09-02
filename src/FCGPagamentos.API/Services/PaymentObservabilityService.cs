using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
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
        _logger.LogInformation("Payment request - PaymentId: {PaymentId}, Amount: {Amount}, CorrelationId: {CorrelationId}",
            paymentId, amount, correlationId);

        SetActivityTags("PaymentRequest", paymentId, correlationId, amount);
        _telemetryClient.TrackEvent("PaymentRequest", CreateProperties(paymentId, correlationId, amount));
    }

    public void TrackPaymentSuccess(Guid paymentId, decimal amount, string correlationId, TimeSpan duration)
    {
        _logger.LogInformation("Payment success - PaymentId: {PaymentId}, Amount: {Amount}, CorrelationId: {CorrelationId}, Duration: {Duration}ms",
            paymentId, amount, correlationId, duration.TotalMilliseconds);

        SetActivityTags("PaymentSuccess", paymentId, correlationId, amount, duration.TotalMilliseconds);
        _telemetryClient.TrackEvent("PaymentSuccess", CreateProperties(paymentId, correlationId, amount, duration.TotalMilliseconds));
    }

    public void TrackPaymentFailure(Guid paymentId, decimal amount, string error, string correlationId)
    {
        _logger.LogError("Payment failure - PaymentId: {PaymentId}, Amount: {Amount}, Error: {Error}, CorrelationId: {CorrelationId}",
            paymentId, amount, error, correlationId);

        SetActivityTags("PaymentFailure", paymentId, correlationId, amount, error: error);
        _telemetryClient.TrackEvent("PaymentFailure", CreateProperties(paymentId, correlationId, amount, error: error));
    }

    public void TrackException(Exception exception, Dictionary<string, string>? properties = null)
    {
        _logger.LogError(exception, "Exception tracked - Properties: {@Properties}", properties);
        
        using var activity = Activity.Current?.Source.StartActivity("Exception");
        activity?.SetTag("exception.type", exception.GetType().Name);
        activity?.SetTag("exception.message", exception.Message);
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                activity?.SetTag(prop.Key.ToLowerInvariant().Replace(".", "_"), prop.Value);
            }
        }

        _telemetryClient.TrackException(exception);
    }

    private void SetActivityTags(string operation, Guid paymentId, string correlationId, decimal amount, double? durationMs = null, string? error = null)
    {
        using var activity = Activity.Current?.Source.StartActivity(operation);
        activity?.SetTag("payment.id", paymentId.ToString());
        activity?.SetTag("correlation.id", correlationId);
        activity?.SetTag("payment.amount", amount.ToString("F2"));
        activity?.SetTag("service.name", "FCGPagamentos.API");
        
        if (durationMs.HasValue)
            activity?.SetTag("duration.ms", durationMs.Value.ToString());
        
        if (error != null)
        {
            activity?.SetTag("error.message", error);
            activity?.SetStatus(ActivityStatusCode.Error, error);
        }
    }

    private Dictionary<string, string> CreateProperties(Guid paymentId, string correlationId, decimal amount, double? durationMs = null, string? error = null)
    {
        var properties = new Dictionary<string, string>
        {
            ["PaymentId"] = paymentId.ToString(),
            ["Amount"] = amount.ToString("F2"),
            ["CorrelationId"] = correlationId
        };

        if (durationMs.HasValue)
            properties["DurationMs"] = durationMs.Value.ToString();
        
        if (error != null)
            properties["Error"] = error;

        return properties;
    }
}
