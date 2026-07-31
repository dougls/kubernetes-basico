# Arquivos da Aplicação Base - .NET 10

Este documento contém os artefatos essenciais para a aplicação da disciplina, estruturados com foco em segurança e eficiência.

## 1. K8sCourseApi.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- Otimização recomendada para imagens baseadas em Alpine Linux -->
    <InvariantGlobalization>true</InvariantGlobalization>
  </PropertyGroup>

</Project>
```

## 2. Dockerfile (Multi-stage e Foco em Segurança)

O Dockerfile utiliza *multi-stage build* para garantir uma imagem final enxuta. Além disso, a configuração para executar o contêiner com um usuário não-root (`USER app`) é uma prática fundamental para estar em conformidade com as políticas de segurança (Pod Security Standards) do Kubernetes e facilitar futuras integrações com pipelines GitOps (como ArgoCD).

```dockerfile
# Etapa 1: Build da Aplicação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura dependências primeiro para aproveitar o cache de camadas do Docker
COPY ["K8sCourseApi.csproj", "./"]
RUN dotnet restore "K8sCourseApi.csproj"

# Copia o restante do código e realiza a compilação
COPY . .
RUN dotnet publish "K8sCourseApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime (Imagem menor e mais segura usando Alpine)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
EXPOSE 8080

# Define variáveis de ambiente úteis
ENV ASPNETCORE_URLS=http://+:8080

# Segurança: Executa a aplicação como usuário sem privilégios administrativos
USER app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "K8sCourseApi.dll"]
```

## 3. appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## Comandos Úteis para os Alunos (Local)

Para que os alunos validem o empacotamento antes de iniciar a orquestração:

```bash
# 1. Construir a imagem localmente
docker build -t k8scourse-api:v1 .

# 2. Executar o contêiner expondo na porta 8080
docker run -d -p 8080:8080 --name k8s-api k8scourse-api:v1

# 3. Testar os endpoints
curl http://localhost:8080/
curl http://localhost:8080/health
```
