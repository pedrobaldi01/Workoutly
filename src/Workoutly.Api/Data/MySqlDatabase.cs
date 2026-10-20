using MySqlConnector;

namespace Workoutly.Api.Data;

/// <summary>Acesso ao banco somente na API. Abrir a API não executa DDL.</summary>
public sealed class MySqlDatabase(IConfiguration configuration)
{
    public const int CurrentSchemaVersion = 1;

    private string? ConnectionString => configuration.GetConnectionString("Workoutly");
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString);

    public async Task<DatabaseReadiness> CheckReadinessAsync(CancellationToken cancellationToken)
    {
        if (!IsConfigured)
            return new(false, "not_configured", null);

        // Limitar a sondagem; nunca devolver endereço, usuário ou erro do driver ao cliente.
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(3));
        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(timeout.Token);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT MAX(version) FROM schema_migrations";
            var value = await command.ExecuteScalarAsync(timeout.Token);
            int? version = value is null or DBNull ? null : Convert.ToInt32(value);
            return version == CurrentSchemaVersion
                ? new(true, "ready", version)
                : new(false, "schema_pending", version);
        }
        catch (MySqlException exception) when (exception.Number == 1146)
        {
            return new(false, "schema_pending", null);
        }
        catch (MySqlException)
        {
            return new(false, "unreachable", null);
        }
        catch (ArgumentException)
        {
            return new(false, "invalid_configuration", null);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(false, "unreachable", null);
        }
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        // Script idempotente, embutido no assembly para funcionar após publicar a API.
        var assembly = typeof(MySqlDatabase).Assembly;
        await using var stream = assembly.GetManifestResourceStream(
            "Workoutly.Api.Data.Migrations.001_bootstrap.sql")
            ?? throw new InvalidOperationException("Migração inicial não encontrada.");
        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private MySqlConnection CreateConnection()
    {
        var connectionString = ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Defina ConnectionStrings__Workoutly no servidor.");

        var options = new MySqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(options.Database))
            throw new ArgumentException("A conexão deve indicar um banco de dados.");
        options.PersistSecurityInfo = false;
        options.ConnectionTimeout = 3;
        options.DefaultCommandTimeout = 5;
        return new MySqlConnection(options.ConnectionString);
    }
}

public sealed record DatabaseReadiness(bool Ready, string Database, int? SchemaVersion);
