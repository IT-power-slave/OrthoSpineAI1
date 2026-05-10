using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.Domain.Models;
using System.Collections.ObjectModel;

namespace OrthoSpineAI.UI.ViewModels;

public partial class SurveyRunViewModel : ViewModelBase, IDisposable
{
    private readonly IMedTestService _medTestService;
    private readonly IDeviceDriver _device;
    private readonly CancellationTokenSource _cts = new();
    private int _medTestId;
    private int _stageIndex;
    private bool _disposed;

    // Pre-test anthropometric data
    private readonly double _weight;
    private readonly double _growth;
    private readonly int _beighton;
    private readonly bool _testPP;
    private readonly bool _kneeValgus;
    private readonly bool _tarsalValgus;
    private readonly bool _gaitDisturbance;

    // Per-stage capture log shown in the right panel
    public ObservableCollection<CapturedStageRow> CapturedStages { get; } = [];

    // ── Patient / survey info ────────────────────────────────────────────────
    public PatientDto Patient { get; }
    public SurveyDefinitionDto Definition { get; }
    public SystemUserInfo CurrentUser { get; }

    // ── Current stage ────────────────────────────────────────────────────────
    [ObservableProperty] private StageDto? _currentStage;
    [ObservableProperty] private int _stageNumber;
    [ObservableProperty] private int _totalStages;
    [ObservableProperty] private string _progressText = string.Empty;
    [ObservableProperty] private bool _isLastStage;
    [ObservableProperty] private bool _isFinalizing;

    // ── Live device telemetry ────────────────────────────────────────────────
    [ObservableProperty] private double _roll;
    [ObservableProperty] private double _tilt;
    [ObservableProperty] private int _way;
    [ObservableProperty] private double _battery;
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _deviceStatus = "Łączenie…";

    // ── Captured measurement ─────────────────────────────────────────────────
    [ObservableProperty] private double _capturedValue;
    [ObservableProperty] private bool _hasCapturedValue;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public event Action<AwwsResultDto>? SurveyCompleted;
    public event Action? Cancelled;

    public SurveyRunViewModel(
        IMedTestService medTestService,
        IDeviceDriver device,
        PatientDto patient,
        SurveyDefinitionDto definition,
        SystemUserInfo currentUser,
        double weight = 0,
        double growth = 0,
        int beighton = 0,
        bool testPP = false,
        bool kneeValgus = false,
        bool tarsalValgus = false,
        bool gaitDisturbance = false)
    {
        _medTestService = medTestService;
        _device = device;
        Patient = patient;
        Definition = definition;
        CurrentUser = currentUser;
        _weight = weight;
        _growth = growth;
        _beighton = beighton;
        _testPP = testPP;
        _kneeValgus = kneeValgus;
        _tarsalValgus = tarsalValgus;
        _gaitDisturbance = gaitDisturbance;
        TotalStages = definition.Stages.Count;
        _device.FrameReceived += OnFrameReceived;
    }

    public async Task InitAsync()
    {
        var createDto = new CreateMedTestDto(
            MedTestDefinitionKey: Definition.Key,
            Weight: _weight,
            Growth: _growth,
            Beighton: _beighton,
            TestPP: _testPP,
            KneeValgus: _kneeValgus,
            TarsalValgus: _tarsalValgus,
            GaitDisturbance: _gaitDisturbance,
            PatientId: Patient.PatientId,
            SystemUserId: CurrentUser.SystemUserId);
        var test = await _medTestService.CreateAsync(createDto);
        _medTestId = test.MedTestId;

        _device.Start(_cts.Token);
        GoToStage(0);
    }

    private void GoToStage(int index)
    {
        if (index >= Definition.Stages.Count)
        {
            FinishSurvey();
            return;
        }
        _stageIndex = index;
        StageNumber = index + 1;
        CurrentStage = Definition.Stages[index];
        IsLastStage = StageNumber == TotalStages;
        ProgressText = $"Krok {StageNumber} / {TotalStages}";
        HasCapturedValue = false;
        CapturedValue = 0;
        StatusMessage = string.Empty;

        if (CurrentStage is not null)
        {
            _device.SendConfig(DeviceConfig.FromResetFlag(
                CurrentStage.OrtMode,
                CurrentStage.OrtResetFlag,
                CurrentStage.Tip[..Math.Min(CurrentStage.Tip.Length, 10)],
                CurrentStage.TipControl[..Math.Min(CurrentStage.TipControl.Length, 10)]));
        }
    }

    [RelayCommand]
    private void CaptureValue()
    {
        CapturedValue = Roll;
        HasCapturedValue = true;
        StatusMessage = $"✓ Zapisano: {CapturedValue:F1}°";
    }

    [RelayCommand(CanExecute = nameof(CanNextStep))]
    private async Task NextStepAsync()
    {
        if (CurrentStage is null) return;
        StatusMessage = "Zapisywanie…";

        var dto = new SaveMeasurementDto(
            _medTestId,
            CurrentStage.Plane,
            CurrentStage.OrtMeas,
            CapturedValue, "°",
            OrthoSpineAI.Domain.Enums.MedTestSide.SIDE_NONE);
        await _medTestService.SaveMeasurementAsync(dto);

        // Log to the captured history panel
        CapturedStages.Add(new CapturedStageRow(StageNumber, CurrentStage.Name, CapturedValue));

        GoToStage(_stageIndex + 1);
    }

    private bool CanNextStep() => HasCapturedValue && !IsFinalizing;

    partial void OnHasCapturedValueChanged(bool value) =>
        NextStepCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    private void PreviousStep()
    {
        if (_stageIndex > 0) GoToStage(_stageIndex - 1);
    }

    [RelayCommand]
    private void CancelSurvey()
    {
        _cts.Cancel();
        _device.Stop();
        Cancelled?.Invoke();
    }

    private void FinishSurvey()
    {
        _device.Stop();
        _ = FinalizeAsync();
    }

    private async Task FinalizeAsync()
    {
        IsFinalizing = true;
        StatusMessage = "Obliczanie wyniku AWWS…";
        var result = await _medTestService.FinishTestAsync(_medTestId, Patient.AgeYears);
        SurveyCompleted?.Invoke(result);
    }

    private void OnFrameReceived(object? sender, DeviceFrame frame)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Roll = frame.Roll;
            Tilt = frame.Tilt;
            Way = frame.Way;
            Battery = Math.Round(frame.Battery, 2);
            IsConnected = _device.IsConnected;
            DeviceStatus = _device.IsConnected ? "Połączono ✓" : "Rozłączono";
        });
    }

    public void Dispose()
    {
        if (_disposed) return;
        _device.FrameReceived -= OnFrameReceived;
        _cts.Cancel();
        _cts.Dispose();
        _disposed = true;
    }
}

/// <summary>Lightweight user info passed to SurveyRunViewModel.</summary>
public record SystemUserInfo(int SystemUserId, string Login, int ClinicId);

/// <summary>One row in the captured-values history panel.</summary>
public record CapturedStageRow(int StageNumber, string StageName, double Value);
