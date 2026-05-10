using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;

namespace OrthoSpineAI.UI.ViewModels;

public partial class SurveySelectionViewModel : ViewModelBase
{
    private readonly ISurveyService _surveyService;

    [ObservableProperty]
    private IReadOnlyList<SurveyDefinitionDto> _definitions = [];

    [ObservableProperty]
    private SurveyDefinitionDto? _selectedDefinition;

    [ObservableProperty]
    private bool _isBusy;

    public PatientDto Patient { get; }

    public event Action<PatientDto, SurveyDefinitionDto>? SurveyStartRequested;
    public event Action? BackRequested;

    public SurveySelectionViewModel(ISurveyService surveyService, PatientDto patient)
    {
        _surveyService = surveyService;
        Patient = patient;
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Definitions = await _surveyService.GetAllDefinitionsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void StartSurvey()
    {
        if (SelectedDefinition is not null)
            SurveyStartRequested?.Invoke(Patient, SelectedDefinition);
    }

    private bool CanStart() => SelectedDefinition is not null;

    [RelayCommand]
    private void SelectDefinition(SurveyDefinitionDto? definition)
    {
        SelectedDefinition = definition;
    }

    partial void OnSelectedDefinitionChanged(SurveyDefinitionDto? value) =>
        StartSurveyCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void Back() => BackRequested?.Invoke();
}
