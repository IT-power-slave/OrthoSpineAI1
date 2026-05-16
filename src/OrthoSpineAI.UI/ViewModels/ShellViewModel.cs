using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Models;

namespace OrthoSpineAI.UI.ViewModels;

/// <summary>
/// Top-level navigation shell.  Hosts one active child ViewModel at a time.
/// DataTemplates in App.xaml map each VM type to its View.
/// </summary>
public partial class ShellViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IPatientService _patientService;
    private readonly ISurveyService _surveyService;
    private readonly IMedTestService _medTestService;
    private readonly IDeviceDriver _device;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private string _currentUserName = string.Empty;

    [ObservableProperty]
    private bool _isLoggedIn;

    private SystemUser? _loggedUser;

    public ShellViewModel(
        IAuthService authService,
        IPatientService patientService,
        ISurveyService surveyService,
        IMedTestService medTestService,
        IDeviceDriver device,
        IDialogService dialogService)
    {
        _authService = authService;
        _patientService = patientService;
        _surveyService = surveyService;
        _medTestService = medTestService;
        _device = device;
        _dialogService = dialogService;

        NavigateToLogin();
    }

    // ── Navigation helpers ───────────────────────────────────────────────────

    private void NavigateToLogin()
    {
        IsLoggedIn = false;
        CurrentUserName = string.Empty;
        var vm = new LoginViewModel(_authService);
        vm.LoginSucceeded += OnLoginSucceeded;
        CurrentPage = vm;
    }

    private void OnLoginSucceeded(SystemUser user)
    {
        _loggedUser = user;
        IsLoggedIn = true;
        CurrentUserName = user.Login;
        NavigateToDashboard();
    }

    private void NavigateToDashboard()
    {
        var vm = new DashboardViewModel(_patientService, _medTestService, CurrentUserName);
        vm.PatientsRequested += NavigateToPatientList;
        vm.ViewResultRequested += medTestId => _ = NavigateToHistoricResultFromDashboardAsync(medTestId);
        _ = vm.LoadAsync();
        CurrentPage = vm;
    }

    private async Task NavigateToHistoricResultFromDashboardAsync(int medTestId)
    {
        var result = await _medTestService.GetAwwsResultAsync(medTestId);
        if (result is null) return;
        var patient = await _patientService.GetByIdAsync(result.PatientId);
        if (patient is null) return;
        NavigateToAwwsResult(result, patient);
    }


    private void NavigateToPatientList()
    {
        var vm = new PatientListViewModel(_patientService);
        vm.PatientSelected += patient => NavigateToPatientDetail(patient);
        vm.AddPatientRequested += NavigateToAddPatient;
        _ = vm.LoadAsync();
        CurrentPage = vm;
    }

    private void NavigateToAddPatient()
    {
        var vm = new AddPatientViewModel(_patientService, _loggedUser!.ClinicId);
        vm.PatientSaved += _ => NavigateToPatientList();
        vm.Cancelled += NavigateToPatientList;
        CurrentPage = vm;
    }

    private void NavigateToPatientDetail(PatientDto patient)
    {
        var vm = new PatientDetailViewModel(_medTestService, _patientService, _dialogService, patient);
        vm.NewSurveyRequested += NavigateToSurveySelection;
        vm.EditRequested += NavigateToEditPatient;
        vm.BackRequested += NavigateToPatientList;
        vm.ViewResultRequested += medTestId => _ = NavigateToHistoricResultAsync(medTestId, patient);
        vm.DeletedRequested += NavigateToPatientList;
        _ = vm.LoadAsync();
        CurrentPage = vm;
    }

    private void NavigateToEditPatient(PatientDto patient)
    {
        var vm = new EditPatientViewModel(_patientService, patient);
        vm.Saved += saved => NavigateToPatientDetail(saved);
        vm.Cancelled += () => NavigateToPatientDetail(patient);
        CurrentPage = vm;
    }

    private void NavigateToSurveySelection(PatientDto patient)
    {
        var vm = new SurveySelectionViewModel(_surveyService, patient);
        vm.SurveyStartRequested += (p, def, side) => NavigateToPreTest(p, def, side);
        vm.BackRequested += NavigateToPatientList;
        _ = vm.LoadAsync();
        CurrentPage = vm;
    }

    private void NavigateToPreTest(PatientDto patient, SurveyDefinitionDto definition, OrthoSpineAI.Domain.Enums.MedTestSide side)
    {
        var vm = new PreTestViewModel(patient, definition);
        vm.Confirmed += preTest => _ = NavigateToSurveyRunAsync(preTest, side);
        vm.Cancelled += () => NavigateToSurveySelection(patient);
        CurrentPage = vm;
    }

    private async Task NavigateToSurveyRunAsync(PreTestViewModel preTest, OrthoSpineAI.Domain.Enums.MedTestSide side)
    {
        // Load the full definition group (e.g. backbone → backbone.1 → backbone.2 → backbone.summary).
        // For single-stage surveys the group will contain only the selected definition.
        var rootKey = preTest.Definition.Key.Split('.')[0];
        var group = await _surveyService.GetSurveyGroupAsync(rootKey);
        // The first definition to run is the one the clinician selected (or index 0 if not found).
        var startDef = group.FirstOrDefault(d => d.Key == preTest.Definition.Key) ?? preTest.Definition;

        var userInfo = new SystemUserInfo(_loggedUser!.SystemUserId, _loggedUser.Login, _loggedUser.ClinicId);
        var vm = new SurveyRunViewModel(
            _medTestService, _device,
            preTest.Patient, startDef, userInfo,
            weight: preTest.Weight, growth: preTest.Growth,
            beighton: preTest.Beighton, hs: preTest.Hs, testPP: preTest.TestPP,
            kneeValgus: preTest.KneeValgus, tarsalValgus: preTest.TarsalValgus,
            gaitDisturbance: preTest.GaitDisturbance,
            side: side,
            description: preTest.Description,
            group: group);
        vm.SurveyCompleted += result => NavigateToAwwsResult(result, preTest.Patient);
        vm.Cancelled += NavigateToPatientList;
        CurrentPage = vm;
        await vm.InitAsync();
    }

    private void NavigateToAwwsResult(AwwsResultDto result, PatientDto patient)
    {
        var vm = new AwwsResultViewModel(result, patient, _medTestService);
        vm.BackToPatients += NavigateToPatientList;
        vm.NewSurveyRequested += NavigateToSurveySelection;
        CurrentPage = vm;
    }

    private async Task NavigateToHistoricResultAsync(int medTestId, PatientDto patient)
    {
        var result = await _medTestService.GetAwwsResultAsync(medTestId);
        if (result is null)
        {
            NavigateToPatientDetail(patient);
            return;
        }
        NavigateToAwwsResult(result, patient);
    }

    [RelayCommand]
    private void GoToDashboard()
    {
        if (IsLoggedIn) NavigateToDashboard();
    }

    [RelayCommand]
    private void GoToPatients()
    {
        if (IsLoggedIn) NavigateToPatientList();
    }

    [RelayCommand]
    private void Logout() => NavigateToLogin();
}
