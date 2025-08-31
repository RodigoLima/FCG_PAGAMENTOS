using FCGPagamentos.Application.Abstractions;
using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FCGPagamentos.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;
    private readonly IEventStore _eventStore;

    public PaymentRepository(AppDbContext db, IEventStore eventStore)
    {
        _db = db;
        _eventStore = eventStore;
    }

    public async Task AddAsync(Payment payment, CancellationToken ct)
    {
        // Salva o aggregate
        await _db.Payments.AddAsync(payment, ct);
        
        // Salva os eventos não confirmados
        foreach (var @event in payment.UncommittedEvents)
        {
            await _eventStore.AppendAsync(@event, @event.OccurredAt, ct);
        }
        
        // Marca eventos como confirmados
        payment.MarkEventsAsCommitted();
        
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Payment?> GetAsync(Guid id, CancellationToken ct)
    {
        // Primeiro tenta buscar do cache/banco
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);
        
        if (payment != null)
        {
            // Recarrega o estado a partir dos eventos
            var events = await _eventStore.GetEventsAsync(id.ToString(), ct);
            payment.LoadFromHistory(events);
        }
        
        return payment;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        // Salva mudanças no aggregate
        await _db.SaveChangesAsync(ct);
        
        // Salva eventos não confirmados de todos os aggregates
        var payments = _db.ChangeTracker.Entries<Payment>()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        foreach (var payment in payments)
        {
            foreach (var @event in payment.UncommittedEvents)
            {
                await _eventStore.AppendAsync(@event, @event.OccurredAt, ct);
            }
            payment.MarkEventsAsCommitted();
        }
        
        await _db.SaveChangesAsync(ct);
    }
}
