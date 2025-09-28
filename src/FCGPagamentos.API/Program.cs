using FCGPagamentos.API.DI;
using FCGPagamentos.API.Endpoints;
using FCGPagamentos.API.Middleware;
using FCGPagamentos.API.Services;
using Grafana.OpenTelemetry;
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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FCG Pagamentos API",
        Version = "v1",
        Description = "API para processamento de pagamentos do sistema FCG"
    });
});

var app = builder.Build();

// Log de inicialização
Console.WriteLine($"FCG Pagamentos API iniciando - Environment: {app.Environment.EnvironmentName}");

// Middleware de correlation ID (deve vir antes de outros middlewares)
app.UseCorrelationId();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG Pagamentos API v1");
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
