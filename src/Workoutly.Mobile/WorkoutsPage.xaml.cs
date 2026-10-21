using Workoutly.Mobile.Services;
namespace Workoutly.Mobile;

public partial class WorkoutsPage : ContentPage
{
    private readonly AccountClient _accounts;
    private bool _busy;
    public WorkoutsPage(AccountClient accounts) { InitializeComponent(); _accounts = accounts; }
    protected override async void OnAppearing() { base.OnAppearing(); await LoadAsync(); }
    private async Task LoadAsync()
    {
        if (_busy) return;
        SetBusy(true); ErrorText.IsVisible = false; WorkoutsList.ItemsSource = null;
        try { WorkoutsList.ItemsSource = await _accounts.GetWorkoutsAsync(); }
        catch (SessionExpiredException exception) { await DisplayAlertAsync("Acesso", exception.Message, "OK"); await Navigation.PopToRootAsync(); }
        catch (AccountException exception) { ErrorText.Text = exception.Message; ErrorText.IsVisible = true; }
        finally { SetBusy(false); }
    }
    private async void OnRefresh(object? sender, EventArgs args) => await LoadAsync();
    private async void OnOpen(object? sender, EventArgs args)
    {
        if (_busy || sender is not Button { CommandParameter: string id }) return;
        SetBusy(true);
        try { await Navigation.PushAsync(new WorkoutDetailsPage(_accounts, id)); }
        finally { SetBusy(false); }
    }
    private void SetBusy(bool busy)
    {
        _busy = busy; Loading.IsVisible = Loading.IsRunning = busy;
        RefreshButton.IsEnabled = WorkoutsList.IsEnabled = !busy;
    }
}
