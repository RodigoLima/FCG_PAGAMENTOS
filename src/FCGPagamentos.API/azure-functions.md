# 🔧 Configuração do Azure Functions

## 📁 Arquivos de Configuração

### 1. `host.json`
Configurações globais do host do Azure Functions:
- **Versioning**: Versão 2.0
- **Logging**: Application Insights configurado
- **Extension Bundle**: Extensões automáticas

### 2. `local.settings.json`
Configurações para desenvolvimento local:
- **Storage**: Azure Storage Emulator
- **Database**: PostgreSQL local
- **Application Insights**: Chave de desenvolvimento

## 🚀 Execução Local

### 1. Instalar Azure Functions Core Tools
```bash
npm install -g azure-functions-core-tools@4 --unsafe-perm true
```

### 2. Configurar variáveis locais
```bash
# Copie o arquivo de exemplo
cp local.settings.json.example local.settings.json

# Edite com suas configurações locais
```

### 3. Executar localmente
```bash
func start
```

## 🔧 Configurações de Produção

### Variáveis de Ambiente Obrigatórias
```bash
# Azure Functions
AzureWebJobsStorage=<connection_string>
FUNCTIONS_WORKER_RUNTIME=dotnet-isolated

# Database
ConnectionStrings__DefaultConnection=<postgres_connection>

# Azure Storage
AzureStorage__ConnectionString=<storage_connection>

# Application Insights
ApplicationInsights__InstrumentationKey=<instrumentation_key>
```

### Configurações de Segurança
- **CORS**: Configurado para produção
- **Authentication**: Azure AD integrado
- **HTTPS**: Forçado em produção

## 📊 Monitoramento

### Application Insights
- **Traces**: Logs estruturados
- **Metrics**: Métricas de performance
- **Dependencies**: Rastreamento de dependências

### Logs Estruturados
```csharp
_logger.LogInformation("Payment processed", new { 
    PaymentId = payment.Id, 
    Amount = payment.Amount,
    Status = payment.Status 
});
```

## 🔄 Deploy

### Automático via GitHub Actions
- **Push para `main`**: Deploy para produção
- **Push para `develop`**: Deploy para staging

### Manual via Azure CLI
```bash
func azure functionapp publish fcg-pagamentos-api
```

## 🧪 Testes

### Smoke Tests
```bash
# Health Check
curl -f https://fcg-pagamentos-api.azurewebsites.net/health

# API Status
curl -f https://fcg-pagamentos-api.azurewebsites.net/api/status
```

### Load Testing
```bash
# Usando Artillery
artillery quick --count 100 --num 10 https://fcg-pagamentos-api.azurewebsites.net/health
```

## 📈 Escalabilidade

### Configuration
- **Plan**: Consumption (serverless)
- **Max Instances**: 10
- **Memory**: 1.5 GB por instância

### Performance
- **Cold Start**: < 2 segundos
- **Response Time**: < 500ms
- **Throughput**: 1000 req/s
