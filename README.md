FCG_PAGAMENTOS
Este repositório contém a implementação de um sistema de pagamentos com uma arquitetura robusta e escalável, seguindo os princípios da Arquitetura Hexagonal. A solução é dividida em projetos com responsabilidades bem definidas, facilitando a manutenção e a testabilidade.

Estrutura do Repositório
A estrutura de pastas e projetos reflete a separação de camadas da Arquitetura Hexagonal, com o núcleo do domínio isolado das camadas de infraestrutura.

.
├── /src
│   ├── /MinhaApp.Core
│   │   ├── /Domain           
│   │   ├── /Ports            
│   │   ├── /Services         
│   │   └── /Events           
│   │
│   ├── /MinhaApp.Application 
│   │   ├── /UseCases         
│   │   ├── /Commands         
│   │   └── /Queries          
│   │
│   ├── /MinhaApp.Infrastructure 
│   │   ├── /Data               
│   │   ├── /Migrations         
│   │   └── /Services           
│   │
│   ├── /MinhaApp.Api         
│   │   ├── /Controllers      
│   │   └── /Startup.cs       
│   │
│   └── /MinhaApp.Functions   
│       └── /...              
│
└── /tests                    
    ├── /MinhaApp.Core.Tests  
    ├── /MinhaApp.Api.Tests   
    └── /MinhaApp.EndToEnd.Tests 
Detalhamento dos Projetos
src/
Contém o código-fonte da aplicação, organizado por camadas:

MinhaApp.Core: O coração do sistema. Contém a lógica de negócio pura, como Entidades e Agregados. Define as Ports (interfaces) que ditam como a comunicação com o mundo externo deve ocorrer, mantendo o domínio agnóstico a qualquer tecnologia.

MinhaApp.Application: Gerencia a orquestração da lógica do core. Contém os Use Cases, Commands e Queries que definem as operações que a aplicação pode realizar, traduzindo as requisições externas para o domínio.

MinhaApp.Infrastructure: A camada de Infraestrutura que lida com o "mundo externo". Contém as implementações concretas dos repositórios (usando Entity Framework Core) e outros serviços externos que o domínio precisa.

MinhaApp.Api: O adaptador de entrada para a API REST. Contém os Controllers que recebem as requisições HTTP e chamam a camada de Aplicação. Configura a injeção de dependência para conectar o domínio à infraestrutura.

MinhaApp.Functions: Outro adaptador de entrada, este para lógica de negócio baseada em eventos ou triggers de Azure Functions, que também se comunica com a camada de Aplicação.

tests/
Contém os projetos de testes automatizados, que garantem a qualidade e a robustez do software:

MinhaApp.Core.Tests: Testes Unitários focados em validar a lógica de negócio do domínio. Por não terem dependência de banco de dados, são rápidos e confiáveis.

MinhaApp.Api.Tests: Testes de Integração para validar o fluxo completo das APIs, incluindo a comunicação entre as camadas.

MinhaApp.EndToEnd.Tests: Testes de ponta a ponta que simulam o comportamento do usuário final para garantir que o sistema completo funcione como esperado.