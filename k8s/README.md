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

**Importante:** O Service deve permanecer como `ClusterIP` (não altere para `LoadBalancer`). O Load Balancer (ALB) é criado automaticamente pelo AWS Load Balancer Controller através do Ingress. Esta é a prática recomendada para EKS.

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
O ALB criado por este ingress pode ser usado como backend do API Gateway. 

**Importante:** O Service está configurado como `ClusterIP` (interno), o que é a prática recomendada. O Load Balancer (ALB) é criado automaticamente pelo AWS Load Balancer Controller quando o Ingress é aplicado. Não é necessário alterar o Service para LoadBalancer.

**Verificando se o ALB foi criado:**

1. **Via kubectl (recomendado):**
```bash
# Verificar status do Ingress e obter o DNS do ALB
kubectl get ingress payments-ingress -n payments

# Obter apenas o DNS
kubectl get ingress payments-ingress -n payments -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'

# Verificar detalhes completos
kubectl describe ingress payments-ingress -n payments
```

2. **Via Console AWS:**
- Acesse: EC2 → Load Balancers
- Procure por um ALB com nome começando com `k8s-fcgservices-` ou similar
- O ALB terá as tags: `Environment=production`, `Service=fcg-services`, `ManagedBy=k8s`

**Se o ALB não aparecer:**
- Verifique se o AWS Load Balancer Controller está instalado e rodando:
```bash
kubectl get pods -n kube-system | grep aws-load-balancer-controller
```
- Verifique os logs do controller:
```bash
kubectl logs -n kube-system -l app.kubernetes.io/name=aws-load-balancer-controller
```
- Verifique se o Ingress foi aplicado corretamente:
```bash
kubectl get ingress -n payments
```

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


