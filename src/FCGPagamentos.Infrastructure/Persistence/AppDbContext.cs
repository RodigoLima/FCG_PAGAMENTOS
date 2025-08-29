using Microsoft.EntityFrameworkCore;
using FCGPagamentos.Domain.Entities;

namespace FCGPagamentos.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

    // DbSets -> representam suas tabelas
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<EventLog> Events => Set<EventLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica mapeamentos via Fluent API
        modelBuilder.ApplyConfiguration(new Mappings.PaymentMap());
        modelBuilder.ApplyConfiguration(new Mappings.EventLogMap());
    }
}

public class EventLog
{
    public long Id { get; set; }
    public string Type { get; set; } = "";
    public string Payload { get; set; } = "";
    public DateTime OccurredAt { get; set; }
}
