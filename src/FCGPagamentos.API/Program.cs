using FCGPagamentos.API.DI;
using FCGPagamentos.API.Endpoints;

var b = WebApplication.CreateBuilder(args);
b.Services.AddAppServices(b.Configuration);

var app = b.Build();
app.MapPaymentEndpoints();
app.Run();
