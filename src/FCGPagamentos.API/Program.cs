using FCGPagamentos.API.DI;
using FCGPagamentos.API.Endpoints;
using FCGPagamentos.API.Middleware;

var b = WebApplication.CreateBuilder(args);

// Configuração do Serilog
b.Host.AddSerilog();

// Serviços da aplicação
b.Services.AddAppServices(b.Configuration);

// Configuração da observabilidade
b.Services.AddObservability(b.Configuration);

// Adiciona Swagger
b.Services.AddEndpointsApiExplorer();
b.Services.AddSwaggerGen();

b.Services.AddApplicationInsightsTelemetry();

var app = b.Build();

// Middleware de correlation ID (deve vir antes de outros middlewares)
app.UseCorrelationId();

// Ativa Swagger só em Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seus endpoints customizados
app.MapPaymentEndpoints();
app.MapMetricsEndpoints();

app.Run();
