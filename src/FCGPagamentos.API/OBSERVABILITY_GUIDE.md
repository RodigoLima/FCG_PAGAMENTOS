# 🔍 Guia de Observabilidade e Monitoramento

## 📋 Resumo das Melhorias

### ✅ **Código Reorganizado e Centralizado:**

1. **ObservabilityService**: Configuração centralizada de toda observabilidade
2. **ObservabilityConfigurationService**: Gerenciamento de configurações
3. **ObservabilityDebugService**: Debug e testes de observabilidade
4. **Program.cs**: Simplificado e limpo
5. **Endpoints de Debug**: Para testar configuração no Azure

### 🚀 **Funcionalidades de Debug:**

- **Logs detalhados** na inicialização da aplicação
- **Endpoints de debug** para testar configuração
- **Informações do sistema** e variáveis de ambiente
- **Testes automáticos** de Application Insights e OpenTelemetry

## 🔧 **Como Usar no Azure**

### **1. Configurar Variáveis de Ambiente:**

No Azure App Service, configure:

```bash
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=xxx;IngestionEndpoint=https://xxx.in.applicationinsights.azure.com/
```

### **2. Verificar Logs de Inicialização:**

Quando a aplicação iniciar, você verá logs como:

```
🚀 Aplicação iniciando...
🌍 Environment: Production
🔍 === OBSERVABILIDADE CONFIGURATION STATUS ===
✅ Application Insights: CONFIGURADO
   Connection String: InstrumentationKey=12345678...
🔧 OpenTelemetry Configuration:
   Console Exporter: DESABILITADO
   Sampling Ratio: 1
🌍 Environment: Production
🖥️ Machine: RD0003FF123456
🔍 === END OBSERVABILIDADE STATUS ===
✅ Application Insights: CONFIGURADO com connection string
✅ OpenTelemetry Tracing: Integrado com Application Insights
✅ OpenTelemetry Metrics: Integrado com Application Insights
✅ Serilog configurado - logs serão enviados via ILogger para Application Insights
```

### **3. Usar Endpoints de Debug:**

#### **Testar Observabilidade:**
```bash
GET /debug/observability
```
- Executa testes de Application Insights
- Executa testes de OpenTelemetry
- Envia eventos de teste
- Retorna status da configuração

#### **Verificar Configuração:**
```bash
GET /debug/config
```
- Retorna status da configuração
- Mostra se Application Insights está configurado
- Exibe configurações do OpenTelemetry

#### **Informações do Sistema:**
```bash
GET /debug/system
```
- Informações detalhadas do sistema
- Variáveis de ambiente
- Status dos recursos

## 📊 **O que Será Enviado para Application Insights**

### **1. Logs Estruturados:**
- Requisições de pagamento
- Sucessos e falhas
- Tempo de processamento
- Correlation IDs
- Exceções detalhadas

### **2. Métricas Customizadas:**
- Número de requisições
- Taxa de sucesso/falha
- Valores de pagamento
- Tempo de processamento
- Métricas de runtime (.NET)

### **3. Traces (OpenTelemetry):**
- HTTP requests/responses
- Dependências externas
- Performance de endpoints
- Correlation entre operações

### **4. Eventos Customizados:**
- `PaymentRequest`: Requisição de pagamento
- `PaymentSuccess`: Pagamento processado com sucesso
- `PaymentFailure`: Falha no processamento
- `ObservabilityDebugTest`: Testes de debug

## 🔍 **Como Verificar se Está Funcionando**

### **1. Logs da Aplicação:**
Verifique se aparecem as mensagens de configuração na inicialização.

### **2. Application Insights Portal:**
1. Acesse o portal do Azure
2. Vá para o Application Insights
3. Verifique as seções:
   - **Logs**: Para logs estruturados
   - **Metrics**: Para métricas customizadas
   - **Traces**: Para traces do OpenTelemetry
   - **Events**: Para eventos customizados

### **3. Queries Úteis no Application Insights:**

```kusto
// Verificar logs de pagamento
traces
| where customDimensions.PaymentId != ""
| order by timestamp desc

// Verificar eventos de debug
customEvents
| where name == "ObservabilityDebugTest"
| order by timestamp desc

// Verificar métricas de performance
customMetrics
| where name == "PaymentProcessingTime"
| summarize avg(value) by bin(timestamp, 5m)

// Verificar exceções
exceptions
| where customDimensions.Operation == "CreatePayment"
| order by timestamp desc
```

## 🛠️ **Troubleshooting**

### **Problema: Dados não aparecem no Application Insights**

**Soluções:**
1. Verifique se a connection string está correta
2. Confirme se as variáveis de ambiente estão configuradas
3. Use o endpoint `/debug/observability` para testar
4. Verifique os logs da aplicação para mensagens de erro
5. Aguarde até 5 minutos para os dados aparecerem

### **Problema: Logs aparecem mas métricas não**

**Soluções:**
1. Verifique se o TelemetryService está sendo injetado
2. Confirme se os endpoints estão usando o serviço
3. Use o endpoint `/debug/observability` para testar
4. Verifique se não há erros de serialização

### **Problema: Performance degradada**

**Soluções:**
1. Ajuste o sampling ratio no `appsettings.json`
2. Reduza o nível de log em produção
3. Configure adaptive sampling no Application Insights

## 📈 **Próximos Passos Recomendados**

1. **Alertas**: Configure alertas para métricas críticas
2. **Dashboards**: Crie dashboards personalizados
3. **Análise de Performance**: Use Application Insights Profiler
4. **Testes de Carga**: Configure testes automatizados
5. **Log Analytics**: Configure queries personalizadas

## 🔗 **Links Úteis**

- [Application Insights Documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [Serilog Documentation](https://serilog.net/)
- [Azure App Service Configuration](https://docs.microsoft.com/en-us/azure/app-service/configure-common)
