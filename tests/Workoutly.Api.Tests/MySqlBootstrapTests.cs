using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Workoutly.Api.Data;

namespace Workoutly.Api.Tests;

public sealed class MySqlBootstrapTests : IClassFixture<MySqlFixture>
{
    private readonly MySqlFixture _mysql;

    public MySqlBootstrapTests(MySqlFixture mysql) => _mysql = mysql;

    [MySqlFact]
    [Trait("Category", "MySql")]
    public async Task Migration_persists_schema_version_across_api_instances_and_can_be_reapplied()
    {
        await using (var first = new DatabaseApplication(_mysql.ConnectionString))
        {
            using var client = first.CreateClient();
            var pending = await client.GetAsync("/health/ready");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, pending.StatusCode);
            var pendingBody = await pending.Content.ReadFromJsonAsync<ReadinessBody>();
            Assert.Equal("schema_pending", pendingBody?.Database);

            var database = first.Services.GetRequiredService<MySqlDatabase>();
            await database.MigrateAsync();
            await database.MigrateAsync();
        }

        // Outra instância da API, sem reaplicar a migração: o estado precisa vir do MySQL.
        await using var restarted = new DatabaseApplication(_mysql.ConnectionString);
        using var restartedClient = restarted.CreateClient();
        var ready = await restartedClient.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
        var body = await ready.Content.ReadFromJsonAsync<ReadinessBody>();
        Assert.NotNull(body);
        Assert.Equal("ready", body.Status);
        Assert.Equal("ready", body.Database);
        Assert.Equal(7, body.SchemaVersion);
    }

    private sealed record ReadinessBody(string Status, string Database, int? SchemaVersion);
}

public sealed class MySqlFactAttribute : FactAttribute
{
    public MySqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WORKOUTLY_TEST_ADMIN_CONNECTION")))
            Skip = "Requer MySQL isolado. Execute python3 scripts/dev.py test-db; consulte docs/mysql.md.";
    }
}

public sealed class MySqlFixture : IAsyncLifetime
{
    // Somente este nome aleatório pode ser criado e removido; nunca limpar o banco da API.
    private readonly string _databaseName = $"workoutly_test_{Guid.NewGuid():N}";
    private string? _adminConnection;
    private bool _created;
    private readonly string _user = $"wly_{Guid.NewGuid():N}"[..28];
    private bool _userCreated;
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _adminConnection = Environment.GetEnvironmentVariable("WORKOUTLY_TEST_ADMIN_CONNECTION");
        if (string.IsNullOrWhiteSpace(_adminConnection))
            return;
        var options = new MySqlConnectionStringBuilder(_adminConnection)
        {
            Database = string.Empty,
            Pooling = false,
            ConnectionTimeout = 3
        };
        _adminConnection = options.ConnectionString;
        await using var connection = new MySqlConnection(_adminConnection);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE `{_databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci";
        await command.ExecuteNonQueryAsync();
        _created = true;
        var password = Guid.NewGuid().ToString("N");
        command.CommandText = $"CREATE USER '{_user}'@'%' IDENTIFIED BY '{password}'";
        await command.ExecuteNonQueryAsync();
        _userCreated = true;
        command.CommandText = $"GRANT ALL PRIVILEGES ON `{_databaseName}`.* TO '{_user}'@'%'";
        await command.ExecuteNonQueryAsync();
        options.Database = _databaseName;
        options.UserID = _user;
        options.Password = password;
        options.SslMode = MySqlSslMode.Required;
        ConnectionString = options.ConnectionString;
    }

    public async Task DisposeAsync()
    {
        if (!_created)
            return;
        await using var connection = new MySqlConnection(_adminConnection);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE `{_databaseName}`";
        await command.ExecuteNonQueryAsync();
        if (_userCreated)
        {
            command.CommandText = $"DROP USER '{_user}'@'%'";
            await command.ExecuteNonQueryAsync();
        }
    }
}
