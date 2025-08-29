using FCGPagamentos.Domain.Entities;
namespace FCGPagamentos.Application.Abstractions;
public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken ct);
    Task<Payment?> GetAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
