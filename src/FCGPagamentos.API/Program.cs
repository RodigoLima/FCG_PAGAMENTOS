using FCGPagamentos.API.DI;
using FCGPagamentos.API.Endpoints;
using FCGPagamentos.API.Middleware;
using System.Diagnostics;

var b = WebApplication.CreateBuilder(args);

// Configuração do Serilog
b.Host.AddSerilog();

// Configuração para habilitar o protocolo W3C Trace Context (traceparent)
// Define o formato de ID hierárquico para suporte ao W3C Trace Context
// IMPORTANTE: Deve ser configurado ANTES de qualquer instrumentação
Activity.DefaultIdFormat = ActivityIdFormat.Hierarchical;
Activity.ForceDefaultIdFormat = true;

// Configuração do Application Insights (deve vir antes de outros serviços)
var appInsightsConnectionString = b.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    b.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
        options.EnableAdaptiveSampling = true;
        options.EnableQuickPulseMetricStream = true;
    });
}
else
{
    // Fallback: habilitar AI mesmo sem connection string explícita
    b.Services.AddApplicationInsightsTelemetry();
}
// Serviços da aplicação
b.Services.AddAppServices(b.Configuration);

// Configuração da observabilidade
b.Services.AddObservability(b.Configuration);

// Adiciona Swagger
b.Services.AddEndpointsApiExplorer();
b.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FCG Pagamentos API",
        Version = "v1",
        Description = "API para processamento de pagamentos do sistema FCG"
    });
});

var app = b.Build();

// Log de inicialização
Console.WriteLine("Aplicação iniciando...");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    Console.WriteLine("Application Insights configurado");
}

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
Console.WriteLine("Aplicação configurada. Iniciando servidor...");
app.Run();
