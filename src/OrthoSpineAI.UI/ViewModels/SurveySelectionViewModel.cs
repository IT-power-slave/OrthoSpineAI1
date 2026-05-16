using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.UI.ViewModels;

public partial class SurveySelectionViewModel : ViewModelBase
{
    private static readonly HashSet<string> BilateralKeys =
        ["shoulder", "elbow", "hip", "knee", "wrist", "ankle"];

    private readonly ISurveyService _surveyService;

    [ObservableProperty]
    private IReadOnlyList<SurveyDefinitionDto> _definitions = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBilateralSurvey))]
    [NotifyCanExecuteChangedFor(nameof(StartSurveyCommand))]
    private SurveyDefinitionDto? _selectedDefinition;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSurveyCommand))]
    [NotifyPropertyChangedFor(nameof(IsSideLeft))]
    [NotifyPropertyChangedFor(nameof(IsSideRight))]
    private MedTestSide _selectedSide = MedTestSide.SIDE_NONE;

    public bool IsSideLeft => SelectedSide == MedTestSide.SIDE_LEFT;
    public bool IsSideRight => SelectedSide == MedTestSide.SIDE_RIGHT;

    public bool IsBilateralSurvey =>
        SelectedDefinition is not null &&
        BilateralKeys.Contains(SelectedDefinition.Key.Split('.')[0].ToLowerInvariant());

    public PatientDto Patient { get; }

    public event Action<PatientDto, SurveyDefinitionDto, MedTestSide>? SurveyStartRequested;
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
            SurveyStartRequested?.Invoke(Patient, SelectedDefinition, SelectedSide);
    }

    private bool CanStart() =>
        SelectedDefinition is not null &&
        (!IsBilateralSurvey || SelectedSide != MedTestSide.SIDE_NONE);

    [RelayCommand]
    private void SelectDefinition(SurveyDefinitionDto? definition)
    {
        SelectedDefinition = definition;
        // Reset side when switching surveys
        SelectedSide = MedTestSide.SIDE_NONE;
    }

    [RelayCommand]
    private void SelectLeft() => SelectedSide = MedTestSide.SIDE_LEFT;

    [RelayCommand]
    private void SelectRight() => SelectedSide = MedTestSide.SIDE_RIGHT;

    partial void OnSelectedDefinitionChanged(SurveyDefinitionDto? value)
    {
        OnPropertyChanged(nameof(IsBilateralSurvey));
        StartSurveyCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke();
}
