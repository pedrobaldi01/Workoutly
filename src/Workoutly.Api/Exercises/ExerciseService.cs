using MySqlConnector;
using Workoutly.Api.Data;

namespace Workoutly.Api.Exercises;

public sealed record Exercise(string Id, string Name, string MuscleGroup, string Equipment, string Description, bool IsShared);

public sealed class ExerciseService(MySqlDatabase database)
{
    public async Task<Exercise> CreateAsync(string ownerId, string name, string muscleGroup, string equipment, string? description, CancellationToken cancellationToken)
    {
        var exercise = new Exercise(Guid.NewGuid().ToString(), name.Trim(), muscleGroup.Trim(), equipment.Trim(), description?.Trim() ?? string.Empty, false);
        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO exercises (id, owner_id, name, muscle_group, equipment, description)
            VALUES (@id, @owner, @name, @muscle, @equipment, @description)
            """;
        command.Parameters.AddWithValue("@id", exercise.Id);
        command.Parameters.AddWithValue("@owner", ownerId);
        command.Parameters.AddWithValue("@name", exercise.Name);
        command.Parameters.AddWithValue("@muscle", exercise.MuscleGroup);
        command.Parameters.AddWithValue("@equipment", exercise.Equipment);
        command.Parameters.AddWithValue("@description", exercise.Description);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return exercise;
    }

    public async Task<List<Exercise>> ListAsync(string ownerId, string? id, CancellationToken cancellationToken)
    {
        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, name, muscle_group, equipment, description, owner_id IS NULL
            FROM exercises
            WHERE (owner_id IS NULL OR owner_id = @owner) AND (@id IS NULL OR id = @id)
            ORDER BY name, id
            """;
        command.Parameters.AddWithValue("@owner", ownerId);
        command.Parameters.AddWithValue("@id", (object?)id ?? DBNull.Value);
        var exercises = new List<Exercise>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            exercises.Add(new(reader.GetGuid(0).ToString(), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetString(4), reader.GetBoolean(5)));
        return exercises;
    }
}
