using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;

namespace OrthoSpineAI.UI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IPatientService _patientService;
    private readonly IMedTestService _medTestService;

    [ObservableProperty] private int _totalPatients;
    [ObservableProperty] private int _testsToday;
    [ObservableProperty] private int _testsThisMonth;
    [ObservableProperty] private IReadOnlyList<RecentTestDto> _recentTests = [];
    [ObservableProperty] private bool _isBusy;

    public string CurrentUserName { get; }

    public event Action? PatientsRequested;
    public event Action<int>? ViewResultRequested;   // medTestId

    public DashboardViewModel(IPatientService patientService, IMedTestService medTestService, string userName)
    {
        _patientService = patientService;
        _medTestService = medTestService;
        CurrentUserName = userName;
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var patients = await _patientService.GetAllAsync();
            var dash = await _medTestService.GetDashboardAsync(patients.Count);
            TotalPatients = dash.TotalPatients;
            TestsToday = dash.TestsToday;
            TestsThisMonth = dash.TestsThisMonth;
            RecentTests = dash.RecentTests;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void GoToPatients() => PatientsRequested?.Invoke();

    [RelayCommand]
    private void OpenResult(RecentTestDto? row)
    {
        if (row is not null)
            ViewResultRequested?.Invoke(row.MedTestId);
    }
}
