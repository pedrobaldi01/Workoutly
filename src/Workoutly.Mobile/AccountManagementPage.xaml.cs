using System.Net;
using Workoutly.Mobile.Services;

namespace Workoutly.Mobile;

public partial class AccountManagementPage : ContentPage, IConfirmLeave
{
    private enum AccountAction { None, Profile, Password, Delete }

    private readonly AccountClient _accounts;
    private AccountProfile _profile;
    private AccountAction _action;
    private bool _busy;

    public AccountManagementPage(AccountClient accounts, AccountProfile profile)
    {
        InitializeComponent();
        _accounts = accounts;
        _profile = profile;
        FormFeedback.FollowError(PageScroll, ErrorText);
        FormFeedback.FollowError(PageScroll, EmailError);
    }

    private bool EmailChanged => !string.Equals(EmailEntry.Text?.Trim(), _profile.Email, StringComparison.OrdinalIgnoreCase);

    private void OnProfile(object? sender, EventArgs args) => ShowForm(AccountAction.Profile);
    private void OnPassword(object? sender, EventArgs args) => ShowForm(AccountAction.Password);
    private void OnDelete(object? sender, EventArgs args) => ShowForm(AccountAction.Delete);

    private void ShowForm(AccountAction action)
    {
        _action = action;
        SuccessText.IsVisible = ErrorText.IsVisible = EmailError.IsVisible = false;
        Actions.IsVisible = false;
        Form.IsVisible = true;
        ProfileFields.IsVisible = action == AccountAction.Profile;
        NewPasswordFields.IsVisible = action == AccountAction.Password;
        NameEntry.Text = _profile.Name;
        EmailEntry.Text = _profile.Email;
        CurrentPasswordField.IsVisible = action != AccountAction.Profile;
        (FormHeading.Text, Hint.Text, SubmitButton.Text) = action switch
        {
            AccountAction.Profile => ("Editar nome e e-mail", "O novo e-mail será usado no próximo login.", "Salvar alterações"),
            AccountAction.Password => ("Alterar senha", "Use de 10 a 128 caracteres. Você precisará entrar novamente em todos os aparelhos.", "Salvar nova senha"),
            _ => ("Excluir conta", _profile.Role == "teacher"
                ? "Sua conta e seus vínculos serão excluídos. As fichas e os treinos dos alunos serão preservados."
                : "Sua conta, fichas, treinos e histórico serão excluídos. Seu vínculo com o professor será removido.", "Revisar exclusão")
        };
    }

    private void OnEmailChanged(object? sender, TextChangedEventArgs args)
    {
        if (_action != AccountAction.Profile) return;
        EmailError.IsVisible = false;
        CurrentPasswordField.IsVisible = EmailChanged;
        if (!EmailChanged) CurrentPasswordEntry.Text = string.Empty;
    }

    private async void OnCancel(object? sender, EventArgs args)
    {
        if (await CanLeaveAsync()) CloseForm();
    }

    private async void OnSubmit(object? sender, EventArgs args)
    {
        if (_busy) return;
        ErrorText.IsVisible = EmailError.IsVisible = false;
        var currentPassword = CurrentPasswordEntry.Text ?? string.Empty;
        var newPassword = NewPasswordEntry.Text ?? string.Empty;
        var finished = false;
        _busy = true;
        Form.IsEnabled = false;
        try
        {
            if (_action == AccountAction.Profile)
            {
                _profile = await _accounts.UpdateProfileAsync(NameEntry.Text ?? string.Empty, EmailEntry.Text ?? string.Empty,
                    EmailChanged ? currentPassword : null);
                CloseForm("Dados da conta atualizados.");
                return;
            }

            if (_action == AccountAction.Password)
            {
                if (newPassword != (ConfirmPasswordEntry.Text ?? string.Empty))
                {
                    ShowError("A confirmação da nova senha precisa ser igual à senha digitada.");
                    return;
                }
                await _accounts.ChangePasswordAsync(currentPassword, newPassword);
                CloseForm();
                finished = true;
                await DisplayAlertAsync("Senha alterada", "Entre novamente com sua nova senha.", "OK");
            }
            else if (_action == AccountAction.Delete)
            {
                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    ShowError("Informe sua senha atual.");
                    return;
                }
                if (!await DisplayAlertAsync("Excluir conta definitivamente?", Hint.Text, "Excluir conta", "Cancelar")) return;
                await _accounts.DeleteAccountAsync(currentPassword);
                CloseForm();
                finished = true;
                await DisplayAlertAsync("Conta excluída", "Sua conta foi excluída.", "OK");
            }
        }
        catch (SessionExpiredException)
        {
            CloseForm();
            finished = true;
            await DisplayAlertAsync("Sessão encerrada", "Entre novamente para continuar.", "OK");
        }
        catch (AccountException exception) when (_action == AccountAction.Profile && exception.StatusCode == HttpStatusCode.Conflict)
        {
            EmailError.Text = exception.Message;
            EmailError.IsVisible = true;
        }
        catch (AccountException exception) { ShowError(exception.Message); }
        finally
        {
            Form.IsEnabled = true;
            _busy = false;
        }
        if (finished) await Navigation.PopAsync();
    }

    public async Task<bool> CanLeaveAsync()
    {
        if (_busy) return false;
        var hasChanges = _action == AccountAction.Profile && (NameEntry.Text != _profile.Name || EmailChanged) ||
            !string.IsNullOrEmpty(CurrentPasswordEntry.Text) || !string.IsNullOrEmpty(NewPasswordEntry.Text) ||
            !string.IsNullOrEmpty(ConfirmPasswordEntry.Text);
        return !hasChanges || await DisplayAlertAsync("Descartar alterações?", "Os dados que você preencheu nesta tela serão descartados.", "Descartar", "Continuar editando");
    }

    private void CloseForm(string? message = null)
    {
        _action = AccountAction.None;
        CurrentPasswordEntry.Text = NewPasswordEntry.Text = ConfirmPasswordEntry.Text = string.Empty;
        Form.IsVisible = false;
        Actions.IsVisible = true;
        SuccessText.Text = message;
        SuccessText.IsVisible = message is not null;
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.IsVisible = true;
    }
}
