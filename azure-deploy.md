# 🚀 Deploy no Azure Functions

## Pré-requisitos

1. **Azure CLI instalado**
2. **Conta Azure ativa**
3. **GitHub Actions configurado**

## Configuração Inicial

### 1. Login no Azure
```bash
az login
az account set --subscription "SUA_SUBSCRIPTION_ID"
```

### 2. Criar Resource Group
```bash
az group create --name fcg-pagamentos-rg --location "Brazil South"
```

### 3. Criar Storage Account
```bash
az storage account create \
  --name fcgpagamentosstorage \
  --resource-group fcg-pagamentos-rg \
  --location "Brazil South" \
  --sku Standard_LRS
```

### 4. Criar App Service Plan
```bash
az appservice plan create \
  --name fcg-pagamentos-plan \
  --resource-group fcg-pagamentos-rg \
  --location "Brazil South" \
  --sku B1 \
  --is-linux
```

### 5. Criar Function App
```bash
az functionapp create \
  --name fcg-pagamentos-api \
  --resource-group fcg-pagamentos-rg \
  --plan fcg-pagamentos-plan \
  --runtime dotnet-isolated \
  --functions-version 4 \
  --storage-account fcgpagamentosstorage
```

## Configuração do GitHub Actions

### 1. Obter Publish Profile
```bash
az functionapp deployment list-publishing-profiles \
  --name fcg-pagamentos-api \
  --resource-group fcg-pagamentos-rg \
  --xml
```

### 2. Adicionar Secret no GitHub
1. Vá para `Settings > Secrets and variables > Actions`
2. Adicione: `AZURE_FUNCTIONAPP_PUBLISH_PROFILE`
3. Cole o conteúdo do publish profile

## Deploy Automático

Após configurar os secrets, o deploy acontece automaticamente:

- **Push para `main`**: Deploy automático
- **Pull Request**: Apenas testes (sem deploy)
- **Manual**: Use `workflow_dispatch` no GitHub Actions

## Verificação

```bash
# Verificar status da Function App
az functionapp show \
  --name fcg-pagamentos-api \
  --resource-group fcg-pagamentos-rg

# Verificar logs
az functionapp logs tail \
  --name fcg-pagamentos-api \
  --resource-group fcg-pagamentos-rg
```

## URLs

- **API**: `https://fcg-pagamentos-api.azurewebsites.net`
- **Swagger**: `https://fcg-pagamentos-api.azurewebsites.net/swagger`
- **Health Check**: `https://fcg-pagamentos-api.azurewebsites.net/health`

## Monitoramento

- **Application Insights**: Configurado automaticamente
- **Logs**: Azure Monitor
- **Métricas**: Azure Portal
