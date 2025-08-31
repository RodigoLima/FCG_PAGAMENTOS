using FCGPagamentos.API.DI;
using FCGPagamentos.API.Endpoints;

var b = WebApplication.CreateBuilder(args);

// Serviços da aplicação
b.Services.AddAppServices(b.Configuration);

// Adiciona Swagger
b.Services.AddEndpointsApiExplorer();
b.Services.AddSwaggerGen();

b.Services.AddApplicationInsightsTelemetry();

var app = b.Build();

// Ativa Swagger só em Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seus endpoints customizados
app.MapPaymentEndpoints();

app.Run();
