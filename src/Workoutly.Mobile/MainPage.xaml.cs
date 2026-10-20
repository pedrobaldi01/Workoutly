using Workoutly.Mobile.Services;

namespace Workoutly.Mobile;

public partial class MainPage : ContentPage
{
    private readonly AccountClient _accounts = new();
    private bool _registering;
    private bool _busy;

    public MainPage()
    {
        InitializeComponent();
        AddressEntry.Text = _accounts.ApiUrl;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_busy) return;
        SetBusy(true);
        try
        {
            var profile = await _accounts.RestoreAsync();
            if (profile is not null) ShowAccount(profile);
            else ShowSignedOut();
        }
        catch (AccountException exception) { ShowError(exception.Message); }
        finally { SetBusy(false); }
    }

    private async void OnSubmit(object? sender, EventArgs args)
    {
        if (_busy) return;
        SetBusy(true);
        ErrorText.IsVisible = false;
        try
        {
            _accounts.ConfigureServer(AddressEntry.Text ?? string.Empty);
            var email = EmailEntry.Text ?? string.Empty;
            var password = PasswordEntry.Text ?? string.Empty;
            if (_registering && password != ConfirmEntry.Text)
                throw new AccountException("As senhas não coincidem.");
            var profile = _registering
                ? await _accounts.RegisterAsync(NameEntry.Text ?? string.Empty, email, password)
                : await _accounts.LoginAsync(email, password);
            PasswordEntry.Text = ConfirmEntry.Text = string.Empty;
            ShowAccount(profile);
        }
        catch (AccountException exception) { ShowError(exception.Message); }
        finally { SetBusy(false); }
    }

    private void OnToggle(object? sender, EventArgs args)
    {
        if (_busy) return;
        _registering = !_registering;
        NamePanel.IsVisible = ConfirmPanel.IsVisible = _registering;
        SubmitButton.Text = _registering ? "Criar conta" : "Entrar";
        ToggleButton.Text = _registering ? "Já tenho conta" : "Criar uma conta";
        Subtitle.Text = _registering ? "Crie sua conta para organizar sua rotina." : "Entre na sua conta para começar.";
        PasswordEntry.Text = ConfirmEntry.Text = string.Empty;
        ErrorText.IsVisible = false;
    }

    private void OnConnection(object? sender, EventArgs args) => ConnectionPanel.IsVisible = !ConnectionPanel.IsVisible;

    private async void OnLogout(object? sender, EventArgs args)
    {
        if (_busy) return;
        SetBusy(true);
        ErrorText.IsVisible = false;
        try
        {
            await _accounts.LogoutAsync();
            ShowSignedOut();
        }
        catch (AccountException exception) { ShowError(exception.Message); }
        finally { SetBusy(false); }
    }

    private void ShowAccount(AccountProfile profile)
    {
        AccountName.Text = profile.Name;
        AccountEmail.Text = profile.Email;
        AuthPanel.IsVisible = false;
        AccountPanel.IsVisible = true;
        Heading.Text = $"Olá, {profile.Name}!";
        Subtitle.Text = "Sua conta está conectada.";
        ErrorText.IsVisible = false;
    }

    private async void OnExercises(object? sender, EventArgs args)
    {
        if (_busy) return;
        await Navigation.PushAsync(new ExercisesPage(_accounts));
    }

    private void ShowSignedOut()
    {
        AccountPanel.IsVisible = false;
        AuthPanel.IsVisible = true;
        Heading.Text = "Seu treino, no seu ritmo.";
        Subtitle.Text = _registering ? "Crie sua conta para organizar sua rotina." : "Entre na sua conta para começar.";
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.IsVisible = true;
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        Loading.IsVisible = Loading.IsRunning = busy;
        AuthPanel.IsEnabled = !busy;
        LogoutButton.IsEnabled = !busy;
        ExercisesButton.IsEnabled = !busy;
    }
}
