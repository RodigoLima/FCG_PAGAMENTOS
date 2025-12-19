# FCG Pagamentos API

Sistema de processamento de pagamentos desenvolvido em .NET 8 com arquitetura limpa, observabilidade completa e integração com Azure Storage.

## 🚀 Tecnologias

- **.NET 8** - Framework principal
- **PostgreSQL** - Banco de dados
- **AWS SQS** - Comunicação assíncrona via filas (MassTransit)
- **OpenTelemetry** - Observabilidade e métricas
- **Application Insights** - APM e monitoramento
- **Grafana** - Visualização de métricas
- **Serilog** - Logging estruturado
- **FluentValidation** - Validação de dados
- **Swagger** - Documentação da API

## 📋 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) (para serviços auxiliares)
- [PostgreSQL](https://www.postgresql.org/download/) ou Docker

## 🛠️ Configuração do Ambiente

### 1. Clone o repositório
```bash
git clone https://github.com/RodigoLima/FCG_PAGAMENTOS.git
cd FCG_PAGAMENTOS
```

### 2. Configure as variáveis de ambiente

Crie um arquivo `appsettings.Development.json` na pasta `src/FCGPagamentos.API/`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Database=fcg_pagamentos;Username=postgres;Password=sua_senha"
  },
  "AzureStorage": {
    "ConnectionString": "UseDevelopmentStorage=true"
  },
  "InternalAuth": {
    "Token": "seu-token-de-desenvolvimento"
  },
  "ApplicationInsights": {
    "ConnectionString": "sua-connection-string-do-app-insights"
  }
}
```

### 3. Inicie os serviços auxiliares

```bash
# Inicia o Azurite (emulador do Azure Storage)
docker-compose up -d
```

### 4. Execute as migrações do banco

```bash
cd src/FCGPagamentos.API
dotnet ef database update
```

## 🏃‍♂️ Executando a Aplicação

### Desenvolvimento
```bash
cd src/FCGPagamentos.API
dotnet run
```

A API estará disponível em:
- **API**: `https://localhost:7000`
- **Swagger**: `https://localhost:7000/swagger`

### Produção
```bash
cd src/FCGPagamentos.API
dotnet publish -c Release
dotnet FCGPagamentos.API.dll
```

## 📚 Documentação da API

### Endpoints Principais

#### Criar Pagamento
```http
POST /payments
Content-Type: application/json

{
  "user_id": "uuid",
  "game_id": "uuid", 
  "amount": 100.50,
  "currency": "BRL",
  "method": "credit_card"
}
```

#### Consultar Pagamento
```http
GET /payments/{payment_id}
```

### Endpoints Internos

- `GET /health` - Health check
- `GET /metrics` - Métricas da aplicação
- `GET /internal/status` - Status interno do sistema

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes específicos
dotnet test tests/FCGPagamentos.Tests/
```

## 📊 Observabilidade

O sistema inclui observabilidade completa com:

- **Logs estruturados** com Serilog
- **Métricas** com OpenTelemetry
- **Tracing distribuído** com correlation IDs
- **Health checks** para monitoramento
- **Application Insights** para APM
- **Grafana** para visualização

### Logs
Os logs são salvos em `logs/fcg-pagamentos-YYYYMMDD.txt` e também enviados para o console.

### Métricas
Acesse as métricas em `/metrics` para monitoramento com Prometheus/Grafana.

## 🔄 Comunicação Assíncrona

Este microsserviço utiliza **AWS SQS** via **MassTransit** para comunicação assíncrona:

- **Publica**: Mensagens `PaymentRequestedMessage` na fila `payments-to-process` quando um pagamento é criado
- **Worker**: O Payments Worker consome essas mensagens e processa os pagamentos de forma assíncrona

### Fluxo de Processamento

1. API recebe requisição de criação de pagamento
2. Cria registro no banco de dados
3. Publica `PaymentRequestedMessage` na fila SQS
4. Retorna resposta imediata ao cliente
5. Worker processa o pagamento de forma assíncrona

Para mais detalhes sobre a arquitetura completa e fluxo assíncrono, consulte a documentação no repositório do [GameService](../fcg.GameService/docs/).

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture**:

```
src/
├── FCGPagamentos.API/          # Camada de apresentação
├── FCGPagamentos.Application/  # Casos de uso e regras de aplicação
├── FCGPagamentos.Domain/       # Entidades e regras de negócio
└── FCGPagamentos.Infrastructure/ # Implementações concretas
```

## 🔧 Desenvolvimento

### Adicionando novas funcionalidades

1. **Domain**: Defina entidades e eventos no domínio
2. **Application**: Crie casos de uso e DTOs
3. **Infrastructure**: Implemente repositórios e serviços externos
4. **API**: Crie endpoints e middleware

### Padrões utilizados

- **CQRS** - Separação entre comandos e consultas
- **Event Sourcing** - Armazenamento de eventos
- **Repository Pattern** - Abstração de acesso a dados
- **Dependency Injection** - Inversão de controle

## 🐳 Docker

Para executar com Docker:

```bash
# Build da aplicação
docker build -t fcg-pagamentos .

# Executar container
docker run -p 7000:7000 fcg-pagamentos
```

