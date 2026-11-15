using Microsoft.AspNetCore.Identity;
using MySqlConnector;

namespace Workoutly.Api.Auth;

public sealed partial class AuthService
{
    public async Task<(string Status, AccountProfile? Profile)> UpdateProfileAsync(
        string id, string token, AuthEndpoints.ProfileUpdateRequest request, CancellationToken ct)
    {
        await using var connection = await database.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        var account = await LockAccountAsync(connection, transaction, id, token, ct);
        if (account is null) return ("expired", null);

        var email = request.Email!.Trim().ToLowerInvariant();
        if (email != account.Profile.Email && !VerifyPassword(account, request.CurrentPassword))
            return ("password", null);

        var profile = account.Profile with { Name = request.Name!.Trim(), Email = email };
        await using var command = AccountCommand(connection, transaction, id);
        command.CommandText = "UPDATE users SET name=@name, email=@email, normalized_email=@normalized WHERE id=@id";
        command.Parameters.AddWithValue("name", profile.Name);
        command.Parameters.AddWithValue("email", profile.Email);
        command.Parameters.AddWithValue("normalized", profile.Email.ToUpperInvariant());
        try { await command.ExecuteNonQueryAsync(ct); }
        catch (MySqlException exception) when (exception.Number == 1062) { return ("email_taken", null); }
        await transaction.CommitAsync(ct);
        return ("ok", profile);
    }

    public async Task<string> ChangePasswordAsync(string id, string token, string currentPassword, string newPassword, CancellationToken ct)
    {
        await using var connection = await database.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        var account = await LockAccountAsync(connection, transaction, id, token, ct);
        if (account is null) return "expired";
        if (!VerifyPassword(account, currentPassword)) return "password";

        await using var command = AccountCommand(connection, transaction, id);
        command.CommandText = """
            UPDATE users SET password_hash=@hash WHERE id=@id;
            DELETE FROM user_sessions WHERE user_id=@id;
            """;
        command.Parameters.AddWithValue("hash", passwords.HashPassword(account.Profile, newPassword));
        await command.ExecuteNonQueryAsync(ct);
        await transaction.CommitAsync(ct);
        return "ok";
    }

    public async Task<string> DeleteAccountAsync(string id, string token, string currentPassword, CancellationToken ct)
    {
        await using var connection = await database.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        var account = await LockAccountAsync(connection, transaction, id, token, ct);
        if (account is null) return "expired";
        if (!VerifyPassword(account, currentPassword)) return "password";

        await using var command = AccountCommand(connection, transaction, id);
        command.CommandText = """
            DELETE FROM workout_session_changes WHERE session_id IN (SELECT id FROM workout_sessions WHERE student_id=@id);
            DELETE FROM workout_session_sets WHERE session_id IN (SELECT id FROM workout_sessions WHERE student_id=@id);
            DELETE FROM workout_sessions WHERE student_id=@id;
            DELETE FROM workout_plan_items WHERE plan_id IN (SELECT id FROM workout_plans WHERE student_id=@id);
            DELETE FROM workout_plan_versions WHERE plan_id IN (SELECT id FROM workout_plans WHERE student_id=@id);
            DELETE FROM workout_plans WHERE student_id=@id;
            UPDATE workout_plan_versions SET author_id=NULL WHERE author_id=@id;
            UPDATE exercises SET owner_id=NULL, archived=TRUE WHERE owner_id=@id
                AND EXISTS (SELECT 1 FROM workout_plan_items i WHERE i.exercise_id=exercises.id);
            DELETE FROM users WHERE id=@id;
            """;
        await command.ExecuteNonQueryAsync(ct);
        await transaction.CommitAsync(ct);
        return "ok";
    }

    private async Task<LockedAccount?> LockAccountAsync(
        MySqlConnection connection, MySqlTransaction transaction, string id, string token, CancellationToken ct)
    {
        await using var command = AccountCommand(connection, transaction, id);
        command.CommandText = """
            SELECT u.name, u.email, r.role, u.password_hash
            FROM users u JOIN account_roles r ON r.user_id=u.id WHERE u.id=@id FOR UPDATE
            """;
        LockedAccount account;
        await using (var reader = await command.ExecuteReaderAsync(ct))
        {
            if (!await reader.ReadAsync(ct)) return null;
            account = new(new(id, reader.GetString(0), reader.GetString(1), reader.GetString(2)), reader.GetString(3));
        }

        command.CommandText = "SELECT 1 FROM user_sessions WHERE user_id=@id AND token_hash=@token AND expires_at>@now FOR UPDATE";
        command.Parameters.AddWithValue("token", HashToken(token));
        command.Parameters.AddWithValue("now", time.GetUtcNow().UtcDateTime);
        return await command.ExecuteScalarAsync(ct) is null ? null : account;
    }

    private static MySqlCommand AccountCommand(MySqlConnection connection, MySqlTransaction transaction, string id)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.Parameters.AddWithValue("id", id);
        return command;
    }

    private bool VerifyPassword(LockedAccount account, string? password) => !string.IsNullOrWhiteSpace(password) &&
        passwords.VerifyHashedPassword(account.Profile, account.PasswordHash, password) != PasswordVerificationResult.Failed;

    private sealed record LockedAccount(AccountProfile Profile, string PasswordHash);
}
