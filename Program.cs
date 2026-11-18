// Program.cs (.NET 10 Minimal API)
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// 1. SERVIÇOS (Injeção de Dependência)
// ====================================================================
// Adicionando suporte nativo a Health Checks (Essencial para Kubernetes Liveness/Readiness)
builder.Services.AddHealthChecks();

var app = builder.Build();

// ====================================================================
// 2. CONFIGURAÇÕES E VALIDAÇÕES (12-Factor App - Fator III)
// ====================================================================
// Lê a variável de ambiente injetada pelo ConfigMap/Secret do Kubernetes
var dbString = builder.Configuration["DB_CONNECTION_STRING"];

if (string.IsNullOrWhiteSpace(dbString))
{
    // Fail-Fast: O Pod deve falhar imediatamente na inicialização se faltar infraestrutura
    throw new ArgumentException("ERRO CRÍTICO: A variável DB_CONNECTION_STRING é obrigatória no ambiente!");
}

// Log padronizado (Aparecerá no comando 'kubectl logs')
app.Logger.LogInformation("Aplicação iniciada. Conectando ao banco no IP: {DbString}", dbString);

// ====================================================================
// 3. ENDPOINTS E ROTAS
// ====================================================================
// Endpoint de diagnóstico interrogado ativamente pelo Kubernetes (Aula 5)
app.MapHealthChecks("/health");

// Rota principal: Demonstra o balanceamento de carga exibindo o nome do Pod
app.MapGet("/", () => new 
{
    Message = "K8s Course API",
    PodName = Environment.MachineName, // No K8s, MachineName devolve o nome exato do Pod
    Framework = ".NET 10 Minimal API",
    Version = "v1",
    Timestamp = DateTime.UtcNow
});

// ====================================================================
// 4. EXECUÇÃO
// ====================================================================
app.Run();