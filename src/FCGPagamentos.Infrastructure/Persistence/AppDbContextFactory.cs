using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCGPagamentos.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var cs = ConnectionStringProvider.Resolve(); // << mesma função
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(cs)
            .Options;
        return new AppDbContext(opts);
    }
}
