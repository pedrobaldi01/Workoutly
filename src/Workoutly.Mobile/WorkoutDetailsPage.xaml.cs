using Workoutly.Mobile.Services;
namespace Workoutly.Mobile;

public partial class WorkoutDetailsPage : ContentPage
{
    private readonly AccountClient _accounts;
    private readonly string _id;
    private bool _busy;
    public WorkoutDetailsPage(AccountClient accounts, string id) { InitializeComponent(); _accounts = accounts; _id = id; }
    protected override async void OnAppearing() { base.OnAppearing(); await LoadAsync(); }
    private async Task LoadAsync()
    {
        if (_busy) return;
        _busy = true; RefreshButton.IsEnabled = false; Loading.IsVisible = Loading.IsRunning = true;
        ErrorText.IsVisible = DetailPanel.IsVisible = false;
        try
        {
            var session = await _accounts.GetWorkoutAsync(_id);
            Heading.Text = session.PlanName;
            DateText.Text = $"{session.StartedAt.ToLocalTime():dd/MM/yyyy 'às' HH:mm} • ficha na versão {session.PlanVersion}";
            SummaryText.Text = $"{session.Exercises.Sum(e => e.Sets.Length)} série(s) realizadas • {WorkoutSummaryItem.FormatDuration(session.DurationSeconds)}";
            NotesText.Text = string.IsNullOrEmpty(session.Notes) ? "Sem observações neste treino." : session.Notes;
            ExercisesPanel.Children.Clear();
            foreach (var exercise in session.Exercises)
            {
                var content = new VerticalStackLayout { Spacing = 10 };
                content.Children.Add(new Label { Text = exercise.Name, FontFamily = "InterSemiBold", FontSize = 19 });
                content.Children.Add(new Label { Text = $"{exercise.MuscleGroup} • {exercise.Equipment}", Style = (Style)Application.Current!.Resources["SecondaryText"] });
                for (var index = 0; index < exercise.Sets.Length; index++)
                {
                    var set = exercise.Sets[index];
                    content.Children.Add(new Label { Text = $"Série {index + 1:00}   •   {set.Repetitions} repetições   •   {set.WeightKg:0.##} kg" });
                }
                ExercisesPanel.Children.Add(new Border { Style = (Style)Application.Current!.Resources["SurfaceCard"], Content = content });
            }
            DetailPanel.IsVisible = true;
        }
        catch (SessionExpiredException exception) { await DisplayAlertAsync("Acesso", exception.Message, "OK"); await Navigation.PopToRootAsync(); }
        catch (AccountException exception) { ErrorText.Text = exception.Message; ErrorText.IsVisible = true; }
        finally { _busy = false; RefreshButton.IsEnabled = true; Loading.IsVisible = Loading.IsRunning = false; }
    }
    private async void OnRefresh(object? sender, EventArgs args) => await LoadAsync();
}
