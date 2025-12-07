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
- Escalar de 1 a 5 réplicas
- Baseado em CPU (70%) e Memória (80%)
- Políticas de scale up/down otimizadas

### `ingress.yaml`
Ingress usando AWS Load Balancer Controller (ALB):
- ALB internet-facing
- Health checks configurados
- Paths: `/payments` e `/health`

## Deploy

O deploy é realizado automaticamente via GitHub Actions quando há push para a branch `main` ou manualmente via `workflow_dispatch`.

### Pré-requisitos:
- Cluster EKS criado na AWS
- AWS Load Balancer Controller instalado no cluster
- Secret `EKS_CLUSTER_NAME` configurado no GitHub

### Ordem de aplicação:
1. Namespace
2. ConfigMap e Secret
3. ECR Secret (para pull de imagens)
4. Deployment
5. Service
6. HPA
7. Ingress

## Monitoramento

- Health checks: `GET /health`
- Métricas: `GET /metrics`
- Status do HPA: `kubectl get hpa payments-hpa -n payments`


