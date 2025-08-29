using Microsoft.Extensions.Configuration;

namespace FCGPagamentos.Infrastructure.Persistence;

public static class ConnectionStringProvider
{
    private const string ConnName = "Postgres";
    private const string EnvVar = "FCG_PAYMENTS_CS";

    public static string Resolve(IConfiguration? runtimeConfig = null)
    {
        // 1) ENV sempre ganha (CI/CD e segredos)
        var env = Environment.GetEnvironmentVariable(EnvVar);
        if (!string.IsNullOrWhiteSpace(env))
            return env!;

        // 2) Runtime: usa a configuração já carregada pelo Program.cs
        if (runtimeConfig is not null)
        {
            var cs = runtimeConfig.GetConnectionString(ConnName);
            if (!string.IsNullOrWhiteSpace(cs)) return cs!;
        }

        // 3) Design-time (dotnet ef): carrega o appsettings da API manualmente
        var basePath = FindApiBasePath();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                          ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                          ?? "Development";

        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var fromJson = cfg.GetConnectionString(ConnName);
        if (string.IsNullOrWhiteSpace(fromJson))
            throw new InvalidOperationException($"Connection string '{ConnName}' não encontrada.");

        return fromJson!;
    }

    private static string FindApiBasePath()
    {
        // Tenta localizar a pasta da API subindo diretórios e checando:
        // 1) <dir>/FCGPagamentos.API/appsettings.json
        // 2) <dir>/src/FCGPagamentos.API/appsettings.json
        var current = AppContext.BaseDirectory;

        for (int i = 0; i < 10; i++)
        {
            // a) irmão direto
            var candidateA = Path.Combine(current, "FCGPagamentos.API");
            var fileA = Path.Combine(candidateA, "appsettings.json");
            if (Directory.Exists(candidateA) && File.Exists(fileA))
                return candidateA;

            // b) dentro de /src
            var candidateB = Path.Combine(current, "src", "FCGPagamentos.API");
            var fileB = Path.Combine(candidateB, "appsettings.json");
            if (Directory.Exists(candidateB) && File.Exists(fileB))
                return candidateB;

            var parent = Directory.GetParent(current)?.FullName;
            if (parent is null)
                break;

            current = parent;
        }

        throw new DirectoryNotFoundException(
            "Não foi possível localizar a pasta FCGPagamentos.API (nem em <dir>/FCGPagamentos.API nem em <dir>/src/FCGPagamentos.API).");
    }
}
