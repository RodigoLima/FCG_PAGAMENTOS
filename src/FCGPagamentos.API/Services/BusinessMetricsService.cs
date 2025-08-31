using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;

namespace FCGPagamentos.API.Services;

public class BusinessMetricsService
{
    private readonly Counter<long> _paymentRequestCounter;
    private readonly Counter<long> _paymentSuccessCounter;
    private readonly Counter<long> _paymentFailureCounter;
    private readonly Histogram<double> _paymentAmountHistogram;
    private readonly Histogram<double> _paymentProcessingTimeHistogram;
    private readonly Counter<long> _queueMessagePublishedCounter;
    private readonly Counter<long> _queueMessageFailedCounter;

    public BusinessMetricsService(Meter meter)
    {
        _paymentRequestCounter = meter.CreateCounter<long>("fcg.payments.requested", "Number of payment requests");
        _paymentSuccessCounter = meter.CreateCounter<long>("fcg.payments.completed", "Number of successful payments");
        _paymentFailureCounter = meter.CreateCounter<long>("fcg.payments.failed", "Number of failed payments");
        _paymentAmountHistogram = meter.CreateHistogram<double>("fcg.payments.amount", "Payment amount distribution");
        _paymentProcessingTimeHistogram = meter.CreateHistogram<double>("fcg.payments.processing_time", "Payment processing time");
        _queueMessagePublishedCounter = meter.CreateCounter<long>("fcg.queue.messages.published", "Number of messages published to queue");
        _queueMessageFailedCounter = meter.CreateCounter<long>("fcg.queue.messages.failed", "Number of failed queue messages");
    }

    public void RecordPaymentRequest() => _paymentRequestCounter.Add(1);
    public void RecordPaymentSuccess() => _paymentSuccessCounter.Add(1);
    public void RecordPaymentFailure() => _paymentFailureCounter.Add(1);
    public void RecordPaymentAmount(decimal amount) => _paymentAmountHistogram.Record((double)amount);
    public void RecordPaymentProcessingTime(double seconds) => _paymentProcessingTimeHistogram.Record(seconds);
    public void RecordQueueMessagePublished() => _queueMessagePublishedCounter.Add(1);
    public void RecordQueueMessageFailed() => _queueMessageFailedCounter.Add(1);
}
