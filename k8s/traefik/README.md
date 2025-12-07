# Traefik Load Balancer

Configuração do Load Balancer para o Traefik Ingress Controller.

## Arquivos

### `service.yaml` (Principal)
Service do Traefik LoadBalancer para integração com API Gateway:
- Tipo: `LoadBalancer` (NLB)
- Scheme: `internal` (para uso interno com API Gateway)
- NodePort: atribuído automaticamente pelo Kubernetes
- Session Affinity: `ClientIP` (mantém conexão do mesmo cliente)

**Importante:** O NodePort será atribuído automaticamente pelo Kubernetes. Após a criação do service, verifique qual porta foi atribuída e configure o Target Group do API Gateway para usar essa porta.

### `service-backup.yaml` (Alternativo)
Versão alternativa sem porta fixa (fallback caso o service.yaml falhe).

## Uso

O arquivo `service.yaml` é aplicado automaticamente pela pipeline de CD após a instalação do Traefik.

Para aplicar manualmente:
```bash
kubectl apply -f k8s/traefik/service.yaml
```

## Verificação

Após aplicar, verifique qual porta foi atribuída:
```bash
kubectl get svc traefik-loadbalancer -n kube-system -o jsonpath='{.spec.ports[?(@.name=="http")].nodePort}'
```

Configure o Target Group do API Gateway para usar a porta retornada.

