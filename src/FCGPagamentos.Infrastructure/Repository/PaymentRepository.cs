using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCGPagamentos.Infrastructure.Persistence.Repositories;
public class PaymentRepository(AppDbContext db) : IPaymentRepository
{
    public async Task AddAsync(Payment payment, CancellationToken ct) => await db.Payments.AddAsync(payment, ct);
    public Task<Payment?> GetAsync(Guid id, CancellationToken ct) => db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
