using MySqlConnector;
using Workoutly.Api.Data;

var migrateOnly = args.Contains("--migrate");
var builder = WebApplication.CreateBuilder(args.Where(argument => argument != "--migrate").ToArray());
builder.Services.AddSingleton<MySqlDatabase>();
var app = builder.Build();

if (migrateOnly)
{
    try
    {
        await app.Services.GetRequiredService<MySqlDatabase>().MigrateAsync();
        Console.WriteLine("Migração MySQL aplicada com sucesso.");
    }
    catch (Exception exception) when (exception is MySqlException or ArgumentException or InvalidOperationException)
    {
        Console.Error.WriteLine("Não foi possível preparar o banco. Confira a configuração, as permissões e a disponibilidade do MySQL.");
        Environment.ExitCode = 1;
    }
    await app.DisposeAsync();
    return;
}

// Liveness não depende do banco. Readiness comprova acesso ao esquema preparado.
app.MapGet("/health", (MySqlDatabase database) => Results.Ok(new
{
    service = "Workoutly.Api",
    status = "ok",
    databaseConfigured = database.IsConfigured
}));

app.MapGet("/health/ready", async (MySqlDatabase database, CancellationToken cancellationToken) =>
{
    var result = await database.CheckReadinessAsync(cancellationToken);
    return Results.Json(new
    {
        status = result.Ready ? "ready" : "unavailable",
        database = result.Database,
        schemaVersion = result.SchemaVersion
    }, statusCode: result.Ready ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
});

app.Run();

// Expõe o ponto de entrada para os testes HTTP com WebApplicationFactory.
public partial class Program { }
