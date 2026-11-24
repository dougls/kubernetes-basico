# Instalação de Controllers da Kubernetes Gateway API via Helm

Este guia apresenta como instalar e configurar um controller para a **Kubernetes Gateway API** utilizando Helm. Escolha **uma** das opções abaixo de acordo com a necessidade do seu cluster.

## Comparativo Rápido

| Controller | `controllerName` | Indicado para |
| :--- | :--- | :--- |
| **Envoy Gateway** | `gateway.envoyproxy.io/gatewayclass-controller` | Padrão CNCF, alta performance, recursos avançados de L7 e extensibilidade |
| **NGINX Gateway Fabric** | `gateway.nginx.org/nginx-gateway-controller` | Times já familiarizados com o ecossistema NGINX |
| **Traefik** | `traefik.io/gateway-controller` | Setup rápido, baixa curva de aprendizado e dashboard integrado |
| **Kong Ingress Controller** | `konghq.com/kic-gateway-controller` | Cenários que exigem recursos de API Management e ecossistema de plugins |

---

## Pré-requisitos

- Cluster Kubernetes funcional.
- `kubectl` configurado com acesso ao cluster.
- `helm` v3.8+ (com suporte a registros OCI).

---

## Passo 1: Instalar as CRDs da Gateway API (Opcional)

Alguns controllers (como o Envoy Gateway) já instalam as CRDs automaticamente. Para os demais (como NGINX Gateway Fabric, Traefik e Kong), instale o canal padrão (**standard**) da Gateway API antes de subir o controller:

```bash
kubectl apply --server-side -f https://github.com/kubernetes-sigs/gateway-api/releases/download/v1.4.1/standard-install.yaml
```

**Nota:** Se o seu provedor de nuvem (EKS, GKE, AKS) já gerencia as CRDs da Gateway API, pule esta etapa.

---

## Passo 2: Escolher e Instalar o Controller

Execute apenas a seção correspondente ao controller escolhido.

### Opção A: Envoy Gateway

Instale o controller via registro OCI (já inclui as CRDs por padrão):

```bash
helm upgrade --install eg oci://docker.io/envoyproxy/gateway-helm \
  --version v1.10.0 \
  --namespace envoy-gateway-system \
  --create-namespace \
  --wait
```

Crie a GatewayClass (`gateway-class-envoy.yaml`):

```yaml
apiVersion: gateway.networking.k8s.io/v1
kind: GatewayClass
metadata:
  name: gateway-class-envoy
spec:
  controllerName: gateway.envoyproxy.io/gatewayclass-controller
```

Aplique o manifesto:

```bash
kubectl apply -f gateway-class-envoy.yaml
```

### Opção B: NGINX Gateway Fabric

Instale o controller via registro OCI:

```bash
helm upgrade --install ngf oci://ghcr.io/nginx/charts/nginx-gateway-fabric \
  --namespace nginx-gateway \
  --create-namespace \
  --wait
```

**Nota:** O chart já cria uma GatewayClass padrão chamada `nginx`. Caso queira criar uma personalizada (`gateway-class-nginx.yaml`):

```yaml
apiVersion: gateway.networking.k8s.io/v1
kind: GatewayClass
metadata:
  name: gateway-class-nginx
spec:
  controllerName: gateway.nginx.org/nginx-gateway-controller
```

Aplique o manifesto:

```bash
kubectl apply -f gateway-class-nginx.yaml
```

### Opção C: Traefik

Adicione o repositório Helm e instale o Traefik com o provider da Gateway API habilitado:

```bash
helm repo add traefik https://traefik.github.io/charts
helm repo update

helm upgrade --install traefik traefik/traefik \
  --namespace traefik \
  --create-namespace \
  --set providers.kubernetesGateway.enabled=true \
  --wait
```

**Nota:** O chart cria automaticamente uma GatewayClass padrão chamada `traefik`. Para criar uma customizada (`gateway-class-traefik.yaml`):

```yaml
apiVersion: gateway.networking.k8s.io/v1
kind: GatewayClass
metadata:
  name: gateway-class-traefik
spec:
  controllerName: traefik.io/gateway-controller
```

Aplique o manifesto:

```bash
kubectl apply -f gateway-class-traefik.yaml
```

### Opção D: Kong Ingress Controller

Adicione o repositório Helm e instale o Kong:

```bash
helm repo add kong https://charts.konghq.com
helm repo update

helm upgrade --install kong kong/ingress \
  --namespace kong \
  --create-namespace \
  --wait
```

Crie a GatewayClass (`gateway-class-kong.yaml`):

```yaml
apiVersion: gateway.networking.k8s.io/v1
kind: GatewayClass
metadata:
  name: gateway-class-kong
spec:
  controllerName: konghq.com/kic-gateway-controller
```

Aplique o manifesto:

```bash
kubectl apply -f gateway-class-kong.yaml
```

---

## Passo 3: Validar a Instalação

Verifique se a GatewayClass escolhida aparece com o status `ACCEPTED: True`:

```bash
kubectl get gatewayclass
```

Saída esperada (exemplo com Envoy):

```text
NAME                  CONTROLLER                                      ACCEPTED   AGE
gateway-class-envoy   gateway.envoyproxy.io/gatewayclass-controller   True       15s
```

---

## Desinstalação

Remova a release do Helm e o namespace correspondente ao controller instalado:

```bash
# Envoy Gateway
helm uninstall eg -n envoy-gateway-system && kubectl delete ns envoy-gateway-system

# NGINX Gateway Fabric
helm uninstall ngf -n nginx-gateway && kubectl delete ns nginx-gateway

# Traefik
helm uninstall traefik -n traefik && kubectl delete ns traefik

# Kong
helm uninstall kong -n kong && kubectl delete ns kong
```

---

## Sugestões de melhorias

- [ ] Adicionar exemplos de manifestos `Gateway` e `HTTPRoute` genéricos ao README, que funcionem com qualquer um desses controllers apenas trocando o `gatewayClassName`.
- [ ] Incluir Cilium e Istio como opções extras de controllers da Gateway API, com instalação via Helm.
- [ ] Adicionar uma seção explicando como integrar o `cert-manager` com a Gateway API para emissão automática de certificados TLS.