# 🏗️ Diagramas de Arquitetura - FCG PAGAMENTOS

## 📋 Visão Geral

Este documento contém os diagramas de arquitetura do microsserviço de pagamentos em diferentes níveis de abstração, desde a visão geral do sistema até os detalhes de implementação.

## 🎯 Níveis de Arquitetura

1. **Visão Geral do Sistema** - Contexto e integração com outros serviços
2. **Arquitetura de Microsserviços** - Estrutura interna e comunicação
3. **Arquitetura de Dados** - Modelos de dados e persistência
4. **Arquitetura de Segurança** - Autenticação, autorização e proteção
5. **Arquitetura de Deploy** - Infraestrutura e pipelines

## 🌐 1. Visão Geral do Sistema

### Diagrama de Contexto
```mermaid
graph TB
    subgraph "Sistemas Externos"
        CLIENT[Cliente/Usuário]
        MERCHANT[Comerciante]
        BANK[Banco/Instituição Financeira]
    end
    
    subgraph "API Gateway"
        APIGW[Azure API Management]
    end
    
    subgraph "Microsserviços"
        PAYMENT[FCG Pagamentos API]
        NOTIFICATION[Notificações]
        ANALYTICS[Analytics]
    end
    
    subgraph "Infraestrutura"
        DB[(PostgreSQL)]
        QUEUE[Azure Queue]
        STORAGE[Azure Storage]
        AI[Application Insights]
    end
    
    subgraph "Worker"
        FUNCTIONS[Azure Functions]
    end
    
    CLIENT --> APIGW
    MERCHANT --> APIGW
    APIGW --> PAYMENT
    APIGW --> NOTIFICATION
    APIGW --> ANALYTICS
    
    PAYMENT --> DB
    PAYMENT --> QUEUE
    PAYMENT --> STORAGE
    PAYMENT --> AI
    
    QUEUE --> FUNCTIONS
    FUNCTIONS --> DB
    FUNCTIONS --> AI
    
    BANK -.-> FUNCTIONS
    
    style PAYMENT fill:#e1f5fe
    style APIGW fill:#fff3e0
    style FUNCTIONS fill:#f3e5f5
```

### Fluxo de Comunicação
```mermaid
sequenceDiagram
    participant C as Cliente
    participant G as API Gateway
    participant P as Pagamentos API
    participant Q as Azure Queue
    participant W as Worker
    participant B as Banco
    participant N as Notificações
    
    C->>G: POST /payments
    G->>P: Forward Request
    P->>B: Save Payment (Pending)
    P->>Q: Send Message
    P->>C: 202 Accepted
    
    Q->>W: Process Message
    W->>B: Update Status (Processing)
    W->>B: Update Status (Completed)
    W->>N: Send Success Notification
    W->>C: Update Client (Webhook)
```

## 🏢 2. Arquitetura de Microsserviços

### Arquitetura Hexagonal (Clean Architecture)
```mermaid
graph TB
    subgraph "API Layer (Driving Adapters)"
        CONTROLLER[PaymentController]
        MIDDLEWARE[Middleware Stack]
        VALIDATION[FluentValidation]
    end
    
    subgraph "Application Layer (Use Cases)"
        CREATE[CreatePaymentHandler]
        GET[GetPaymentHandler]
        VALIDATOR[CreatePaymentValidator]
    end
    
    subgraph "Domain Layer (Core Business)"
        PAYMENT[Payment Entity]
        MONEY[Money Value Object]
        STATUS[PaymentStatus Enum]
        EVENTS[Domain Events]
    end
    
    subgraph "Infrastructure Layer (Driven Adapters)"
        REPO[PaymentRepository]
        EVENTSTORE[EventStore]
        QUEUE[AzureQueuePublisher]
        DB[PostgreSQL]
    end
    
    CONTROLLER --> CREATE
    CONTROLLER --> GET
    CREATE --> VALIDATOR
    CREATE --> PAYMENT
    GET --> PAYMENT
    
    PAYMENT --> MONEY
    PAYMENT --> STATUS
    PAYMENT --> EVENTS
    
    CREATE --> REPO
    CREATE --> EVENTSTORE
    CREATE --> QUEUE
    REPO --> DB
    EVENTSTORE --> DB
    
    style PAYMENT fill:#f3e5f5
    style CREATE fill:#e8f5e8
    style CONTROLLER fill:#e1f5fe
    style REPO fill:#fff3e0
```

