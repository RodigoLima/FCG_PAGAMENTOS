# 📚 Documentação Técnica da API - FCG PAGAMENTOS

## 🔗 Base URL
```
Development: https://localhost:7001
Production: https://fcg-pagamentos-api.azurewebsites.net
```

## 🔐 Autenticação
Atualmente a API não requer autenticação, mas está preparada para implementar:
- API Key para comunicação entre microsserviços
- JWT para autenticação de usuários (opcional)

## 📋 Endpoints

### 1. Criação de Pagamento

#### `POST /payments`
Cria um novo pagamento e o envia para processamento assíncrono.

**Headers:**
```
Content-Type: application/json
Accept: application/json
```

**Request Body:**
```json
{
  "amount": 150.75,
  "currency": "BRL",
  "description": "Pagamento de serviço de consultoria",
  "payerId": "USR_12345",
  "paymentMethod": "PIX",
  "metadata": {
    "orderId": "ORD_789",
    "customerEmail": "cliente@exemplo.com"
  }
}
```

**Campos Obrigatórios:**
- `amount`: Valor do pagamento (decimal > 0)
- `currency`: Moeda (string, 3 caracteres)
- `description`: Descrição do pagamento (string, 1-500 caracteres)
- `payerId`: ID do pagador (string, 1-100 caracteres)
- `paymentMethod`: Método de pagamento (string, valores válidos: PIX, CREDIT_CARD, DEBIT_CARD, BANK_TRANSFER)

**Campos Opcionais:**
- `metadata`: Dados adicionais em formato JSON

**Response Codes:**

**202 Accepted (Sucesso)**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 150.75,
  "currency": "BRL",
  "status": "Pending",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "location": "/payments/550e8400-e29b-41d4-a716-446655440000"
}
```

**400 Bad Request (Dados inválidos)**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid request data",
  "errors": {
    "amount": ["Amount must be greater than 0"],
    "currency": ["Currency must be exactly 3 characters"]
  }
}
```

**422 Unprocessable Entity (Erro de validação)**
```json
{
  "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "Validation failed",
  "errors": {
    "paymentMethod": ["Payment method 'INVALID_METHOD' is not supported"]
  }
}
```

**500 Internal Server Error (Erro interno)**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred"
}
```

### 2. Consulta de Pagamento

#### `GET /payments/{id}`
Consulta um pagamento específico pelo ID.

**Headers:**
```
Accept: application/json
```

**Path Parameters:**
- `id`: UUID do pagamento (formato: 550e8400-e29b-41d4-a716-446655440000)

**Response Codes:**

**200 OK (Pagamento encontrado)**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 150.75,
  "currency": "BRL",
  "description": "Pagamento de serviço de consultoria",
  "payerId": "USR_12345",
  "paymentMethod": "PIX",
  "status": "Completed",
  "statusDetails": "Pagamento processado com sucesso",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:35:00Z",
  "processedAt": "2024-01-15T10:35:00Z",
  "metadata": {
    "orderId": "ORD_789",
    "customerEmail": "cliente@exemplo.com"
  }
}
```

**404 Not Found (Pagamento não encontrado)**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Payment with ID '550e8400-e29b-41d4-a716-446655440000' not found"
}
```

**400 Bad Request (ID inválido)**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid payment ID format"
}
```

### 3. Health Check

#### `GET /health`
Verifica a saúde do serviço e suas dependências.

**Headers:**
```
Accept: application/json
```

**Response Codes:**

