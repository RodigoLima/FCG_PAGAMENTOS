using FCGPagamentos.Application;
using FCGPagamentos.Domain.Context;
using FCGPagamentos.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Healthchecks
builder.Services.AddHealthChecks()
    .AddCheck("Example Health Check", () => 
        HealthCheckResult.Healthy("The service is healthy!"), 
        tags: new[] { "example" });
#endregion
#region Logging
#endregion
#region CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});
#endregion
#region Dependency Injection
ApplicationBootstrapper.Register(builder.Services);
InfrastructureBootstrapper.Register(builder.Services);
#endregion
#region Entity Framework
builder.Services.AddDbContext<ApplicationContext>();
#endregion
//

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
