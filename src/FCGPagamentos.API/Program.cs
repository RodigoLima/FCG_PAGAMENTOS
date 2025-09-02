using FCGPagamentos.API.DI;
using FCGPagamentos.API.Endpoints;
using FCGPagamentos.API.Middleware;
using FCGPagamentos.API.Services;
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

// Log de inicialização e debug de observabilidade
Console.WriteLine("🚀 Aplicação iniciando...");
Console.WriteLine($"🌍 Environment: {app.Environment.EnvironmentName}");

// Debug de observabilidade
var debugService = app.Services.GetRequiredService<IObservabilityDebugService>();
debugService.LogDebugInfo();

// Middleware de correlation ID (deve vir antes de outros middlewares)
app.UseCorrelationId();

// Ativa Swagger em Development e opcionalmente em outros ambientes

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG Pagamentos API v1");
    c.RoutePrefix = "swagger"; // Acessível em /swagger
});


// Redirecionar raiz para Swagger
app.MapGet("/", () => Results.Redirect("/swagger"))
    .WithName("RedirectToSwagger")
    .WithSummary("Redireciona para o Swagger UI")
    .ExcludeFromDescription();

// Seus endpoints customizados
app.MapPaymentEndpoints();
app.MapMetricsEndpoints();
app.MapInternal();
app.MapDebugEndpoints(); 
Console.WriteLine("Aplicação configurada. Iniciando servidor...");
app.Run();