### Estrutura de Camadas
```mermaid
graph LR
    subgraph "Presentation"
        API[API Endpoints]
        SWAGGER[Swagger/OpenAPI]
        HEALTH[Health Checks]
    end
    
    subgraph "Application"
        HANDLERS[Command/Query Handlers]
        VALIDATORS[Validators]
        DTOs[Data Transfer Objects]
    end
    
    subgraph "Domain"
        ENTITIES[Entities]
        VALUE_OBJECTS[Value Objects]
        ENUMS[Enums]
        EVENTS[Domain Events]
    end
    
    subgraph "Infrastructure"
        REPOSITORIES[Repositories]
        DATABASE[Database]
        QUEUES[Message Queues]
        TELEMETRY[Telemetry]
    end
    
    API --> HANDLERS
    HANDLERS --> ENTITIES
    ENTITIES --> REPOSITORIES
    REPOSITORIES --> DATABASE
    
    style ENTITIES fill:#f3e5f5
    style HANDLERS fill:#e8f5e8
    style API fill:#e1f5fe
    style REPOSITORIES fill:#fff3e0
```

## 💾 3. Arquitetura de Dados

### Modelo de Dados
```mermaid
erDiagram
    PAYMENTS {
        uuid id PK
        decimal amount
        string currency
        string description
        string payer_id
        string payment_method
        string status
        string status_details
        jsonb metadata
        timestamp created_at
        timestamp updated_at
        timestamp processed_at
    }
    
    PAYMENT_EVENTS {
        uuid id PK
        uuid payment_id FK
        string event_type
        jsonb event_data
        timestamp occurred_at
        string correlation_id
    }
    
    PAYMENT_STATUS_HISTORY {
        uuid id PK
        uuid payment_id FK
        string old_status
        string new_status
        string reason
        timestamp changed_at
        string changed_by
    }
    
    PAYMENTS ||--o{ PAYMENT_EVENTS : "has"
    PAYMENTS ||--o{ PAYMENT_STATUS_HISTORY : "tracks"
```

### Fluxo de Eventos (Event Sourcing)
```mermaid
graph LR
    subgraph "Commands"
        CREATE[CreatePayment]
        PROCESS[ProcessPayment]
        COMPLETE[CompletePayment]
    end
    
    subgraph "Events"
        PAYMENT_CREATED[PaymentCreated]
        PAYMENT_PROCESSING[PaymentProcessing]
        PAYMENT_COMPLETED[PaymentCompleted]
        PAYMENT_FAILED[PaymentFailed]
    end
    
    subgraph "Event Store"
        ES[Event Store Table]
        SNAPSHOTS[Snapshots]
    end
    
    CREATE --> PAYMENT_CREATED
    PROCESS --> PAYMENT_PROCESSING
    COMPLETE --> PAYMENT_COMPLETED
    
    PAYMENT_CREATED --> ES
    PAYMENT_PROCESSING --> ES
    PAYMENT_COMPLETED --> ES
    
    ES --> SNAPSHOTS
    
    style CREATE fill:#e8f5e8
    style PAYMENT_CREATED fill:#f3e5f5
    style ES fill:#fff3e0
```

### Estratégia de Persistência
```mermaid
graph TB
    subgraph "Write Model"
        COMMAND[Command Handler]
        AGGREGATE[Payment Aggregate]
        EVENT_STORE[Event Store]
    end
    
    subgraph "Read Model"
        QUERY[Query Handler]
        DTO[Payment DTO]
        VIEW[Payment View]
    end
    
    subgraph "Storage"
        EVENTS_TABLE[Events Table]
        PAYMENTS_TABLE[Payments Table]
        INDEXES[Database Indexes]
    end
    
    COMMAND --> AGGREGATE
    AGGREGATE --> EVENT_STORE
    EVENT_STORE --> EVENTS_TABLE
    
    QUERY --> DTO
    DTO --> VIEW
    VIEW --> PAYMENTS_TABLE
    
    PAYMENTS_TABLE --> INDEXES
    
    style AGGREGATE fill:#f3e5f5
    style COMMAND fill:#e8f5e8
    style QUERY fill:#e1f5fe
```

## 🔒 4. Arquitetura de Segurança

### Camadas de Segurança
```mermaid
graph TB
    subgraph "External Security"
        WAF[Web Application Firewall]
        DDoS[DDoS Protection]
        SSL[SSL/TLS Termination]
    end
    
    subgraph "API Security"
        AUTH[Authentication]
        AUTHOR[Authorization]
        RATE_LIMIT[Rate Limiting]
        VALIDATION[Input Validation]
    end
    
    subgraph "Application Security"
        ENCRYPTION[Data Encryption]
        LOGGING[Security Logging]
        AUDIT[Audit Trail]
    end
    
    subgraph "Infrastructure Security"
        NETWORK[Network Security]
        FIREWALL[Firewall Rules]
        ACCESS[Access Control]
    end
    
    WAF --> AUTH
    AUTH --> AUTHOR
    AUTHOR --> ENCRYPTION
    ENCRYPTION --> NETWORK
    
    style AUTH fill:#ffebee
    style ENCRYPTION fill:#e8f5e8
    style NETWORK fill:#fff3e0
```

