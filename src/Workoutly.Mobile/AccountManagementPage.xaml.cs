using System.Globalization;
using System.Net;
using Workoutly.Mobile.Services;

namespace Workoutly.Mobile;

public partial class AccountManagementPage : ContentPage, IConfirmLeave
{
    private enum AccountAction { None, Profile, StudentDetails, Password, Delete }

    private readonly AccountClient _accounts;
    private AccountProfile _profile;
    private StudentProfile? _studentProfile;
    private AccountAction _action;
    private bool _busy;
    private bool _loadingStudent;

    public AccountManagementPage(AccountClient accounts, AccountProfile profile)
    {
        InitializeComponent();
        _accounts = accounts;
        _profile = profile;
        StudentCard.IsVisible = profile.Role == "student";
        if (StudentCard.IsVisible) RenderStudentProfile();
        FormFeedback.FollowError(PageScroll, ErrorText);
        FormFeedback.FollowError(PageScroll, EmailError);
    }

    private bool EmailChanged => !string.Equals(EmailEntry.Text?.Trim(), _profile.Email, StringComparison.OrdinalIgnoreCase);

    private void OnProfile(object? sender, EventArgs args) => ShowForm(AccountAction.Profile);
    private void OnPassword(object? sender, EventArgs args) => ShowForm(AccountAction.Password);
    private void OnDelete(object? sender, EventArgs args) => ShowForm(AccountAction.Delete);

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (StudentCard.IsVisible && _studentProfile is null)
            await LoadStudentProfileAsync();
    }

    private async void OnStudentProfile(object? sender, EventArgs args)
    {
        if (_busy || _loadingStudent) return;
        if (_studentProfile is null) await LoadStudentProfileAsync();
        if (_studentProfile is not { } student || _action != AccountAction.None) return;
        StudentFirstNameEntry.Text = student.FirstName;
        StudentLastNameEntry.Text = student.LastName;
        StudentWeightEntry.Text = FormatWeight(student.BodyWeightKg);
        StudentHeightEntry.Text = student.HeightCm?.ToString(CultureInfo.CurrentCulture);
        ShowForm(AccountAction.StudentDetails);
    }

    private async Task LoadStudentProfileAsync()
    {
        if (_loadingStudent) return;
        _loadingStudent = true;
        EditStudentButton.IsEnabled = false;
        StudentStatus.Text = "Carregando perfil…";
        StudentStatus.IsVisible = true;
        try
        {
            _studentProfile = await _accounts.GetStudentProfileAsync();
            RenderStudentProfile();
            StudentStatus.IsVisible = false;
        }
        catch (AccountException exception) { StudentStatus.Text = exception.Message; }
        finally
        {
            EditStudentButton.IsEnabled = true;
            _loadingStudent = false;
        }
    }

    private void RenderStudentProfile()
    {
        StudentName.Text = _studentProfile?.Name ?? _profile.Name;
        StudentEmail.Text = _profile.Email;
        StudentWeight.IsVisible = StudentHeight.IsVisible = _studentProfile is not null;
        StudentWeight.Text = _studentProfile?.BodyWeightKg is decimal weight ? $"Peso atual: {FormatWeight(weight)} kg" : "Peso atual: a preencher";
        StudentHeight.Text = _studentProfile?.HeightCm is int height ? $"Altura: {height} cm" : "Altura: a preencher";
    }

    private static string? FormatWeight(decimal? weight) => weight?.ToString("0.##", CultureInfo.CurrentCulture);

    private void ShowForm(AccountAction action)
    {
        _action = action;
        SuccessText.IsVisible = ErrorText.IsVisible = EmailError.IsVisible = false;
        Actions.IsVisible = false;
        Form.IsVisible = true;
        ProfileFields.IsVisible = action == AccountAction.Profile;
        StudentFields.IsVisible = action == AccountAction.StudentDetails;
        NewPasswordFields.IsVisible = action == AccountAction.Password;
        NameEntry.Text = _profile.Name;
        EmailEntry.Text = _profile.Email;
        CurrentPasswordField.IsVisible = action is AccountAction.Password or AccountAction.Delete;
        (FormHeading.Text, Hint.Text, SubmitButton.Text) = action switch
        {
            AccountAction.Profile => ("Editar nome e e-mail", "O novo e-mail será usado no próximo login.", "Salvar alterações"),
            AccountAction.StudentDetails => ("Editar perfil", "Você pode atualizar seu peso e sua altura quando quiser.", "Salvar perfil"),
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
                if (StudentCard.IsVisible)
                {
                    if (_studentProfile is { } student && student.Name != _profile.Name)
                        _studentProfile = student with { Name = _profile.Name, FirstName = _profile.Name, LastName = null };
                    RenderStudentProfile();
                }
                CloseForm("Dados da conta atualizados.");
                return;
            }

            if (_action == AccountAction.StudentDetails)
            {
                _studentProfile = await _accounts.UpdateStudentProfileAsync(ReadStudentProfile());
                _profile = _profile with { Name = _studentProfile.Name };
                RenderStudentProfile();
                CloseForm("Perfil atualizado.");
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
            _action == AccountAction.StudentDetails && _studentProfile is { } student &&
                ((StudentFirstNameEntry.Text ?? "") != (student.FirstName ?? "") || (StudentLastNameEntry.Text ?? "") != (student.LastName ?? "") ||
                 (StudentWeightEntry.Text ?? "") != (FormatWeight(student.BodyWeightKg) ?? "") ||
                 (StudentHeightEntry.Text ?? "") != (student.HeightCm?.ToString(CultureInfo.CurrentCulture) ?? "")) ||
            !string.IsNullOrEmpty(CurrentPasswordEntry.Text) || !string.IsNullOrEmpty(NewPasswordEntry.Text) ||
            !string.IsNullOrEmpty(ConfirmPasswordEntry.Text);
        return !hasChanges || await DisplayAlertAsync("Descartar alterações?", "Os dados que você preencheu nesta tela serão descartados.", "Descartar", "Continuar editando");
    }

    private StudentProfilePayload ReadStudentProfile()
    {
        decimal? weight = null;
        if (!string.IsNullOrWhiteSpace(StudentWeightEntry.Text))
        {
            if (!decimal.TryParse(StudentWeightEntry.Text.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed)
                || parsed is <= 0 or > 1000 || decimal.Round(parsed, 2) != parsed)
                throw new AccountException("Informe um peso maior que 0 até 1000 kg, com até duas casas decimais.");
            weight = parsed;
        }

        int? height = null;
        if (!string.IsNullOrWhiteSpace(StudentHeightEntry.Text))
        {
            if (!int.TryParse(StudentHeightEntry.Text, out var parsed) || parsed is < 1 or > 300)
                throw new AccountException("Informe a altura em centímetros inteiros, de 1 a 300.");
            height = parsed;
        }
        return new(StudentFirstNameEntry.Text ?? "", StudentLastNameEntry.Text, weight, height);
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