**200 OK (Serviço saudável)**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "checks": {
    "database": "Healthy",
    "azure_queue": "Healthy",
    "application_insights": "Healthy"
  },
  "version": "1.0.0",
  "environment": "Production"
}
```

**503 Service Unavailable (Serviço não saudável)**
```json
{
  "status": "Unhealthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "checks": {
    "database": "Healthy",
    "azure_queue": "Unhealthy",
    "application_insights": "Healthy"
  },
  "version": "1.0.0",
  "environment": "Production"
}
```

## 📊 Modelos de Dados

### PaymentDto
```json
{
  "id": "string (UUID)",
  "amount": "decimal",
  "currency": "string (3 chars)",
  "description": "string (1-500 chars)",
  "payerId": "string (1-100 chars)",
  "paymentMethod": "string (enum)",
  "status": "string (enum)",
  "statusDetails": "string (nullable)",
  "createdAt": "datetime (ISO 8601)",
  "updatedAt": "datetime (ISO 8601)",
  "processedAt": "datetime (ISO 8601, nullable)",
  "metadata": "object (nullable)"
}
```

### CreatePaymentCommand
```json
{
  "amount": "decimal (required, > 0)",
  "currency": "string (required, 3 chars)",
  "description": "string (required, 1-500 chars)",
  "payerId": "string (required, 1-100 chars)",
  "paymentMethod": "string (required, enum)",
  "metadata": "object (optional)"
}
```

### GetPaymentQuery
```json
{
  "id": "string (UUID, required)"
}
```

## 🔄 Status de Pagamento

### PaymentStatus Enum
```csharp
public enum PaymentStatus
{
    Pending,        // Aguardando processamento
    Processing,     // Em processamento
    Completed,      // Concluído com sucesso
    Failed,         // Falhou
    Cancelled,      // Cancelado
    Refunded        // Reembolsado
}
```

### Transições de Status
```
Pending → Processing → Completed
Pending → Processing → Failed
Pending → Cancelled
Completed → Refunded
```

## 📝 Validações

### Regras de Validação

#### Amount
- Deve ser maior que 0
- Máximo de 2 casas decimais
- Máximo de 999,999.99

#### Currency
- Exatamente 3 caracteres
- Valores válidos: BRL, USD, EUR, ARS, CLP

#### Description
- Entre 1 e 500 caracteres
- Não pode conter caracteres especiais perigosos

#### PayerId
- Entre 1 e 100 caracteres
- Apenas letras, números, underscore e hífen

#### PaymentMethod
- Valores válidos: PIX, CREDIT_CARD, DEBIT_CARD, BANK_TRANSFER

## 🚀 Rate Limiting

**Limites atuais:**
- **POST /payments**: 100 requests por minuto por IP
- **GET /payments/{id}**: 1000 requests por minuto por IP
- **GET /health**: 1000 requests por minuto por IP

**Headers de Rate Limiting:**
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1642233600
```

## 🔍 Logs e Rastreamento

### Correlation ID
Cada requisição recebe um correlation ID único para rastreamento:

```
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440000
```

### Logs Estruturados
```json
{
  "timestamp": "2024-01-15T10:30:00Z",
  "level": "Information",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "operation": "CreatePayment",
  "userId": "USR_12345",
  "amount": 150.75,
  "currency": "BRL"
}
```

## 🧪 Testes

### Endpoints de Teste

#### `POST /test/payments` (Apenas Development)
Cria um pagamento de teste com dados mockados.

#### `GET /test/payments/{id}` (Apenas Development)
Consulta um pagamento de teste.

### Dados de Teste
```json
{
  "amount": 99.99,
  "currency": "BRL",
  "description": "Pagamento de teste",
  "payerId": "TEST_USER",
  "paymentMethod": "PIX"
}
```

## 📈 Métricas

### Métricas Disponíveis
- **Request Rate**: Requisições por segundo
- **Response Time**: Tempo médio de resposta
- **Error Rate**: Taxa de erro por endpoint
- **Success Rate**: Taxa de sucesso por endpoint
- **Queue Depth**: Tamanho da fila de processamento

### Endpoint de Métricas
```
GET /metrics (formato Prometheus)
```

## 🔒 Segurança

### Headers de Segurança
```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Strict-Transport-Security: max-age=31536000; includeSubDomains
```

### Validação de Input
- Sanitização de HTML
- Prevenção de SQL Injection
- Validação de tipos de dados
- Escape de caracteres especiais

## 📞 Suporte

### Códigos de Erro Comuns
- **1000**: Erro interno do sistema
- **1001**: Dados de entrada inválidos
- **1002**: Pagamento não encontrado
- **1003**: Serviço temporariamente indisponível
- **1004**: Limite de rate exceeded

### Contato
- **Documentação**: Este arquivo
- **Swagger**: `/swagger` quando rodando
- **Issues**: GitHub Issues do projeto
- **Suporte**: suporte@fcg.com.br

---

**Versão da Documentação**: 1.0.0  
**Mantido por**: Equipe de Desenvolvimento FCG
