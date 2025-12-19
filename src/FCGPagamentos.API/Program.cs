using FCGPagamentos.API.DI;
using FCGPagamentos.API.Endpoints;
using FCGPagamentos.API.Middleware;
using FCGPagamentos.API.Services;
using FCGPagamentos.Infrastructure.Persistence;
using Grafana.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Configuração do protocolo W3C Trace Context (deve vir ANTES de qualquer instrumentação)
Activity.DefaultIdFormat = ActivityIdFormat.Hierarchical;
Activity.ForceDefaultIdFormat = true;

// Configuração do Serilog
builder.Host.AddSerilog();

// Configuração centralizada de observabilidade
builder.Services.AddObservability(builder.Configuration);

// Serviços da aplicação
builder.Services.AddAppServices(builder.Configuration);

// Adiciona Swagger
builder.Services.AddEndpointsApiExplorer();
var pathBase = Environment.GetEnvironmentVariable("PATH_BASE");
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FCG Pagamentos API",
        Version = "v1",
        Description = "API para processamento de pagamentos do sistema FCG"
    });
    
    if (!string.IsNullOrEmpty(pathBase))
    {
        c.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
        {
            Url = pathBase.TrimEnd('/')
        });
    }
});

var app = builder.Build();

// Log de inicialização
Console.WriteLine($"FCG Pagamentos API iniciando - Environment: {app.Environment.EnvironmentName}");

// Aplicar migrações do banco de dados
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Console.WriteLine("Aplicando migrações do banco de dados...");
        await context.Database.MigrateAsync();
        Console.WriteLine("Migrações aplicadas com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrações (ignorado): {ex.Message}");
    }
}

// Middleware de correlation ID (deve vir antes de outros middlewares)
app.UseCorrelationId();

app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    var swaggerPath = string.IsNullOrEmpty(pathBase) 
        ? "./v1/swagger.json" 
        : $"{pathBase.TrimEnd('/')}/swagger/v1/swagger.json";
    c.SwaggerEndpoint(swaggerPath, "FCG Pagamentos API v1");
    c.RoutePrefix = "swagger";
});
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .UseGrafana()
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .UseGrafana()
    .Build();

// Endpoint raiz
app.MapGet("/", () => Results.Ok(new { 
    Service = "FCG Pagamentos API", 
    Version = "1.0.0",
    Status = "Running"
}))
.WithName("Root")
.WithSummary("Informações básicas da API")
.ExcludeFromDescription();

// Endpoints da aplicação
app.MapPaymentEndpoints();
app.MapMetricsEndpoints();
app.MapInternal();

Console.WriteLine("API configurada. Iniciando servidor...");
app.Run();
