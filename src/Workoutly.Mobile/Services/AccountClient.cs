using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Workoutly.Mobile.Services;

public sealed record AccountProfile(string Id, string Name, string Email);
public sealed record AccountSession(string AccessToken, DateTimeOffset ExpiresAt, AccountProfile User);
public sealed record LoginPayload(string Email, string Password);
public sealed record RegisterPayload(string Name, string Email, string Password);
public sealed record StoredSession(string ApiUrl, string Token);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AccountProfile))]
[JsonSerializable(typeof(AccountSession))]
[JsonSerializable(typeof(LoginPayload))]
[JsonSerializable(typeof(RegisterPayload))]
[JsonSerializable(typeof(StoredSession))]
internal partial class AccountJson : JsonSerializerContext;

public sealed class AccountClient
{
    private const string SessionKey = "workoutly.account.session";
    private readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(10) };
    private string? _token;
    public string ApiUrl { get; private set; } = Preferences.Default.Get("workoutly.api_url", DefaultApiUrl);

    private static string DefaultApiUrl
    {
        get
        {
#if DEBUG
            return DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5080/" : "http://localhost:5080/";
#else
            return string.Empty;
#endif
        }
    }

    public void ConfigureServer(string address)
    {
        if (!Uri.TryCreate(address.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && uri.Scheme != "http") || !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new AccountException("Informe um endereço válido para o serviço.");
#if !DEBUG
        if (uri.Scheme != "https")
            throw new AccountException("A conexão com sua conta precisa usar HTTPS.");
#endif
        var normalized = uri.AbsoluteUri.TrimEnd('/') + "/";
        if (normalized != ApiUrl)
            ClearSession();
        ApiUrl = normalized;
        Preferences.Default.Set("workoutly.api_url", normalized);
    }

    public async Task<AccountProfile?> RestoreAsync()
    {
        if (!string.IsNullOrWhiteSpace(ApiUrl)) ConfigureServer(ApiUrl);
        StoredSession? stored;
        try
        {
            var value = await SecureStorage.Default.GetAsync(SessionKey);
            stored = value is null ? null : JsonSerializer.Deserialize(value, AccountJson.Default.StoredSession);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            ClearSession();
            throw new AccountException("Não foi possível recuperar a sessão protegida. Entre novamente.");
        }
        if (stored is null || stored.ApiUrl != ApiUrl)
            return null;
        _token = stored.Token;
        try { return await GetProfileAsync(); }
        catch (SessionExpiredException) { return null; }
    }

    public async Task<AccountProfile> LoginAsync(string email, string password)
    {
        using var content = JsonContent.Create(new LoginPayload(email.Trim(), password), AccountJson.Default.LoginPayload);
        var session = await SendAsync(HttpMethod.Post, "auth/login", content, AccountJson.Default.AccountSession, false);
        return await StoreSessionAsync(session);
    }

    public async Task<AccountProfile> RegisterAsync(string name, string email, string password)
    {
        using var content = JsonContent.Create(new RegisterPayload(name.Trim(), email.Trim(), password), AccountJson.Default.RegisterPayload);
        var session = await SendAsync(HttpMethod.Post, "auth/register", content, AccountJson.Default.AccountSession, false);
        return await StoreSessionAsync(session);
    }

    public Task<AccountProfile> GetProfileAsync() => SendAsync(HttpMethod.Get, "auth/me", null, AccountJson.Default.AccountProfile, true);

    public async Task LogoutAsync()
    {
        try
        {
            using var response = await RequestAsync(HttpMethod.Post, "auth/logout", null, true);
        }
        catch (SessionExpiredException) { }
        ClearSession();
    }

    private async Task<AccountProfile> StoreSessionAsync(AccountSession session)
    {
        _token = session.AccessToken;
        try
        {
            var value = JsonSerializer.Serialize(new StoredSession(ApiUrl, session.AccessToken), AccountJson.Default.StoredSession);
            await SecureStorage.Default.SetAsync(SessionKey, value);
            return session.User;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            try { await LogoutAsync(); } catch (AccountException) { ClearSession(); }
            throw new AccountException("A conta foi validada, mas não foi possível guardar a sessão protegida. Tente entrar novamente.");
        }
    }

    private void ClearSession()
    {
        _token = null;
        try { SecureStorage.Default.Remove(SessionKey); }
        catch (Exception exception) when (exception is not OutOfMemoryException) { }
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, HttpContent? content, JsonTypeInfo<T> type, bool authenticated)
    {
        using var response = await RequestAsync(method, path, content, authenticated);
        try
        {
            return await response.Content.ReadFromJsonAsync(type) ?? throw new AccountException("O serviço enviou uma resposta vazia.");
        }
        catch (JsonException) { throw new AccountException("Não foi possível compreender a resposta do serviço."); }
    }

    private async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string path, HttpContent? content, bool authenticated)
    {
        if (string.IsNullOrWhiteSpace(ApiUrl))
            throw new AccountException("Configure o endereço do serviço em Conexão.");
        using var request = new HttpRequestMessage(method, new Uri(new Uri(ApiUrl), path)) { Content = content };
        if (authenticated && _token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        HttpResponseMessage response;
        try { response = await _http.SendAsync(request); }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw new AccountException("Não foi possível conectar ao serviço. Confira sua conexão e tente novamente. Seus campos foram preservados.");
        }
        if (response.IsSuccessStatusCode)
            return response;
        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (authenticated) { ClearSession(); throw new SessionExpiredException(); }
                throw new AccountException("E-mail ou senha inválidos.");
            }
            if (response.StatusCode == HttpStatusCode.Conflict)
                throw new AccountException("Este e-mail já está cadastrado. Use Entrar.");
            if ((int)response.StatusCode == 429)
                throw new AccountException("Muitas tentativas. Aguarde um minuto e tente novamente.");
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                try
                {
                    using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    if (body.RootElement.TryGetProperty("errors", out var errors))
                        throw new AccountException(string.Join("\n", errors.EnumerateObject().SelectMany(field => field.Value.EnumerateArray()).Select(value => value.GetString())));
                }
                catch (JsonException) { }
                throw new AccountException("Revise os dados informados e o endereço do serviço.");
            }
            throw new AccountException("Serviço indisponível. Tente novamente em instantes.");
        }
    }
}

public class AccountException(string message) : Exception(message);
public sealed class SessionExpiredException() : AccountException("Sua sessão expirou. Entre novamente.");
