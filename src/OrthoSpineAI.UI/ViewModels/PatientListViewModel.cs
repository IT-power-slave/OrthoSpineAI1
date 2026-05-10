using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Services;

namespace OrthoSpineAI.UI.ViewModels;

public partial class PatientListViewModel : ViewModelBase
{
    private readonly PatientService _patientService;
    private IReadOnlyList<PatientDto> _allPatients = [];

    [ObservableProperty]
    private IReadOnlyList<PatientDto> _patients = [];

    [ObservableProperty]
    private PatientDto? _selectedPatient;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public event Action<PatientDto>? PatientSelected;
    public event Action? AddPatientRequested;

    public PatientListViewModel(PatientService patientService)
    {
        _patientService = patientService;
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            _allPatients = await _patientService.GetAllAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd ładowania: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Live filter — no extra DB call
    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Patients = _allPatients;
            return;
        }
        var lower = SearchText.ToLowerInvariant();
        Patients = _allPatients
            .Where(p => p.FirstName.Contains(lower, StringComparison.OrdinalIgnoreCase)
                     || p.LastName.Contains(lower, StringComparison.OrdinalIgnoreCase)
                     || p.PESEL.Contains(lower, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    [RelayCommand]
    private void SelectPatient(PatientDto? patient)
    {
        var target = patient ?? SelectedPatient;
        if (target is not null)
            PatientSelected?.Invoke(target);
    }

    [RelayCommand]
    private void AddPatient() => AddPatientRequested?.Invoke();

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync();
}

