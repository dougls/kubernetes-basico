// Program.cs (.NET 10 Minimal API)
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Adicionando suporte nativo a Health Checks (Essencial para Kubernetes)
builder.Services.AddHealthChecks();

var app = builder.Build();

// Rota principal: Demonstra o balanceamento de carga exibindo o nome do Pod
app.MapGet("/", () => new 
{
    Message = "K8s Course API",
    PodName = Environment.MachineName,
    Framework = ".NET 10",
    Version = "v1",
    Timestamp = DateTime.UtcNow
});

// Endpoint nativo de Health Check (Liveness/Readiness na Aula 5)
app.MapHealthChecks("/health");

app.Run();
