# 🚀 Guia de Deploy - FCG PAGAMENTOS

## 📋 Visão Geral

Este guia fornece instruções detalhadas para fazer deploy do microsserviço de pagamentos em diferentes ambientes, desde desenvolvimento local até produção no Azure.

## 🎯 Ambientes

- **Development**: Ambiente local para desenvolvimento
- **Staging**: Ambiente de homologação
- **Production**: Ambiente de produção

## 🛠️ Pré-requisitos

### Ferramentas Necessárias
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [PostgreSQL](https://www.postgresql.org/download/) (local)
- [Git](https://git-scm.com/downloads)

### Contas e Recursos Azure
- Azure Subscription ativa
- Resource Group para o projeto
- Azure Storage Account
- Azure Application Insights
- Azure App Service Plan
- Azure API Management (opcional)

## 🏠 Ambiente de Desenvolvimento Local

### 1. Configuração Inicial

```bash
# Clone o repositório
git clone https://github.com/seu-usuario/FCG_PAGAMENTOS.git
cd FCG_PAGAMENTOS

# Restaure as dependências
dotnet restore
```

### 2. Configuração do Banco de Dados

```bash
# Instale o PostgreSQL localmente ou use Docker
docker run --name postgres-fcg -e POSTGRES_PASSWORD=sua_senha -e POSTGRES_DB=fcg_pagamentos -p 5432:5432 -d postgres:15

# Ou use um PostgreSQL local instalado
```

### 3. Configuração das Variáveis de Ambiente

Crie o arquivo `src/FCGPagamentos.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=fcg_pagamentos;Username=postgres;Password=sua_senha;Port=5432"
  },
  "AzureStorage": {
    "ConnectionString": "UseDevelopmentStorage=true"
  },
  "ApplicationInsights": {
    "ConnectionString": "sua_connection_string_do_ai"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 4. Execução das Migrações

```bash
# Navegue para o projeto da API
cd src/FCGPagamentos.API

# Instale o EF Core CLI globalmente (se necessário)
dotnet tool install --global dotnet-ef

# Execute as migrações
dotnet ef database update

# Verifique se as tabelas foram criadas
dotnet ef dbcontext info
```

### 5. Execução da Aplicação

```bash
# Execute a aplicação
dotnet run

# A API estará disponível em:
# https://localhost:7001 (HTTPS)
# http://localhost:5000 (HTTP)
# Swagger: https://localhost:7001/swagger
```

### 6. Testes Locais

```bash
# Execute os testes
dotnet test

# Execute testes específicos
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

## 🧪 Ambiente de Staging

### 1. Criação dos Recursos Azure

```bash
# Login no Azure
az login

# Defina a subscription
az account set --subscription "sua-subscription-id"

# Crie um resource group
az group create --name "fcg-pagamentos-staging-rg" --location "Brazil South"

# Crie uma storage account
az storage account create \
  --name "fcgstagingstorage" \
  --resource-group "fcg-pagamentos-staging-rg" \
  --location "Brazil South" \
  --sku Standard_LRS

# Crie um app service plan
az appservice plan create \
  --name "fcg-staging-plan" \
  --resource-group "fcg-pagamentos-staging-rg" \
  --location "Brazil South" \
  --sku B1

# Crie um application insights
az monitor app-insights component create \
  --app "fcg-pagamentos-staging-ai" \
  --location "Brazil South" \
  --resource-group "fcg-pagamentos-staging-rg" \
  --application-type web
```

### 2. Deploy da API

```bash
# Crie o web app
az webapp create \
  --name "fcg-pagamentos-staging-api" \
  --resource-group "fcg-pagamentos-staging-rg" \
  --plan "fcg-staging-plan" \
  --runtime "DOTNETCORE:8.0"

# Configure as variáveis de ambiente
az webapp config appsettings set \
  --name "fcg-pagamentos-staging-api" \
  --resource-group "fcg-pagamentos-staging-rg" \
  --settings \
    "ConnectionStrings__DefaultConnection"="sua_connection_string_staging" \
    "AzureStorage__ConnectionString"="sua_storage_connection_staging" \
    "ApplicationInsights__ConnectionString"="sua_ai_connection_staging" \
    "ASPNETCORE_ENVIRONMENT"="Staging"

# Configure o .NET version
az webapp config set \
  --name "fcg-pagamentos-staging-api" \
  --resource-group "fcg-pagamentos-staging-rg" \
  --net-framework-version "v8.0"
```

### 3. Deploy via GitHub Actions

Crie o arquivo `.github/workflows/deploy-staging.yml`:

```yaml
name: Deploy to Staging

on:
  push:
    branches: [ develop, staging ]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --verbosity normal --configuration Release
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'fcg-pagamentos-staging-api'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_STAGING }}
        package: ./src/FCGPagamentos.API
```

### 4. Configuração do Banco Staging

```bash
# Use Azure Database for PostgreSQL ou configure um PostgreSQL em VM
# Configure as regras de firewall para permitir conexão do App Service

# Execute as migrações
az webapp ssh --name "fcg-pagamentos-staging-api" --resource-group "fcg-pagamentos-staging-rg"

# Dentro do SSH, execute:
dotnet ef database update --project /home/site/wwwroot
```

## 🚀 Ambiente de Produção

### 1. Criação dos Recursos de Produção

```bash
# Crie um resource group para produção
az group create --name "fcg-pagamentos-prod-rg" --location "Brazil South"

# Crie uma storage account com redundância
az storage account create \
  --name "fcgprodstorage" \
  --resource-group "fcg-pagamentos-prod-rg" \
  --location "Brazil South" \
  --sku Standard_GRS \
  --encryption-services blob file

# Crie um app service plan com mais recursos
az appservice plan create \
  --name "fcg-prod-plan" \
  --resource-group "fcg-pagamentos-prod-rg" \
  --location "Brazil South" \
  --sku P1V2

# Crie um application insights para produção
az monitor app-insights component create \
  --app "fcg-pagamentos-prod-ai" \
  --location "Brazil South" \
  --resource-group "fcg-pagamentos-prod-rg" \
  --application-type web
```

### 2. Deploy da API de Produção

```bash
# Crie o web app de produção
az webapp create \
  --name "fcg-pagamentos-prod-api" \
  --resource-group "fcg-pagamentos-prod-rg" \
  --plan "fcg-prod-plan" \
  --runtime "DOTNETCORE:8.0"

# Configure as variáveis de ambiente de produção
az webapp config appsettings set \
  --name "fcg-pagamentos-prod-api" \
  --resource-group "fcg-pagamentos-prod-rg" \
  --settings \
    "ConnectionStrings__DefaultConnection"="sua_connection_string_prod" \
    "AzureStorage__ConnectionString"="sua_storage_connection_prod" \
    "ApplicationInsights__ConnectionString"="sua_ai_connection_prod" \
    "ASPNETCORE_ENVIRONMENT"="Production" \
    "ASPNETCORE_URLS"="https://+:443;http://+:80"

# Configure HTTPS
az webapp update \
  --name "fcg-pagamentos-prod-api" \
  --resource-group "fcg-pagamentos-prod-rg" \
  --https-only true

# Configure autoscaling
az monitor autoscale create \
  --resource-group "fcg-pagamentos-prod-rg" \
  --resource "fcg-pagamentos-prod-api" \
  --resource-type "Microsoft.Web/sites" \
  --name "fcg-pagamentos-autoscale" \
  --min-count 2 \
  --max-count 10 \
  --count 3
```

### 3. Pipeline de Deploy de Produção

Crie o arquivo `.github/workflows/deploy-production.yml`:

```yaml
name: Deploy to Production

on:
  push:
    branches: [ main, master ]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --verbosity normal --configuration Release
    
    - name: Security scan
      run: dotnet list package --vulnerable
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'fcg-pagamentos-prod-api'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_PROD }}
        package: ./src/FCGPagamentos.API
    
    - name: Health check
      run: |
        sleep 30
        curl -f https://fcg-pagamentos-prod-api.azurewebsites.net/health || exit 1
```

### 4. Configuração do API Gateway

```bash
# Crie um API Management
az apim create \
  --name "fcg-pagamentos-apim" \
  --resource-group "fcg-pagamentos-prod-rg" \
  --location "Brazil South" \
  --publisher-email "admin@fcg.com.br" \
  --publisher-name "FCG Pagamentos"

# Crie uma API
az apim api create \
  --resource-group "fcg-pagamentos-prod-rg" \
  --service-name "fcg-pagamentos-apim" \
  --api-id "payments-api" \
  --display-name "FCG Pagamentos API" \
  --path "/api/v1" \
  --protocols https

# Configure as operações
az apim api operation create \
  --resource-group "fcg-pagamentos-prod-rg" \
  --service-name "fcg-pagamentos-apim" \
  --api-id "payments-api" \
  --operation-id "create-payment" \
  --method POST \
  --url-template "/payments"

az apim api operation create \
  --resource-group "fcg-pagamentos-prod-rg" \
  --service-name "fcg-pagamentos-apim" \
  --api-id "payments-api" \
  --operation-id "get-payment" \
  --method GET \
  --url-template "/payments/{id}"
```

## 🔧 Configurações de Infraestrutura

### 1. Configuração de Monitoramento

```bash
# Configure alertas no Application Insights
az monitor metrics alert create \
  --name "high-error-rate" \
  --resource-group "fcg-pagamentos-prod-rg" \
  --scopes "sua_app_insights_resource_id" \
  --condition "avg requests/failedRequests > 0.05" \
  --description "High error rate detected"

# Configure alertas de disponibilidade
az monitor metrics alert create \
  --name "low-availability" \
  --resource-group "fcg-pagamentos-prod-rg" \
  --scopes "sua_app_insights_resource_id" \
  --condition "avg availability/availabilityPercentage < 99.9" \
  --description "Low availability detected"
```

### 2. Configuração de Backup

```bash
# Configure backup automático do banco
az postgres flexible-server backup create \
  --resource-group "fcg-pagamentos-prod-rg" \
  --name "seu-postgres-server" \
  --backup-name "daily-backup"

# Configure backup do App Service
az webapp config backup create \
  --resource-group "fcg-pagamentos-prod-rg" \
  --webapp-name "fcg-pagamentos-prod-api" \
  --backup-name "daily-backup"
```

### 3. Configuração de Segurança

```bash
# Configure regras de firewall
az webapp config access-restriction add \
  --resource-group "fcg-pagamentos-prod-rg" \
  --name "fcg-pagamentos-prod-api" \
  --rule-name "allow-corporate-ip" \
  --action Allow \
  --ip-address "sua_ip_corporativa"

# Configure autenticação
az webapp auth update \
  --resource-group "fcg-pagamentos-prod-rg" \
  --name "fcg-pagamentos-prod-api" \
  --enabled true \
  --action LoginWithAzureActiveDirectory
```

## 🧪 Testes de Deploy

### 1. Testes de Smoke

```bash
# Teste básico de saúde
curl -f https://fcg-pagamentos-prod-api.azurewebsites.net/health

# Teste de criação de pagamento
curl -X POST https://fcg-pagamentos-prod-api.azurewebsites.net/payments \
  -H "Content-Type: application/json" \
  -d '{"amount": 10.00, "currency": "BRL", "description": "Teste", "payerId": "TEST", "paymentMethod": "PIX"}'

# Teste de consulta
curl -f https://fcg-pagamentos-prod-api.azurewebsites.net/payments/{id}
```

### 2. Testes de Performance

```bash
# Use Apache Bench ou similar
ab -n 1000 -c 10 https://fcg-pagamentos-prod-api.azurewebsites.net/health

# Use Artillery para testes mais complexos
artillery run load-test.yml
```

## 🚨 Troubleshooting

### Problemas Comuns

#### Erro de Conexão com Banco
```bash
# Verifique as regras de firewall
az postgres flexible-server firewall-rule list \
  --resource-group "fcg-pagamentos-prod-rg" \
  --name "seu-postgres-server"

# Verifique a connection string
az webapp config appsettings list \
  --name "fcg-pagamentos-prod-api" \
  --resource-group "fcg-pagamentos-prod-rg"
```

#### Erro de Deploy
```bash
# Verifique os logs de deploy
az webapp log tail \
  --name "fcg-pagamentos-prod-api" \
  --resource-group "fcg-pagamentos-prod-rg"

# Verifique o status do App Service
az webapp show \
  --name "fcg-pagamentos-prod-api" \
  --resource-group "fcg-pagamentos-prod-rg"
```

#### Performance Lenta
```bash
# Verifique as métricas
az monitor metrics list \
  --resource "sua_app_service_resource_id" \
  --metric "HttpResponseTime" \
  --interval PT1H

# Verifique o Application Insights
az monitor app-insights component show \
  --app "fcg-pagamentos-prod-ai" \
  --resource-group "fcg-pagamentos-prod-rg"
```

## 📊 Monitoramento Pós-Deploy

### 1. Métricas Importantes
- **Response Time**: < 200ms para 95% das requisições
- **Error Rate**: < 1%
- **Availability**: > 99.9%
- **Queue Depth**: < 100 mensagens

### 2. Dashboards Recomendados
- **Operacional**: Health checks, error rates, response times
- **Negócio**: Volume de pagamentos, valores processados
- **Infraestrutura**: CPU, memória, disco, rede

### 3. Alertas Essenciais
- **Critical**: API indisponível, erro rate > 5%
- **Warning**: Response time > 500ms, queue depth > 50
- **Info**: Deploy realizado, backup concluído

## 🔄 Rollback

### Procedimento de Rollback
```bash
# 1. Identifique a versão anterior
az webapp deployment list \
  --name "fcg-pagamentos-prod-api" \
  --resource-group "fcg-pagamentos-prod-rg"

# 2. Faça rollback para a versão anterior
az webapp deployment source config-zip \
  --resource-group "fcg-pagamentos-prod-rg" \
  --name "fcg-pagamentos-prod-api" \
  --src "versao_anterior.zip"

# 3. Verifique se o rollback foi bem-sucedido
curl -f https://fcg-pagamentos-prod-api.azurewebsites.net/health
```

## 📚 Recursos Adicionais

### Documentação
- [Azure App Service](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/)
- [Azure API Management](https://docs.microsoft.com/en-us/azure/api-management/)
- [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

### Ferramentas
- [Azure DevOps](https://azure.microsoft.com/en-us/services/devops/)
- [GitHub Actions](https://github.com/features/actions)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/)
- [Azure Portal](https://portal.azure.com/)

---

**Versão do Guia**: 1.0.0  
**Mantido por**: Equipe de DevOps FCG
