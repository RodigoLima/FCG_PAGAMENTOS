using FCGPagamentos.Application.Repository;
using FCGPagamentos.Domain.Context;
using FCGPagamentos.Domain.Entites;
using Microsoft.EntityFrameworkCore;

namespace FCGPagamentos.Infrastructure.Repository;
public class PaymentRepository : IRepository<Payment>
{
  private readonly ApplicationContext _context;
  public PaymentRepository(ApplicationContext context)
  {
    _context = context;
  }
  public async Task<Payment?> GetByIdAsync(Guid Id) => await _context.Payments.FirstOrDefaultAsync(x => x.Id == Id);
  public async Task<Payment> CreateAsync(Payment payment)
  {
    _context.Payments.Add(payment);
    await _context.SaveChangesAsync();
    return payment;
  }
  public async Task<bool> UpdateAsync(Payment payment)
  {
    _context.Payments.Update(payment);
    return await _context.SaveChangesAsync() > 0;
  }
}