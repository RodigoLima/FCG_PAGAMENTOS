using Microsoft.EntityFrameworkCore;
using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Infrastructure.Persistence.Interceptors;

namespace FCGPagamentos.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly UpdateTimestampInterceptor _updateTimestampInterceptor;

    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) 
    {
        _updateTimestampInterceptor = new UpdateTimestampInterceptor();
    }

    // DbSets -> representam suas tabelas
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentEvent> PaymentEvents => Set<PaymentEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica mapeamentos via Fluent API
        modelBuilder.ApplyConfiguration(new Mappings.PaymentMap());
        modelBuilder.ApplyConfiguration(new Mappings.PaymentEventMap());
        
        // Configuração para ignorar propriedades de Event Sourcing durante a migration
        modelBuilder.Entity<Payment>()
            .Ignore(p => p.UncommittedEvents)
            .Ignore(p => p.Version);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_updateTimestampInterceptor);
    }
}


