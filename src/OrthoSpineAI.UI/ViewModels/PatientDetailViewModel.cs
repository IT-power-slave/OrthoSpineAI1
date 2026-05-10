using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.Application.Services;

namespace OrthoSpineAI.UI.ViewModels;

public partial class PatientDetailViewModel : ViewModelBase
{
    private readonly MedTestService _medTestService;
    private readonly PatientService _patientService;
    private readonly IDialogService _dialogService;

    public PatientDto Patient { get; }

    [ObservableProperty] private IReadOnlyList<MedTestDto> _testHistory = [];
    [ObservableProperty] private MedTestDto? _selectedTest;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _canViewResult;

    public event Action<PatientDto>? NewSurveyRequested;
    public event Action<PatientDto>? EditRequested;
    public event Action? BackRequested;
    public event Action? DeletedRequested;
    public event Action<int>? ViewResultRequested;

    public PatientDetailViewModel(MedTestService medTestService, PatientService patientService, IDialogService dialogService, PatientDto patient)
    {
        _medTestService = medTestService;
        _patientService = patientService;
        _dialogService = dialogService;
        Patient = patient;
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            TestHistory = await _medTestService.GetByPatientAsync(Patient.PatientId);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedTestChanged(MedTestDto? value) =>
        CanViewResult = value is not null;

    [RelayCommand(CanExecute = nameof(CanViewResult))]
    private void ViewResult()
    {
        if (SelectedTest is not null)
            ViewResultRequested?.Invoke(SelectedTest.MedTestId);
    }

    partial void OnCanViewResultChanged(bool value) =>
        ViewResultCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (!_dialogService.Confirm(
                $"Czy na pewno usunąć pacjenta {Patient.FullName}?\nOperacja jest nieodwracalna.",
                "Potwierdzenie usunięcia"))
            return;

        await _patientService.DeleteAsync(Patient.PatientId);
        DeletedRequested?.Invoke();
    }

    [RelayCommand]
    private void NewSurvey() => NewSurveyRequested?.Invoke(Patient);

    [RelayCommand]
    private void Edit() => EditRequested?.Invoke(Patient);

    [RelayCommand]
    private void Back() => BackRequested?.Invoke();
}
