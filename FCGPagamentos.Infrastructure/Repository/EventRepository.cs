using FCGPagamentos.Application.Repository;
using FCGPagamentos.Domain.Context;
using FCGPagamentos.Domain.Entites;
using Microsoft.EntityFrameworkCore;

namespace FCGPagamentos.Infrastructure.Repository;
public class EventRepository : IEventRepository
{
  private readonly ApplicationContext _context;
  public EventRepository(ApplicationContext context)
  {
    _context = context;
  }
  public async Task<Event?> GetByIdAsync(Guid Id) => await _context.Events.FirstOrDefaultAsync(x => x.Id == Id);
  public async Task<Event> CreateAsync(Event payment)
  {
    _context.Events.Add(payment);
    await _context.SaveChangesAsync();
    return payment;
  }
}