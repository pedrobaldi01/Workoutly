using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Workoutly.Api.Auth;
using Workoutly.Api.Data;
using Workoutly.Api.Exercises;
using Workoutly.Api.Plans;
using Workoutly.Api.Teaching;
using Workoutly.Api.Workouts;

namespace Workoutly.Api.Tests;

public sealed class AccountManagementTests(MySqlFixture mysql) : IClassFixture<MySqlFixture>
{
    private const string CurrentPassword = "Treino2026!seguro";
    private const string NewPassword = "NovaSenha2026!";

    [MySqlFact, Trait("Category", "MySql")]
    public async Task Profile_changes_update_login_and_password_changes_revoke_all_sessions()
    {
        await using var app = new DatabaseApplication(mysql.ConnectionString);
        await app.Services.GetRequiredService<MySqlDatabase>().MigrateAsync();
        using var first = app.CreateClient();
        using var otherDevice = app.CreateClient();
        using var secondUser = app.CreateClient();
        var account = await Register(first, "student");
        var otherAccount = await Register(secondUser, "student");

        var nameOnly = await first.PatchAsJsonAsync("/auth/me", new { name = "Nome editado", email = account.Email });
        nameOnly.EnsureSuccessStatusCode();
        Assert.Equal("Nome editado", (await nameOnly.Content.ReadFromJsonAsync<AccountProfile>())?.Name);
        var email = $"novo-{Guid.NewGuid():N}@example.test";
        Assert.Equal(HttpStatusCode.BadRequest,
            (await first.PatchAsJsonAsync("/auth/me", new { name = "Nome editado", email })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,
            (await first.PatchAsJsonAsync("/auth/me", new { name = "Nome editado", email = otherAccount.Email.ToUpperInvariant(), currentPassword = CurrentPassword })).StatusCode);
        var updated = await first.PatchAsJsonAsync("/auth/me", new { name = "Nome editado", email = email.ToUpperInvariant(), currentPassword = CurrentPassword });
        updated.EnsureSuccessStatusCode();
        Assert.Equal(account.Id, (await updated.Content.ReadFromJsonAsync<AccountProfile>())?.Id);
        Assert.Equal(email, (await first.GetFromJsonAsync<AccountProfile>("/auth/me"))?.Email);
        Assert.Equal(HttpStatusCode.Unauthorized, (await otherDevice.PostAsJsonAsync("/auth/login", new { email = account.Email, password = CurrentPassword })).StatusCode);

        var login = await otherDevice.PostAsJsonAsync("/auth/login", new { email, password = CurrentPassword });
        login.EnsureSuccessStatusCode();
        otherDevice.DefaultRequestHeaders.Authorization = new("Bearer", (await login.Content.ReadFromJsonAsync<AccountSession>())!.AccessToken);
        Assert.Equal(HttpStatusCode.BadRequest, (await first.PostAsJsonAsync("/auth/me/password", new { currentPassword = "incorreta", newPassword = NewPassword })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await otherDevice.GetAsync("/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await first.PostAsJsonAsync("/auth/me/password", new { currentPassword = CurrentPassword, newPassword = NewPassword })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await first.GetAsync("/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await otherDevice.GetAsync("/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await secondUser.GetAsync("/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await first.PostAsJsonAsync("/auth/login", new { email, password = CurrentPassword })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await first.PostAsJsonAsync("/auth/login", new { email, password = NewPassword })).StatusCode);
    }

    [MySqlFact, Trait("Category", "MySql")]
    public async Task Teacher_deletion_preserves_student_records_without_publishing_private_exercises()
    {
        await using var app = new DatabaseApplication(mysql.ConnectionString);
        await app.Services.GetRequiredService<MySqlDatabase>().MigrateAsync();
        using var teacher = app.CreateClient();
        using var student = app.CreateClient();
        using var stranger = app.CreateClient();
        var teacherAccount = await Register(teacher, "teacher");
        var studentAccount = await Register(student, "student");
        await Register(stranger, "student");

        var invitation = await teacher.PostAsync("/teaching/invitation", null);
        invitation.EnsureSuccessStatusCode();
        var code = (await invitation.Content.ReadFromJsonAsync<TeacherInvitation>())!.Code;
        (await student.PostAsJsonAsync("/enrollment", new { code })).EnsureSuccessStatusCode();
        var created = await teacher.PostAsJsonAsync("/exercises", new { name = "Movimento privado", muscleGroup = "Costas", equipment = "Barra" });
        created.EnsureSuccessStatusCode();
        var exerciseId = (await created.Content.ReadFromJsonAsync<Exercise>())!.Id;
        var planId = Guid.NewGuid();
        string planPath = $"/plans/{planId}";
        var sessionId = Guid.NewGuid();
        var exercises = new[] { new { exerciseId, sets = 1, repetitions = 10 } };
        (await teacher.PostAsJsonAsync($"/teaching/students/{studentAccount.Id}/plans", new { id = planId, name = "Ficha recebida", exercises })).EnsureSuccessStatusCode();
        (await student.PostAsJsonAsync("/sessions", new
        {
            id = sessionId, planId, planVersion = 1, startedAt = DateTimeOffset.UtcNow.AddMinutes(-30), durationSeconds = 300,
            exercises = new[] { new { exerciseId, sets = new[] { new { repetitions = 10, weightKg = 20 } } } }
        })).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.BadRequest, (await teacher.PostAsJsonAsync("/auth/me/delete", new { currentPassword = "incorreta" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await teacher.PostAsJsonAsync("/auth/me/delete", new { currentPassword = CurrentPassword })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await teacher.GetAsync("/auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await teacher.PostAsJsonAsync("/auth/login", new { email = teacherAccount.Email, password = CurrentPassword })).StatusCode);
        Assert.Null((await student.GetFromJsonAsync<Enrollment>("/enrollment"))?.Teacher);
        Assert.Equal("Ficha recebida", (await student.GetFromJsonAsync<WorkoutPlan>(planPath))?.Name);
        var workout = await student.GetFromJsonAsync<WorkoutSession>($"/sessions/{sessionId}");
        Assert.NotNull(workout);
        Assert.Equal(1, workout.PlanVersion);
        Assert.Equal(200m, workout.VolumeKg);
        Assert.True((await student.GetFromJsonAsync<Exercise>($"/exercises/{exerciseId}"))!.IsInPlan);
        Assert.Equal(HttpStatusCode.NotFound, (await stranger.GetAsync($"/exercises/{exerciseId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await student.PutAsJsonAsync(planPath, new { name = "Ficha editada", version = 1, exercises })).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await student.PostAsJsonAsync("/auth/me/delete", new { currentPassword = CurrentPassword })).StatusCode);
        var newAccount = await Register(student, "student", studentAccount.Email);
        Assert.NotEqual(studentAccount.Id, newAccount.Id);
        Assert.Empty((await student.GetFromJsonAsync<WorkoutSession[]>("/sessions"))!);
        Assert.Empty((await student.GetFromJsonAsync<WorkoutPlan[]>("/plans"))!);
    }

    private static async Task<AccountProfile> Register(HttpClient client, string role, string? email = null)
    {
        email ??= $"{Guid.NewGuid():N}@example.test";
        var response = await client.PostAsJsonAsync("/auth/register", new { name = "Perfil de teste", email, password = CurrentPassword, role });
        response.EnsureSuccessStatusCode();
        var session = (await response.Content.ReadFromJsonAsync<AccountSession>())!;
        client.DefaultRequestHeaders.Authorization = new("Bearer", session.AccessToken);
        return session.User;
    }
}
