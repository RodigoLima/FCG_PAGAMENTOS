# Traefik Load Balancer

Configuração do Load Balancer para o Traefik Ingress Controller.

## Arquivos

### `service.yaml` (Principal)
Service do Traefik com porta fixa (31551) para integração com API Gateway:
- Tipo: `LoadBalancer` (NLB)
- Scheme: `internal` (para uso interno com API Gateway)
- NodePort HTTP: `31551` (porta fixa para Target Group)
- NodePort HTTPS: `31552`
- Session Affinity: `ClientIP` (mantém conexão do mesmo cliente)

**Importante:** Este arquivo força a porta 31551 no NodePort para garantir que o Target Group do API Gateway aponte para a porta correta.

### `service-backup.yaml` (Alternativo)
Versão alternativa sem porta fixa (fallback caso o service.yaml falhe).

## Uso

O arquivo `service.yaml` é aplicado automaticamente pela pipeline de CD após a instalação do Traefik.

Para aplicar manualmente:
```bash
kubectl apply -f k8s/traefik/service.yaml
```

## Verificação

Após aplicar, verifique se a porta está correta:
```bash
kubectl get svc traefik-loadbalancer -n kube-system -o jsonpath='{.spec.ports[?(@.name=="http")].nodePort}'
```

Deve retornar: `31551`

