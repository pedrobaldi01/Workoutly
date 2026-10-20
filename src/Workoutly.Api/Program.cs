var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Liveness da API. Não consulta nem valida MySQL nesta etapa.
app.MapGet("/health", () => Results.Ok(new
{
    service = "Workoutly.Api",
    status = "ok",
    databaseConfigured = false
}));

app.Run();

// Expõe o ponto de entrada para os testes HTTP com WebApplicationFactory.
public partial class Program { }
