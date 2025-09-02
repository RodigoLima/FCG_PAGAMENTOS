using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Application.DTOs;

namespace FCGPagamentos.Application.UseCases.GetPayment;
public class GetPaymentHandler
{
    private readonly IPaymentRepository _repo;
    public GetPaymentHandler(IPaymentRepository repo) => _repo = repo;

    public async Task<PaymentDto?> Handle(GetPaymentQuery q, CancellationToken ct)
    {
        var p = await _repo.GetAsync(q.Id, ct);
        if (p is null) return null;
        return new PaymentDto(p.Id, p.OrderId, p.Value.Amount, p.Value.Currency, p.Method, p.Status, p.CreatedAt, p.UpdatedAt, p.ProcessedAt);
    }
}
