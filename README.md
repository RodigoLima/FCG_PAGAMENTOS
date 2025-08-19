# FCG_PAGAMENTOS

## Arquitetura Hexagonal
/MinhaSolucaoHexagonal.sln  # O arquivo de solução que contém todos os projetos
├── /src                      # Pasta para todo o código-fonte da aplicação
│   ├── /MinhaApp.Core        # Projeto da CAMADA DO DOMÍNIO
│   │   ├── /Domain           # Entidades e Agregados
│   │   ├── /Ports            # Interfaces (Ports) do Domínio
│   │   ├── /Services         # Serviços de Domínio
│   │   └── /Events           # Eventos do Domínio
│   │
│   ├── /MinhaApp.Application # Projeto da CAMADA DE APLICAÇÃO
│   │   ├── /UseCases         # Lógica da aplicação
│   │   ├── /Commands         # Comandos (requisições) que a API recebe
│   │   └── /Queries          # Consultas para a API
│   │
│   ├── /MinhaApp.Infrastructure # Projeto da CAMADA DE INFRAESTRUTURA
│   │   ├── /Data               # Implementação do Repositório (EF Core)
│   │   ├── /Migrations         # Migrações do Entity Framework
│   │   └── /Services           # Implementações de serviços externos
│   │
│   ├── /MinhaApp.Api         # Projeto da CAMADA DE INFRAESTRUTURA (Entrada - API)
│   │   ├── /Controllers      # Controllers da API REST
│   │   └── /Startup.cs       # Configuração da injeção de dependência e middleware
│   │
│   └── /MinhaApp.Functions   # Projeto da CAMADA DE INFRAESTRUTURA (Entrada - Azure Functions)
│       └── /...              # Lógica das funções, que chamam a camada de Aplicação
│
└── /tests                    # Pasta para todos os projetos de testes
    ├── /MinhaApp.Core.Tests  # Testes Unitários para a Lógica de Negócio
    ├── /MinhaApp.Api.Tests   # Testes de Integração para a API
    └── /MinhaApp.EndToEnd.Tests # Testes de ponta a ponta