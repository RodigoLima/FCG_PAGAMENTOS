using Microsoft.EntityFrameworkCore;
using FCGPagamentos.Domain.Entities;

namespace FCGPagamentos.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

    // DbSets -> representam suas tabelas
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<EventStore> Events => Set<EventStore>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica mapeamentos via Fluent API
        modelBuilder.ApplyConfiguration(new Mappings.PaymentMap());
        modelBuilder.ApplyConfiguration(new Mappings.EventStoreMap());
        
        // Configuração para ignorar propriedades de Event Sourcing durante a migration
        modelBuilder.Entity<Payment>()
            .Ignore(p => p.UncommittedEvents)
            .Ignore(p => p.Version);
    }
}

public class EventStore
{
    public long Id { get; set; }
    public Guid EventId { get; set; }
    public string Type { get; set; } = "";
    public string Payload { get; set; } = "";
    public DateTime OccurredAt { get; set; }
    public long Version { get; set; }
    public string AggregateId { get; set; } = "";
}
