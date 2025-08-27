
using FCGPagamentos.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FCGPagamentos.Domain.Context;
public partial class ApplicationContext : DbContext
{
  public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
      modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationContext).Assembly);
  }

  public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    var timestamp = DateTime.UtcNow;

    var entries = ChangeTracker.Entries<BaseModel>();

    foreach (var entry in entries)
    {
      if (entry.State == EntityState.Added)
      {
        entry.Entity.CreatedAt = timestamp;
      }

      if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
      {
        entry.Entity.UpdatedAt = timestamp;
      }
    }

    return base.SaveChangesAsync(cancellationToken);
  }
}