### Fluxo de Autenticação
```mermaid
sequenceDiagram
    participant C as Client
    participant G as API Gateway
    participant A as Auth Service
    participant P as Payment API
    participant D as Database
    
    C->>G: Request with API Key
    G->>A: Validate API Key
    A->>G: Valid/Invalid Response
    
    alt Valid API Key
        G->>P: Forward Request
        P->>D: Process Request
        P->>G: Response
        G->>C: Success Response
    else Invalid API Key
        G->>C: 401 Unauthorized
    end
```

## 🚀 5. Arquitetura de Deploy

### Infraestrutura no Azure
```mermaid
graph TB
    subgraph "Azure Resources"
        subgraph "Networking"
            VNET[Virtual Network]
            NSG[Network Security Groups]
            SUBNET[Subnets]
        end
        
        subgraph "Compute"
            APP_SERVICE[App Service Plan]
            WEB_APP[Web App]
            FUNCTIONS[Azure Functions]
        end
        
        subgraph "Storage"
            STORAGE_ACCOUNT[Storage Account]
            QUEUE[Azure Queue]
            BLOB[Blob Storage]
        end
        
        subgraph "Database"
            POSTGRES[PostgreSQL Server]
            BACKUP[Backup Storage]
        end
        
        subgraph "Monitoring"
            APP_INSIGHTS[Application Insights]
            LOG_ANALYTICS[Log Analytics]
            ALERTS[Alert Rules]
        end
    end
    
    VNET --> SUBNET
    SUBNET --> WEB_APP
    SUBNET --> FUNCTIONS
    SUBNET --> POSTGRES
    
    WEB_APP --> STORAGE_ACCOUNT
    FUNCTIONS --> STORAGE_ACCOUNT
    WEB_APP --> POSTGRES
    FUNCTIONS --> POSTGRES
    
    WEB_APP --> APP_INSIGHTS
    FUNCTIONS --> APP_INSIGHTS
    
    style WEB_APP fill:#e1f5fe
    style FUNCTIONS fill:#f3e5f5
    style POSTGRES fill:#e8f5e8
```

### Pipeline de CI/CD
```mermaid
graph LR
    subgraph "Source Control"
        GITHUB[GitHub Repository]
        BRANCHES[Feature Branches]
    end
    
    subgraph "CI Pipeline"
        BUILD[Build]
        TEST[Test]
        SCAN[Security Scan]
        PACKAGE[Package]
    end
    
    subgraph "CD Pipeline"
        STAGING[Deploy to Staging]
        TESTING[Integration Tests]
        PRODUCTION[Deploy to Production]
        MONITOR[Monitor & Rollback]
    end
    
    subgraph "Environments"
        DEV[Development]
        STG[Staging]
        PROD[Production]
    end
    
    GITHUB --> BUILD
    BUILD --> TEST
    TEST --> SCAN
    SCAN --> PACKAGE
    
    PACKAGE --> STAGING
    STAGING --> TESTING
    TESTING --> PRODUCTION
    PRODUCTION --> MONITOR
    
    STAGING --> STG
    PRODUCTION --> PROD
    
    style BUILD fill:#e8f5e8
    style TEST fill:#fff3e0
    style PRODUCTION fill:#ffebee
```

## 📊 6. Arquitetura de Monitoramento

### Observabilidade
```mermaid
graph TB
    subgraph "Application"
        LOGS[Structured Logs]
        METRICS[Business Metrics]
        TRACES[Distributed Traces]
    end
    
    subgraph "Infrastructure"
        RESOURCE_METRICS[Resource Metrics]
        HEALTH_CHECKS[Health Checks]
        ALERTS[Alerting]
    end
    
    subgraph "External Services"
        DEPENDENCIES[Dependency Health]
        PERFORMANCE[Performance Metrics]
        ERRORS[Error Tracking]
    end
    
    subgraph "Monitoring Tools"
        APP_INSIGHTS[Application Insights]
        LOG_ANALYTICS[Log Analytics]
        DASHBOARDS[Dashboards]
        NOTIFICATIONS[Notifications]
    end
    
    LOGS --> APP_INSIGHTS
    METRICS --> APP_INSIGHTS
    TRACES --> APP_INSIGHTS
    
    RESOURCE_METRICS --> LOG_ANALYTICS
    HEALTH_CHECKS --> LOG_ANALYTICS
    
    DEPENDENCIES --> APP_INSIGHTS
    PERFORMANCE --> APP_INSIGHTS
    ERRORS --> APP_INSIGHTS
    
    APP_INSIGHTS --> DASHBOARDS
    LOG_ANALYTICS --> DASHBOARDS
    
    DASHBOARDS --> ALERTS
    ALERTS --> NOTIFICATIONS
    
    style APP_INSIGHTS fill:#e1f5fe
    style DASHBOARDS fill:#fff3e0
    style ALERTS fill:#ffebee
```

