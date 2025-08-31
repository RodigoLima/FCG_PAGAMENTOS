using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FCGPagamentos.API.Services;

public class BusinessMetricsService
{
    private readonly Meter _meter;
    private readonly Counter<long> _paymentRequestsCounter;
    private readonly Counter<long> _paymentSuccessCounter;
    private readonly Counter<long> _paymentFailureCounter;
    private readonly Histogram<double> _paymentProcessingTime;
    private readonly Counter<long> _totalAmountProcessed;

    public BusinessMetricsService()
    {
        _meter = new Meter("FCGPagamentos", "1.0.0");
        
        // Contadores para diferentes tipos de eventos
        _paymentRequestsCounter = _meter.CreateCounter<long>("payment_requests_total", "Total de requisições de pagamento");
        _paymentSuccessCounter = _meter.CreateCounter<long>("payment_success_total", "Total de pagamentos com sucesso");
        _paymentFailureCounter = _meter.CreateCounter<long>("payment_failure_total", "Total de pagamentos com falha");
        
        // Histograma para tempo de processamento
        _paymentProcessingTime = _meter.CreateHistogram<double>("payment_processing_time_seconds", "Tempo de processamento dos pagamentos");
        
        // Contador para valor total processado
        _totalAmountProcessed = _meter.CreateCounter<long>("payment_amount_total", "Valor total processado em centavos");
    }

    public void RecordPaymentRequest()
    {
        _paymentRequestsCounter.Add(1);
    }

    public void RecordPaymentSuccess()
    {
        _paymentSuccessCounter.Add(1);
    }

    public void RecordPaymentFailure()
    {
        _paymentFailureCounter.Add(1);
    }

    public void RecordPaymentAmount(decimal amount)
    {
        // Converte para centavos para evitar problemas com decimal
        var amountInCents = (long)(amount * 100);
        _totalAmountProcessed.Add(amountInCents);
    }

    public IDisposable MeasurePaymentProcessingTime()
    {
        return _paymentProcessingTime.NewTimer();
    }

    public void RecordPaymentProcessingTime(double seconds)
    {
        _paymentProcessingTime.Record(seconds);
    }
}
