# Azure Functions Worker - Configuração

## 📋 Visão Geral

Este documento descreve como configurar o **Worker de Processamento de Pagamentos** em Azure Functions, que deve estar em um repositório separado.

## 🏗️ Arquitetura do Worker

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Azure Queue   │───▶│ Azure Functions  │───▶│   PostgreSQL    │
│ (payments-requests) │   │   (Worker)       │   │   (Database)    │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

## 🚀 Configuração do Azure Functions

### 1. Estrutura do Projeto

```bash
FCGPagamentos.Worker/
├── FCGPagamentos.Worker.csproj
├── Program.cs
├── Functions/
│   ├── ProcessPaymentFunction.cs
│   └── PaymentStatusUpdateFunction.cs
├── Services/
│   ├── IPaymentProcessor.cs
│   ├── PaymentProcessor.cs
│   └── PaymentStatusUpdater.cs
└── Models/
    ├── PaymentMessage.cs
    └── PaymentStatus.cs
```

### 2. FCGPagamentos.Worker.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <AzureFunctionsVersion>v4</AzureFunctionsVersion>
    <OutputType>Exe</OutputType>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.Azure.Functions.Worker" Version="1.20.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Storage" Version="6.2.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="1.16.4" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="8.0.0" />
  </ItemGroup>
</Project>
```

### 3. Program.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FCGPagamentos.Worker.Services;
using FCGPagamentos.Worker.Functions;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Configuração do banco de dados
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")));
        
        // Serviços
        services.AddScoped<IPaymentProcessor, PaymentProcessor>();
        services.AddScoped<IPaymentStatusUpdater, PaymentStatusUpdater>();
        
        // Logging
        services.AddLogging();
    })
    .Build();

host.Run();
```

### 4. ProcessPaymentFunction.cs

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FCGPagamentos.Worker.Services;
using FCGPagamentos.Worker.Models;

namespace FCGPagamentos.Worker.Functions;

public class ProcessPaymentFunction
{
    private readonly ILogger<ProcessPaymentFunction> _logger;
    private readonly IPaymentProcessor _paymentProcessor;

    public ProcessPaymentFunction(
        ILogger<ProcessPaymentFunction> logger,
        IPaymentProcessor paymentProcessor)
    {
        _logger = logger;
        _paymentProcessor = paymentProcessor;
    }

    [Function("ProcessPayment")]
    public async Task Run(
        [QueueTrigger("payments-requests", Connection = "AzureStorage")] PaymentMessage message)
    {
        _logger.LogInformation("Processing payment: {PaymentId}", message.PaymentId);
        
        try
        {
            await _paymentProcessor.ProcessAsync(message);
            _logger.LogInformation("Payment processed successfully: {PaymentId}", message.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment: {PaymentId}", message.PaymentId);
            throw; // Retry policy do Azure Functions
        }
    }
}
```

### 5. PaymentMessage.cs

```csharp
namespace FCGPagamentos.Worker.Models;

public class PaymentMessage
{
    public Guid PaymentId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public string PaymentMethod { get; set; }
}
```

### 6. IPaymentProcessor.cs

```csharp
using FCGPagamentos.Worker.Models;

namespace FCGPagamentos.Worker.Services;

public interface IPaymentProcessor
{
    Task ProcessAsync(PaymentMessage message);
}
```

### 7. PaymentProcessor.cs

```csharp
using FCGPagamentos.Worker.Models;
using FCGPagamentos.Worker.Services;
using Microsoft.Extensions.Logging;

namespace FCGPagamentos.Worker.Services;

public class PaymentProcessor : IPaymentProcessor
{
    private readonly ILogger<PaymentProcessor> _logger;
    private readonly IPaymentStatusUpdater _statusUpdater;

    public PaymentProcessor(
        ILogger<PaymentProcessor> logger,
        IPaymentStatusUpdater statusUpdater)
    {
        _logger = logger;
        _statusUpdater = statusUpdater;
    }

