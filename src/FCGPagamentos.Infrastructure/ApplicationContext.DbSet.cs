
using FCGPagamentos.Domain.Entites;
using Microsoft.EntityFrameworkCore;

namespace FCGPagamentos.Domain.Context;
public partial class ApplicationContext : DbContext
{
  public DbSet<Payment> Payments { get; set; }
  public DbSet<Event> Events { get; set; }
}
