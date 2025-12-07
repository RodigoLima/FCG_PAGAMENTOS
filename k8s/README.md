# Kubernetes Manifests - Payments Service

Estrutura completa de orquestração Kubernetes para o serviço de pagamentos seguindo boas práticas.

## Estrutura de Arquivos

```
k8s/
├── namespace.yaml          # Namespace do serviço
├── configmap.yaml          # Configurações não sensíveis
├── secret.yaml             # Dados sensíveis
├── deployment.yaml         # Deployment da aplicação
├── service.yaml            # Service (ClusterIP)
├── hpa.yaml               # Horizontal Pod Autoscaler
├── ingress.yaml           # Ingress (Traefik)
├── middleware.yaml        # Middleware Traefik (stripPrefix)
└── traefik/               # Configurações do Traefik
    ├── service.yaml       # LoadBalancer do Traefik (NodePort automático)
    └── service-backup.yaml # Versão alternativa
```

## Arquivos do Payments Service

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
Service do tipo **ClusterIP** (não expõe diretamente):
- Tipo: `ClusterIP`
- Porta: 80 (mapeada para 8080 do container)
- Acesso via Ingress/Traefik

### `hpa.yaml`
Horizontal Pod Autoscaler configurado para:
- Escalar de 1 a 5 réplicas
- Baseado em CPU (70%) e Memória (80%)
- Políticas de scale up/down otimizadas

### `ingress.yaml`
Ingress usando **Traefik** como Ingress Controller:
- IngressClassName: `traefik`
- Path: `/api/payments` (com middleware para remover prefixo)
- Health check: `/api/payments/health`

### `middleware.yaml`
Middleware do Traefik para remover o prefixo `/api/payments`:
- StripPrefix: remove `/api/payments` antes de encaminhar para o service
- Permite que o código C# receba requisições em `/` mesmo quando acessado via `/api/payments`

## Arquivos do Traefik

Ver documentação em `traefik/README.md`.

O Traefik é instalado automaticamente pela pipeline de CD via Helm.

## Arquitetura de Roteamento

```
API Gateway
    ↓
NLB (Traefik) - NodePort automático
    ↓
Traefik Ingress Controller
    ↓
Middleware (stripPrefix: /api/payments)
    ↓
Ingress (/api/payments → /)
    ↓
Service (payments-service:80)
    ↓
Pods (payments:8080)
```

## Deploy

O deploy é realizado automaticamente via GitHub Actions quando há push para a branch `main` ou manualmente via `workflow_dispatch`.

### Pré-requisitos:
- Cluster EKS criado na AWS
- Traefik instalado no cluster (instalado automaticamente pela pipeline)
- Secret `EKS_CLUSTER_NAME` configurado no GitHub

### Ordem de aplicação (automática):
1. Namespace
2. ConfigMap e Secret
3. ECR Secret (para pull de imagens)
4. Deployment
5. Service (ClusterIP)
6. HPA
7. Middleware (Traefik)
8. Ingress (Traefik)
9. Traefik LoadBalancer (NodePort atribuído automaticamente)

## Monitoramento

- Health checks: `GET /health` ou `GET /api/payments/health`
- Métricas: `GET /metrics`
- Status do HPA: `kubectl get hpa payments-hpa -n payments`
- Status do Traefik: `kubectl get pods -n kube-system -l app.kubernetes.io/name=traefik`

## Verificações Úteis

### Verificar porta do Traefik NLB:
```bash
kubectl get svc traefik-loadbalancer -n kube-system -o jsonpath='{.spec.ports[?(@.name=="http")].nodePort}'
```
Retorna a porta NodePort atribuída automaticamente. Configure o Target Group do API Gateway para usar essa porta.

### Verificar DNS do NLB:
```bash
kubectl get svc traefik-loadbalancer -n kube-system -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'
```

### Verificar status do middleware:
```bash
kubectl get middleware -n payments
```