## 🔄 7. Padrões de Arquitetura

### Padrões Utilizados
```mermaid
graph TB
    subgraph "Architectural Patterns"
        HEXAGONAL[Hexagonal Architecture]
        CQRS[CQRS Pattern]
        EVENT_SOURCING[Event Sourcing]
        SAGA[Saga Pattern]
    end
    
    subgraph "Design Patterns"
        REPOSITORY[Repository Pattern]
        FACTORY[Factory Pattern]
        STRATEGY[Strategy Pattern]
        OBSERVER[Observer Pattern]
    end
    
    subgraph "Integration Patterns"
        MESSAGE_QUEUE[Message Queue]
        WEBHOOK[Webhook Pattern]
        API_GATEWAY[API Gateway]
        CIRCUIT_BREAKER[Circuit Breaker]
    end
    
    HEXAGONAL --> REPOSITORY
    CQRS --> FACTORY
    EVENT_SOURCING --> OBSERVER
    SAGA --> MESSAGE_QUEUE
    
    style HEXAGONAL fill:#f3e5f5
    style CQRS fill:#e8f5e8
    style EVENT_SOURCING fill:#e1f5fe
```

## 📈 8. Escalabilidade e Performance

### Estratégias de Escalabilidade
```mermaid
graph TB
    subgraph "Horizontal Scaling"
        LOAD_BALANCER[Load Balancer]
        MULTIPLE_INSTANCES[Multiple API Instances]
        AUTO_SCALING[Auto Scaling Rules]
    end
    
    subgraph "Database Scaling"
        READ_REPLICAS[Read Replicas]
        SHARDING[Database Sharding]
        CACHING[Redis Cache]
    end
    
    subgraph "Queue Scaling"
        MULTIPLE_QUEUES[Multiple Queues]
        MULTIPLE_WORKERS[Multiple Workers]
        PARTITIONING[Queue Partitioning]
    end
    
    subgraph "Performance"
        CDN[Content Delivery Network]
        COMPRESSION[Response Compression]
        OPTIMIZATION[Query Optimization]
    end
    
    LOAD_BALANCER --> MULTIPLE_INSTANCES
    MULTIPLE_INSTANCES --> AUTO_SCALING
    
    READ_REPLICAS --> CACHING
    MULTIPLE_QUEUES --> MULTIPLE_WORKERS
    
    CDN --> COMPRESSION
    COMPRESSION --> OPTIMIZATION
    
    style LOAD_BALANCER fill:#e1f5fe
    style READ_REPLICAS fill:#e8f5e8
    style MULTIPLE_QUEUES fill:#f3e5f5
```

## 🎯 9. Decisões de Arquitetura

### Trade-offs e Decisões

| Aspecto | Decisão | Justificativa | Alternativas Consideradas |
|---------|---------|---------------|---------------------------|
| **Banco de Dados** | PostgreSQL | ACID, JSON support, Open source | SQL Server, MySQL |
| **Mensageria** | Azure Queue | Simplicidade, Integração Azure | RabbitMQ, Kafka |
| **Arquitetura** | Hexagonal | Testabilidade, Flexibilidade | MVC, Clean Architecture |
| **Event Sourcing** | Implementado | Audit trail, Replay capability | CRUD tradicional |
| **API Gateway** | Azure APIM | Gerenciamento centralizado | Kong, AWS API Gateway |
| **Monitoramento** | Application Insights | Integração Azure, AI/ML | Prometheus, Grafana |

### Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| **Perda de dados** | Baixa | Alto | Backup automático, Event sourcing |
| **Performance** | Média | Média | Cache, Read replicas, Auto-scaling |
| **Segurança** | Média | Alto | WAF, Rate limiting, Input validation |
| **Disponibilidade** | Baixa | Alto | Multi-region, Health checks, Circuit breaker |

## 📚 10. Referências e Recursos

### Documentação
- [Azure Architecture Center](https://docs.microsoft.com/en-us/azure/architecture/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)

### Ferramentas
- [Draw.io](https://draw.io/) - Criação de diagramas
- [Mermaid](https://mermaid-js.github.io/mermaid/) - Diagramas em Markdown
- [Azure Architecture Icons](https://docs.microsoft.com/en-us/azure/architecture/icons/) - Ícones oficiais

---

**Versão dos Diagramas**: 1.0.0  
**Mantido por**: Equipe de Arquitetura FCG