    public async Task ProcessAsync(PaymentMessage message)
    {
        _logger.LogInformation("Starting payment processing for: {PaymentId}", message.PaymentId);
        
        // 1. Atualizar status para "Processing"
        await _statusUpdater.UpdateStatusAsync(message.PaymentId, "Processing");
        
        // 2. Simular processamento (em produção, integrar com gateway de pagamento)
        await Task.Delay(2000); // Simula processamento
        
        // 3. Simular sucesso (90% das vezes)
        var random = new Random();
        if (random.Next(100) < 90)
        {
            await _statusUpdater.UpdateStatusAsync(message.PaymentId, "Completed");
            _logger.LogInformation("Payment completed successfully: {PaymentId}", message.PaymentId);
        }
        else
        {
            await _statusUpdater.UpdateStatusAsync(message.PaymentId, "Failed", "Simulated failure");
            _logger.LogWarning("Payment failed: {PaymentId}", message.PaymentId);
        }
    }
}
```

## 🔧 Configuração do Azure

### 1. Criar Function App

```bash
# Via Azure CLI
az functionapp create \
  --name fcg-pagamentos-worker \
  --resource-group seu-rg \
  --consumption-plan-location eastus \
  --runtime dotnet-isolated \
  --functions-version 4 \
  --storage-account sua-storage-account
```

### 2. Configurar Variáveis de Ambiente

```bash
az functionapp config appsettings set \
  --name fcg-pagamentos-worker \
  --resource-group seu-rg \
  --settings \
    "ConnectionStrings__DefaultConnection"="sua_connection_string" \
    "AzureStorage__ConnectionString"="sua_storage_connection" \
    "FUNCTIONS_WORKER_RUNTIME"="dotnet-isolated"
```

### 3. Configurar Queue Trigger

```bash
# A queue "payments-requests" deve existir no Azure Storage
# O Function App deve ter permissão para acessar a queue
```

## 📊 Monitoramento

### 1. Application Insights

```csharp
// Adicionar ao Program.cs
services.AddApplicationInsightsTelemetryWorkerService();
services.ConfigureFunctionsApplicationInsights();
```

### 2. Métricas Importantes

- **Queue Length**: Número de mensagens na fila
- **Processing Time**: Tempo de processamento por pagamento
- **Success Rate**: Taxa de sucesso dos pagamentos
- **Error Rate**: Taxa de erro dos pagamentos

### 3. Alertas

```bash
# Criar alerta para queue length > 100
az monitor metrics alert create \
  --name "HighQueueLength" \
  --resource-group seu-rg \
  --scopes /subscriptions/seu-subscription/resourceGroups/seu-rg/providers/Microsoft.Storage/storageAccounts/sua-storage-account \
  --condition "avg QueueLength > 100" \
  --description "Queue length is too high"
```

## 🔄 Retry Policy

### 1. Configuração Padrão

```csharp
// Azure Functions v4 tem retry policy padrão
// 5 tentativas com backoff exponencial
```

### 2. Configuração Customizada

```csharp
[Function("ProcessPayment")]
public async Task Run(
    [QueueTrigger("payments-requests", Connection = "AzureStorage")] PaymentMessage message,
    int dequeueCount)
{
    if (dequeueCount > 3)
    {
        _logger.LogError("Payment {PaymentId} failed after {DequeueCount} attempts", 
            message.PaymentId, dequeueCount);
        
        // Mover para dead letter queue ou notificar
        return;
    }
    
    // Processar pagamento
}
```

## 🚨 Dead Letter Queue

### 1. Configuração

```csharp
// Azure Functions automaticamente move mensagens falhadas para DLQ
// Após 5 tentativas sem sucesso
```

### 2. Monitoramento

```csharp
[Function("ProcessDeadLetter")]
public async Task ProcessDeadLetter(
    [QueueTrigger("payments-requests-poison", Connection = "AzureStorage")] PaymentMessage message)
{
    _logger.LogError("Processing dead letter message: {PaymentId}", message.PaymentId);
    
    // Notificar equipe de suporte
    // Registrar em sistema de tickets
    // Tentar reprocessamento manual se necessário
}
```

## 📝 Checklist de Deploy

- [ ] Function App criado no Azure
- [ ] Variáveis de ambiente configuradas
- [ ] Queue "payments-requests" criada
- [ ] Permissões configuradas
- [ ] Application Insights habilitado
- [ ] Código deployado via CI/CD
- [ ] Testes de integração executados
- [ ] Monitoramento configurado
- [ ] Alertas configurados

## 🔗 Integração com API

### 1. Fluxo de Comunicação

```
API → Azure Queue → Azure Functions → Database
  ↑                                           ↓
  └─────────── Status Updates ←──────────────┘
```

### 2. Endpoints de Status

```csharp
// Na API, adicionar endpoint para atualizar status
app.MapPut("/payments/{id}/status", async (Guid id, string status, string? errorMessage) =>
{
    // Atualizar status do pagamento
    // Registrar evento de mudança de status
    return Results.Ok();
});
```

## 📚 Recursos Adicionais

- [Azure Functions Documentation](https://docs.microsoft.com/en-us/azure/azure-functions/)
- [Queue Storage Triggers](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger)
- [Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview)
- [Application Insights for Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-monitoring)
