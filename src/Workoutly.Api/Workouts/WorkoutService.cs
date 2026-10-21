using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using MySqlConnector;
using Workoutly.Api.Data;

namespace Workoutly.Api.Workouts;

public sealed record PerformedSet(int Repetitions, decimal WeightKg);
public sealed record PerformedExercise(Guid ExerciseId, PerformedSet[]? Sets);
public sealed record CreateWorkoutRequest(Guid Id, Guid PlanId, int PlanVersion, DateTimeOffset StartedAt, int DurationSeconds, string? Notes, PerformedExercise[]? Exercises);
public sealed record WorkoutExercise(string ExerciseId, string Name, string MuscleGroup, string Equipment, string Description, int PlannedSets, int PlannedRepetitions, List<PerformedSet> Sets);
public sealed record WorkoutSession(string Id, string StudentId, string PlanId, int PlanVersion, string PlanName, DateTimeOffset StartedAt, int DurationSeconds, string Notes, List<WorkoutExercise> Exercises);
public sealed record WorkoutSummary(string Id, string PlanName, DateTimeOffset StartedAt, int DurationSeconds, int ExerciseCount, int SetCount);
public sealed record WorkoutResult(string Status, WorkoutSession? Session = null);

public sealed class WorkoutService(MySqlDatabase database)
{
    public async Task<List<WorkoutSummary>> ListAsync(string student, CancellationToken ct)
    {
        await using var connection = await database.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT s.id,v.name,s.started_at,s.duration_seconds,COUNT(DISTINCT x.exercise_id),COUNT(*)
            FROM workout_sessions s
            JOIN workout_plan_versions v ON v.plan_id=s.plan_id AND v.version=s.plan_version
            JOIN workout_session_sets x ON x.session_id=s.id
            WHERE s.student_id=@student
            GROUP BY s.id,v.name,s.started_at,s.duration_seconds
            ORDER BY s.started_at DESC,s.id
            """;
        command.Parameters.AddWithValue("@student", student);
        var sessions = new List<WorkoutSummary>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            sessions.Add(new(reader.GetGuid(0).ToString(), reader.GetString(1), Utc(reader.GetDateTime(2)), reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5)));
        return sessions;
    }

    public async Task<WorkoutSession?> GetAsync(string student, string id, CancellationToken ct)
    {
        await using var connection = await database.OpenConnectionAsync(ct);
        return await Read(connection, null, student, id, ct);
    }
    private static DateTimeOffset Utc(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    private static async Task<WorkoutSession?> Read(MySqlConnection connection, MySqlTransaction? transaction, string student, string id, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT s.id,s.student_id,s.plan_id,s.plan_version,v.name,s.started_at,s.duration_seconds,s.notes,
                   i.exercise_id,i.name,i.muscle_group,i.equipment,i.description,i.set_count,i.repetitions,x.repetitions,x.weight_kg
            FROM workout_sessions s
            JOIN workout_plan_versions v ON v.plan_id=s.plan_id AND v.version=s.plan_version
            JOIN workout_session_sets x ON x.session_id=s.id
            JOIN workout_plan_items i ON i.plan_id=s.plan_id AND i.version=s.plan_version AND i.exercise_id=x.exercise_id
            WHERE s.student_id=@student AND s.id=@id
            ORDER BY i.position,x.set_position
            """;
        command.Parameters.AddWithValue("@student", student);
        command.Parameters.AddWithValue("@id", id);
        WorkoutSession? session = null;
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            session ??= new(reader.GetGuid(0).ToString(), reader.GetGuid(1).ToString(), reader.GetGuid(2).ToString(), reader.GetInt32(3), reader.GetString(4), Utc(reader.GetDateTime(5)), reader.GetInt32(6), reader.GetString(7), []);
            var exerciseId = reader.GetGuid(8).ToString();
            var exercise = session.Exercises.LastOrDefault();
            if (exercise?.ExerciseId != exerciseId)
            {
                exercise = new(exerciseId, reader.GetString(9), reader.GetString(10), reader.GetString(11), reader.GetString(12), reader.GetInt32(13), reader.GetInt32(14), []);
                session.Exercises.Add(exercise);
            }
            exercise.Sets.Add(new(reader.GetInt32(15), reader.GetDecimal(16)));
        }
        return session;
    }

    // Canonicalização: ordem do corpo, fuso e casas decimais equivalentes não criam outro registro.
    private static string Fingerprint(CreateWorkoutRequest request) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
    {
        request.PlanId, request.PlanVersion, StartedAt = request.StartedAt.ToUnixTimeMilliseconds(), request.DurationSeconds,
        Notes = request.Notes?.Trim() ?? string.Empty,
        Exercises = request.Exercises!.OrderBy(e => e.ExerciseId).Select(e => new
        {
            e.ExerciseId, Sets = e.Sets!.Select(s => new { s.Repetitions, WeightKg = s.WeightKg.ToString("0.##", CultureInfo.InvariantCulture) })
        })
    })));

    public async Task<WorkoutResult> CreateAsync(string student, CreateWorkoutRequest request, CancellationToken ct)
    {
        var id = request.Id.ToString();
        var fingerprint = Fingerprint(request);
        await using var connection = await database.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        // Mesmo bloqueio das fichas: serializa criação, edição da ficha e reenvios desta conta.
        command.CommandText = "SELECT user_id FROM account_roles WHERE user_id=@student AND role='student' FOR UPDATE";
        command.Parameters.AddWithValue("@student", student);
        if (await command.ExecuteScalarAsync(ct) is null) return new("missing");
        command.Parameters.AddWithValue("@id", id);
        command.CommandText = "SELECT student_id,request_hash FROM workout_sessions WHERE id=@id";
        bool exists;
        bool sameRequest;
        await using (var reader = await command.ExecuteReaderAsync(ct))
        {
            exists = await reader.ReadAsync(ct);
            sameRequest = exists && reader.GetGuid(0).ToString() == student && reader.GetString(1) == fingerprint;
        }
        if (exists) return sameRequest ? new("existing", await Read(connection, transaction, student, id, ct)) : new("conflict");
        command.CommandText = """
            SELECT i.exercise_id FROM workout_plans p
            JOIN workout_plan_items i ON i.plan_id=p.id AND i.version=@version
            WHERE p.id=@plan AND p.student_id=@student
            """;
        command.Parameters.AddWithValue("@plan", request.PlanId.ToString());
        command.Parameters.AddWithValue("@version", request.PlanVersion);
        var allowed = new HashSet<Guid>();
        await using (var reader = await command.ExecuteReaderAsync(ct))
            while (await reader.ReadAsync(ct)) allowed.Add(reader.GetGuid(0));
        if (allowed.Count == 0) return new("missing");
        if (request.Exercises!.Any(item => !allowed.Contains(item.ExerciseId))) return new("exercise_invalid");
        command.CommandText = """
            INSERT INTO workout_sessions (id,student_id,plan_id,plan_version,started_at,duration_seconds,notes,request_hash)
            VALUES (@id,@student,@plan,@version,@started,@duration,@notes,@hash)
            """;
        command.Parameters.AddWithValue("@started", DateTimeOffset.FromUnixTimeMilliseconds(request.StartedAt.ToUnixTimeMilliseconds()).UtcDateTime);
        command.Parameters.AddWithValue("@duration", request.DurationSeconds);
        command.Parameters.AddWithValue("@notes", request.Notes?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("@hash", fingerprint);
        try { await command.ExecuteNonQueryAsync(ct); }
        catch (MySqlException exception) when (exception.Number == 1062) { return new("conflict"); }
        foreach (var exercise in request.Exercises!)
            for (var index = 0; index < exercise.Sets!.Length; index++)
            {
                var set = exercise.Sets[index];
                await using var insert = connection.CreateCommand();
                insert.Transaction = transaction;
                insert.CommandText = """
                    INSERT INTO workout_session_sets (session_id,plan_id,plan_version,exercise_id,set_position,repetitions,weight_kg)
                    VALUES (@id,@plan,@version,@exercise,@position,@repetitions,@weight)
                    """;
                insert.Parameters.AddWithValue("@id", id);
                insert.Parameters.AddWithValue("@plan", request.PlanId.ToString());
                insert.Parameters.AddWithValue("@version", request.PlanVersion);
                insert.Parameters.AddWithValue("@exercise", exercise.ExerciseId.ToString());
                insert.Parameters.AddWithValue("@position", index);
                insert.Parameters.AddWithValue("@repetitions", set.Repetitions);
                insert.Parameters.AddWithValue("@weight", set.WeightKg);
                await insert.ExecuteNonQueryAsync(ct);
            }
        var session = await Read(connection, transaction, student, id, ct);
        await transaction.CommitAsync(ct);
        return new("created", session);
    }
}
