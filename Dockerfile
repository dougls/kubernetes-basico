# Etapa 1: Build da Aplicação
FROM [mcr.microsoft.com/dotnet/sdk:10.0](https://mcr.microsoft.com/dotnet/sdk:10.0) AS build
WORKDIR /src

# Restaura dependências primeiro para aproveitar o cache de camadas do Docker
COPY ["K8sCourseApi.csproj", "./"]
RUN dotnet restore "K8sCourseApi.csproj"

# Copia o restante do código e realiza a compilação
COPY . .
RUN dotnet publish "K8sCourseApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime (Imagem menor e mais segura usando Alpine)
FROM [mcr.microsoft.com/dotnet/aspnet:10.0-alpine](https://mcr.microsoft.com/dotnet/aspnet:10.0-alpine) AS final
WORKDIR /app
EXPOSE 8080

# Define variáveis de ambiente úteis
ENV ASPNETCORE_URLS=http://+:8080

# Segurança: Executa a aplicação como usuário sem privilégios administrativos
USER app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "K8sCourseApi.dll"]
