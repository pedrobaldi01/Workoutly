using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MySqlConnector;
using Workoutly.Api.Data;

namespace Workoutly.Api.Auth;

public sealed record AccountProfile(string Id, string Name, string Email);
public sealed record AccountSession(string AccessToken, DateTimeOffset ExpiresAt, AccountProfile User);

public sealed class AuthService(MySqlDatabase database, IPasswordHasher<AccountProfile> passwords, TimeProvider time)
{
    private readonly AccountProfile _dummy = new("", "", "");
    private readonly string _dummyHash = passwords.HashPassword(new("", "", ""), "timing-placeholder-only");

    public async Task<AccountSession?> RegisterAsync(string name, string email, string password, CancellationToken cancellationToken)
    {
        var profile = new AccountProfile(Guid.NewGuid().ToString(), name.Trim(), email.Trim().ToLowerInvariant());
        var hash = passwords.HashPassword(profile, password);
        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "INSERT INTO users (id, name, email, normalized_email, password_hash, created_at) VALUES (@id, @name, @email, @normalized, @hash, @now)";
            command.Parameters.AddWithValue("id", profile.Id);
            command.Parameters.AddWithValue("name", profile.Name);
            command.Parameters.AddWithValue("email", profile.Email);
            command.Parameters.AddWithValue("normalized", profile.Email.ToUpperInvariant());
            command.Parameters.AddWithValue("hash", hash);
            command.Parameters.AddWithValue("now", time.GetUtcNow().UtcDateTime);
            await command.ExecuteNonQueryAsync(cancellationToken);
            var session = await CreateSessionAsync(profile, connection, transaction, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return session;
        }
        catch (MySqlException exception) when (exception.Number == 1062)
        {
            return null;
        }
    }

    public async Task<AccountSession?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        AccountProfile? profile = null;
        string hash = _dummyHash;
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT id, name, email, password_hash FROM users WHERE normalized_email = @email LIMIT 1";
            command.Parameters.AddWithValue("email", email.Trim().ToUpperInvariant());
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                profile = new(reader.GetGuid(0).ToString(), reader.GetString(1), reader.GetString(2));
                hash = reader.GetString(3);
            }
        }
        var verification = passwords.VerifyHashedPassword(profile ?? _dummy, hash, password);
        if (profile is null || verification == PasswordVerificationResult.Failed)
            return null;
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "UPDATE users SET password_hash = @hash WHERE id = @id";
            command.Parameters.AddWithValue("hash", passwords.HashPassword(profile, password));
            command.Parameters.AddWithValue("id", profile.Id);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        return await CreateSessionAsync(profile, connection, null, cancellationToken);
    }

    public async Task<AccountProfile?> GetCurrentAsync(string token, CancellationToken cancellationToken)
    {
        if (token.Length != 43)
            return null;
        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT u.id, u.name, u.email FROM user_sessions s JOIN users u ON u.id = s.user_id WHERE s.token_hash = @hash AND s.expires_at > @now";
        command.Parameters.AddWithValue("hash", HashToken(token));
        command.Parameters.AddWithValue("now", time.GetUtcNow().UtcDateTime);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? new(reader.GetGuid(0).ToString(), reader.GetString(1), reader.GetString(2)) : null;
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken)
    {
        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM user_sessions WHERE token_hash = @hash";
        command.Parameters.AddWithValue("hash", HashToken(token));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<AccountSession> CreateSessionAsync(AccountProfile profile, MySqlConnection connection, MySqlTransaction? transaction, CancellationToken cancellationToken)
    {
        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var now = time.GetUtcNow();
        var expires = now.AddDays(7);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO user_sessions (token_hash, user_id, created_at, expires_at) VALUES (@hash, @user, @now, @expires)";
        command.Parameters.AddWithValue("hash", HashToken(token));
        command.Parameters.AddWithValue("user", profile.Id);
        command.Parameters.AddWithValue("now", now.UtcDateTime);
        command.Parameters.AddWithValue("expires", expires.UtcDateTime);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return new(token, expires, profile);
    }

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
