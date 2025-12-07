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
Service do tipo LoadBalancer com anotação para criar um NLB (Network Load Balancer):
- Tipo: `LoadBalancer`
- Anotação: `service.beta.kubernetes.io/aws-load-balancer-type: "nlb"`
- Scheme: `internet-facing` (exposto para API Gateway)
- Porta: 80 (mapeada para 8080 do container)

**Importante:** Este Service cria um NLB diretamente, que será visível no console EC2 → Load Balancers. O NLB pode ser usado como backend do API Gateway.

### `hpa.yaml`
Horizontal Pod Autoscaler configurado para:
- Escalar de 1 a 5 réplicas
- Baseado em CPU (70%) e Memória (80%)
- Políticas de scale up/down otimizadas

### `ingress.yaml`
Ingress usando AWS Load Balancer Controller (ALB):
- ALB internet-facing (exposto para API Gateway)
- Health checks configurados
- Paths: `/payments` e `/health`
- Compartilha ALB com outros microserviços via `group.name: fcg-services`
- Ordem de prioridade: `100` (ajustar conforme necessário para outros serviços)
- Tags configuradas para identificação e gerenciamento
- Timeout de idle configurado para 60 segundos

**Integração com API Gateway:**
O NLB criado pelo Service pode ser usado como backend do API Gateway. O NLB é criado automaticamente quando o Service do tipo LoadBalancer é aplicado no cluster EKS.

**Verificando se o NLB foi criado:**

1. **Via kubectl (recomendado):**
```bash
# Verificar status do Service e obter o DNS do NLB
kubectl get service payments-service -n payments

# Obter apenas o DNS do NLB
kubectl get service payments-service -n payments -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'

# Verificar detalhes completos
kubectl describe service payments-service -n payments
```

2. **Via Console AWS:**
- Acesse: EC2 → Load Balancers
- Procure por um NLB (Network Load Balancer) com nome relacionado ao serviço
- O NLB será do tipo "Network" e estará com scheme "internet-facing"

**Se o NLB não aparecer:**
- Verifique se o Service foi aplicado corretamente:
```bash
kubectl get service -n payments
kubectl describe service payments-service -n payments
```
- Verifique os eventos do namespace:
```bash
kubectl get events -n payments --sort-by='.lastTimestamp'
```
- Aguarde alguns minutos, pois a criação do NLB pode levar 1-2 minutos

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


