using Microsoft.Extensions.Logging;

namespace FCGPagamentos.API.Services;

public interface IStructuredLoggingService
{
    void LogPaymentRequest(Guid paymentId, decimal amount, string correlationId);
    void LogPaymentSuccess(Guid paymentId, decimal amount, string correlationId);
    void LogPaymentFailure(Guid paymentId, decimal amount, string error, string correlationId);
    void LogPaymentProcessing(Guid paymentId, string step, string correlationId);
}

public class StructuredLoggingService : IStructuredLoggingService
{
    private readonly ILogger<StructuredLoggingService> _logger;

    public StructuredLoggingService(ILogger<StructuredLoggingService> logger)
    {
        _logger = logger;
    }

    public void LogPaymentRequest(Guid paymentId, decimal amount, string correlationId)
    {
        _logger.LogInformation(
            "Payment request received - PaymentId: {PaymentId}, Amount: {Amount}, CorrelationId: {CorrelationId}",
            paymentId, amount, correlationId);
    }

    public void LogPaymentSuccess(Guid paymentId, decimal amount, string correlationId)
    {
        _logger.LogInformation(
            "Payment processed successfully - PaymentId: {PaymentId}, Amount: {Amount}, CorrelationId: {CorrelationId}",
            paymentId, amount, correlationId);
    }

    public void LogPaymentFailure(Guid paymentId, decimal amount, string error, string correlationId)
    {
        _logger.LogError(
            "Payment processing failed - PaymentId: {PaymentId}, Amount: {Amount}, Error: {Error}, CorrelationId: {CorrelationId}",
            paymentId, amount, error, correlationId);
    }

    public void LogPaymentProcessing(Guid paymentId, string step, string correlationId)
    {
        _logger.LogDebug(
            "Payment processing step - PaymentId: {PaymentId}, Step: {Step}, CorrelationId: {CorrelationId}",
            paymentId, step, correlationId);
    }
}
