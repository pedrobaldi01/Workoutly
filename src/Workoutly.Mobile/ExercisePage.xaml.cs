using Workoutly.Mobile.Services;

namespace Workoutly.Mobile;

public partial class ExercisePage : ContentPage
{
    private readonly AccountClient _accounts;
    private bool _busy;
    private bool _saved;

    public ExercisePage(AccountClient accounts, ExerciseItem? exercise = null)
    {
        InitializeComponent();
        _accounts = accounts;
        if (exercise is null) return;
        Title = "Detalhes do exercício";
        Heading.Text = exercise.Name;
        OriginText.Text = exercise.Origin;
        NameEntry.Text = exercise.Name;
        GroupEntry.Text = exercise.MuscleGroup;
        EquipmentEntry.Text = exercise.Equipment;
        DescriptionEditor.Text = string.IsNullOrEmpty(exercise.Description) ? "Sem descrição." : exercise.Description;
        NameEntry.IsReadOnly = GroupEntry.IsReadOnly = EquipmentEntry.IsReadOnly = DescriptionEditor.IsReadOnly = true;
        SaveButton.IsVisible = false;
        PrivacyNote.IsVisible = !exercise.IsShared;
    }

    private async void OnSave(object? sender, EventArgs args)
    {
        if (_busy || _saved) return;
        SetBusy(true);
        ErrorText.IsVisible = false;
        try
        {
            await _accounts.CreateExerciseAsync(NameEntry.Text ?? string.Empty, GroupEntry.Text ?? string.Empty,
                EquipmentEntry.Text ?? string.Empty, DescriptionEditor.Text ?? string.Empty);
            _saved = true;
            await Navigation.PopAsync();
        }
        catch (SessionExpiredException exception)
        {
            await DisplayAlertAsync("Acesso", exception.Message, "OK");
            await Navigation.PopToRootAsync();
        }
        catch (AccountException exception)
        {
            ErrorText.Text = exception.Message;
            ErrorText.IsVisible = true;
        }
        finally { SetBusy(false); }
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        Loading.IsVisible = Loading.IsRunning = busy;
        Fields.IsEnabled = SaveButton.IsEnabled = !busy && !_saved;
    }
}
