using Workoutly.Mobile.Services;

namespace Workoutly.Mobile;

public partial class MainPage : ContentPage
{
    private readonly AccountClient _accounts = new();
    private bool _registering;
    private bool _busy;
    private bool _isTeacher;

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
        UpdateAccessCopy();
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
        _isTeacher = profile.Role == "teacher";
        AccountRole.Text = _isTeacher ? "Professor / admin" : "Aluno";
        HomeTitle.Text = _isTeacher ? "Área do professor" : "Pronto para treinar?";
        ConnectionsTitle.Text = _isTeacher ? "Seus alunos" : "Seu acompanhamento";
        ConnectionsDescription.Text = _isTeacher ? "Compartilhe seu código para receber os vínculos dos alunos." : "Use o código do seu professor para se vincular a ele.";
        ConnectionsButton.Text = _isTeacher ? "Meus alunos" : "Meu professor";
        PlansTitle.Text = _isTeacher ? "Planeje para seus alunos" : "Suas fichas de treino";
        PlansDescription.Text = _isTeacher ? "Abra um aluno vinculado para montar ou ajustar suas fichas." : "Organize exercícios, séries e repetições para o próximo treino.";
        PlansButton.Text = _isTeacher ? "Montar fichas" : "Minhas fichas";
        Greeting.Text = $"Olá, {profile.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()}";
        Intro.IsVisible = false;
        AuthPanel.IsVisible = false;
        AccountPanel.IsVisible = true;
        ErrorText.IsVisible = false;
    }

    private async void OnExercises(object? sender, EventArgs args)
    {
        if (_busy) return;
        await Navigation.PushAsync(new ExercisesPage(_accounts));
    }

    private async void OnPlans(object? sender, EventArgs args)
    {
        if (_busy) return;
        await Navigation.PushAsync(_isTeacher ? new ConnectionsPage(_accounts, true) : new PlansPage(_accounts));
    }

    private void ShowSignedOut()
    {
        AccountPanel.IsVisible = false;
        AuthPanel.IsVisible = true;
        Intro.IsVisible = true;
        UpdateAccessCopy();
    }

    private async void OnConnections(object? sender, EventArgs args)
    {
        if (_busy) return;
        await Navigation.PushAsync(new ConnectionsPage(_accounts, _isTeacher));
    }

    private void UpdateAccessCopy()
    {
        Heading.Text = _registering ? "Seu próximo treino\ncomeça aqui." : "Treinos claros.\nProgresso visível.";
        Subtitle.Text = _registering ? "Crie sua conta para organizar suas fichas e acompanhar sua evolução." : "Organize suas fichas e acompanhe sua evolução em um só lugar.";
        ToggleButton.Text = _registering ? "Já tem conta?  Entre" : "Ainda não tem conta?  Cadastre-se";
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
        ConnectionButton.IsEnabled = !busy;
        ConnectionsButton.IsEnabled = !busy;
        PlansButton.IsEnabled = !busy;
    }
}
