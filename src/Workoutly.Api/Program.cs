using MySqlConnector;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;
using Workoutly.Api.Auth;
using Workoutly.Api.Data;
using Workoutly.Api.Exercises;

var migrateOnly = args.Contains("--migrate");
var builder = WebApplication.CreateBuilder(args.Where(argument => argument != "--migrate").ToArray());
builder.Services.AddSingleton<MySqlDatabase>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 210_000);
builder.Services.AddSingleton<IPasswordHasher<AccountProfile>, PasswordHasher<AccountProfile>>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<ExerciseService>();
builder.Services.AddAuthentication("Session")
    .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>("Session", _ => { });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("account", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
        }));
});
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
        var code = exception is MySqlException mysql ? $" (código MySQL {mysql.Number})" : string.Empty;
        Console.Error.WriteLine($"Não foi possível preparar o banco{code}. Confira a configuração, as permissões e a disponibilidade do MySQL.");
        if (exception.InnerException is System.Net.Sockets.SocketException socket)
            Console.Error.WriteLine($"Falha de transporte: {socket.SocketErrorCode}.");
        Environment.ExitCode = 1;
    }
    await app.DisposeAsync();
    return;
}

app.Use(async (context, next) =>
{
    var protectedResource = context.Request.Path.StartsWithSegments("/auth") || context.Request.Path.StartsWithSegments("/exercises");
    if (protectedResource)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!context.Request.IsHttps && !app.Environment.IsDevelopment())
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { message = "Use HTTPS para acessar sua conta." });
            return;
        }
    }
    try { await next(context); }
    catch (Exception exception) when (protectedResource &&
        exception is MySqlException or InvalidOperationException or ArgumentException)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new { message = "Serviço indisponível. Tente novamente em instantes." });
    }
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapAccountEndpoints();
app.MapExerciseEndpoints();

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
