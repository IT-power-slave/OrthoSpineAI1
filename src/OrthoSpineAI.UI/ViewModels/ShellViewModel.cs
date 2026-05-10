using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Services;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Models;

namespace OrthoSpineAI.UI.ViewModels;

/// <summary>
/// Top-level navigation shell.  Hosts one active child ViewModel at a time.
/// DataTemplates in App.xaml map each VM type to its View.
/// </summary>
public partial class ShellViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly PatientService _patientService;
    private readonly SurveyService _surveyService;
    private readonly MedTestService _medTestService;
    private readonly IDeviceDriver _device;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private string _currentUserName = string.Empty;

    [ObservableProperty]
    private bool _isLoggedIn;

    private SystemUser? _loggedUser;

    public ShellViewModel(
        AuthService authService,
        PatientService patientService,
        SurveyService surveyService,
        MedTestService medTestService,
        IDeviceDriver device)
    {
        _authService = authService;
        _patientService = patientService;
        _surveyService = surveyService;
        _medTestService = medTestService;
        _device = device;

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
        var dash = await _medTestService.GetDashboardAsync(0);
        var row = dash.RecentTests.FirstOrDefault(r => r.MedTestId == medTestId);
        if (row is null) return;
        var patients = await _patientService.GetAllAsync();
        var patient = patients.FirstOrDefault(p => p.PatientId == row.PatientId);
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
        var vm = new PatientDetailViewModel(_medTestService, _patientService, patient);
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
        vm.SurveyStartRequested += (p, def) => NavigateToPreTest(p, def);
        vm.BackRequested += NavigateToPatientList;
        _ = vm.LoadAsync();
        CurrentPage = vm;
    }

    private void NavigateToPreTest(PatientDto patient, SurveyDefinitionDto definition)
    {
        var vm = new PreTestViewModel(patient, definition);
        vm.Confirmed += preTest => _ = NavigateToSurveyRunAsync(preTest);
        vm.Cancelled += () => NavigateToSurveySelection(patient);
        CurrentPage = vm;
    }

    private async Task NavigateToSurveyRunAsync(PreTestViewModel preTest)
    {
        var userInfo = new SystemUserInfo(_loggedUser!.SystemUserId, _loggedUser.Login, _loggedUser.ClinicId);
        var vm = new SurveyRunViewModel(
            _medTestService, _device,
            preTest.Patient, preTest.Definition, userInfo,
            weight: preTest.Weight, growth: preTest.Growth,
            beighton: preTest.Beighton, testPP: preTest.TestPP,
            kneeValgus: preTest.KneeValgus, tarsalValgus: preTest.TarsalValgus,
            gaitDisturbance: preTest.GaitDisturbance);
        vm.SurveyCompleted += result => NavigateToAwwsResult(result, preTest.Patient);
        vm.Cancelled += NavigateToPatientList;
        CurrentPage = vm;
        await vm.InitAsync();
    }

    private void NavigateToAwwsResult(AwwsResultDto result, PatientDto patient)
    {
        var vm = new AwwsResultViewModel(result, patient);
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
