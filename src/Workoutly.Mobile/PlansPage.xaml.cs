using Workoutly.Mobile.Services;
namespace Workoutly.Mobile;

public partial class PlansPage : ContentPage
{
    private readonly AccountClient _accounts;
    private readonly string? _studentId;
    private bool _busy;
    public PlansPage(AccountClient accounts, string? studentId = null, string? studentName = null)
    {
        InitializeComponent();
        _accounts = accounts;
        _studentId = studentId;
        if (studentId is not null)
        {
            Heading.Text = "Fichas do aluno";
            Subtitle.Text = studentName;
        }
    }
    protected override async void OnAppearing() { base.OnAppearing(); await LoadAsync(); }
    private async Task LoadAsync()
    {
        if (_busy) return;
        SetBusy(true);
        ErrorText.IsVisible = false;
        // Não manter dados de um aluno visíveis após perder o vínculo.
        PlansList.ItemsSource = null;
        try { PlansList.ItemsSource = await _accounts.GetPlansAsync(_studentId); }
        catch (SessionExpiredException exception)
        {
            await DisplayAlertAsync("Acesso", exception.Message, "OK");
            await Navigation.PopToRootAsync();
        }
        catch (AccountException exception) { ErrorText.Text = exception.Message; ErrorText.IsVisible = true; }
        finally { SetBusy(false); }
    }
    private async void OnRefresh(object? sender, EventArgs args) => await LoadAsync();
    private async void OnCreate(object? sender, EventArgs args) => await OpenAsync(null);
    private async void OnOpen(object? sender, EventArgs args)
    {
        if (sender is Button { CommandParameter: PlanItem plan }) await OpenAsync(plan);
    }
    private async Task OpenAsync(PlanItem? plan)
    {
        if (_busy) return;
        SetBusy(true);
        try { await Navigation.PushAsync(new PlanPage(_accounts, _studentId, plan)); }
        finally { SetBusy(false); }
    }
    private void SetBusy(bool busy)
    {
        _busy = busy;
        Loading.IsVisible = Loading.IsRunning = busy;
        CreateButton.IsEnabled = RefreshButton.IsEnabled = PlansList.IsEnabled = !busy;
    }
}
