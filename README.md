FCG_PAGAMENTOS
Este repositório contém a implementação de um sistema de pagamentos com uma arquitetura robusta e escalável, seguindo os princípios da Arquitetura Hexagonal.

```
├── /src
│   ├── /adapters
│   │   ├── /Driven
│   |   |   └── /FCGPagamentos.Infrastructure // Onde ficam os repositórios
│   │   └── /Driver
│   │       └── FCGPagamentos.API // Onde fica a nossa API, e somente ela tem acesso ao Domain
│   └── /Core
│       ├── /FCGPagamentos.Domain // Onde ficam todas as entidades
│       └── /Entities
│
└── /tests
    ├── /FCGPagamentos.API.Tests
```