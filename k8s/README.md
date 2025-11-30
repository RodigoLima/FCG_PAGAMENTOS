# Kubernetes Manifests - Payments Service

Estrutura completa de orquestração Kubernetes para o serviço de pagamentos seguindo boas práticas.

## Arquivos

### `namespace.yaml`
Define o namespace `payments` para isolamento dos recursos.

### `configmap.yaml`
ConfigMap com configurações não sensíveis:
- AWS Region
- AWS Account ID
- SQS Queue Name

### `secret.yaml`
Secret com dados sensíveis:
- Connection String do PostgreSQL
- AWS Access Key
- AWS Secret Key
- AWS Session Token

### `deployment.yaml`
Deployment do serviço com:
- Health checks (liveness e readiness)
- Resource limits e requests
- Referências a ConfigMap e Secret
- Image pull secrets para ECR

### `service.yaml`
Service do tipo ClusterIP para expor o deployment internamente.

### `hpa.yaml`
Horizontal Pod Autoscaler configurado para:
- Escalar de 1 a 10 réplicas
- Baseado em CPU (70%) e Memória (80%)
- Políticas de scale up/down otimizadas

## Deploy

O deploy é realizado automaticamente via GitHub Actions quando há push para a branch `main` ou manualmente via `workflow_dispatch`.

### Ordem de aplicação:
1. Namespace
2. ConfigMap e Secret
3. Deployment
4. Service
5. HPA

## Monitoramento

- Health checks: `GET /health`
- Métricas: `GET /metrics`
- Status do HPA: `kubectl get hpa payments-hpa -n payments`


