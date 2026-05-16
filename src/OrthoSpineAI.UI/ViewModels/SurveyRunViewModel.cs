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
    private readonly int _hs;
    private readonly bool _testPP;
    private readonly bool _kneeValgus;
    private readonly bool _tarsalValgus;
    private readonly bool _gaitDisturbance;
    private readonly OrthoSpineAI.Domain.Enums.MedTestSide _side;
    private readonly string _description;

    // Per-stage capture log
    public ObservableCollection<CapturedStageRow> CapturedStages { get; } = [];

    // ── Survey group support (gap #9/#10) ────────────────────────────────────
    // All definitions that form the group (e.g. backbone → backbone.1 → backbone.2 → backbone.summary).
    private readonly IReadOnlyList<SurveyDefinitionDto> _group;
    private int _definitionIndex;   // index into _group for the currently running definition

    // ── Patient / survey info ────────────────────────────────────────────────
    public PatientDto Patient { get; }
    public SurveyDefinitionDto Definition { get; private set; }
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

    /// <summary>True when the current stage is a preparation/instruction stage (OrtMeas == MEAS_NULL).
    /// No measurement is taken and no MedTestResult row should be created.</summary>
    public bool IsNullMeasStage =>
        CurrentStage?.OrtMeas == OrthoSpineAI.Domain.Enums.ORT100Measurement.MEAS_NULL;

    /// <summary>True only for BTN_SAMPLE stages: the clinician must capture a reading before
    /// advancing and the result is saved to MedTestResult. BTN_NEXT and BTN_RESET stages
    /// require no capture and produce no MedTestResult row.</summary>
    public bool IsBtnSampleStage =>
        CurrentStage?.OrtNextStepButton == OrthoSpineAI.Domain.Enums.ORT100Button.BTN_SAMPLE;

    /// <summary>True when the current stage has at least one ISOM normative reference value to display.</summary>
    public bool HasIsomReference =>
        CurrentStage?.ValueISOM1.HasValue == true || CurrentStage?.ValueISOM3.HasValue == true;

    // ── Vertebra highlight helpers (bound to spine diagram in SurveyRunView) ──────────
    private OrthoSpineAI.Domain.Enums.ORT100ControlState OrtState =>
        CurrentStage?.OrtState ?? OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_NONE;

    public bool IsSpineDiagramVisible =>
        OrtState != OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_NONE;

    public bool HighlightC7   => OrtState == OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_ALL
                               || OrtState == OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_C7;
    public bool HighlightTH6  => OrtState == OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_ALL
                               || OrtState == OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_TH6;
    public bool HighlightTH12 => OrtState == OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_ALL
                               || OrtState == OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_TH12;
    public bool HighlightL3   => OrtState == OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_ALL
                               || OrtState == OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_L3;
    public bool HighlightS1   => OrtState == OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_ALL
                               || OrtState == OrthoSpineAI.Domain.Enums.ORT100ControlState.HIGHLIGHT_S1;

    /// <summary>True when the currently running definition is a *.summary display-only definition (gap #10).</summary>
    public bool IsSummaryDefinition => Definition.Key.EndsWith(".summary", StringComparison.OrdinalIgnoreCase);

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
        int hs = 0,
        bool testPP = false,
        bool kneeValgus = false,
        bool tarsalValgus = false,
        bool gaitDisturbance = false,
        OrthoSpineAI.Domain.Enums.MedTestSide side = OrthoSpineAI.Domain.Enums.MedTestSide.SIDE_NONE,
        string description = "",
        IReadOnlyList<SurveyDefinitionDto>? group = null)
    {
        _medTestService = medTestService;
        _device = device;
        Patient = patient;
        Definition = definition;
        CurrentUser = currentUser;
        _weight = weight;
        _growth = growth;
        _beighton = beighton;
        _hs = hs;
        _testPP = testPP;
        _kneeValgus = kneeValgus;
        _tarsalValgus = tarsalValgus;
        _gaitDisturbance = gaitDisturbance;
        _side = side;
        _description = description;
        // Build the group list: if a group is provided use it; otherwise treat the single definition as a group of one.
        _group = group is { Count: > 0 } ? group : [definition];
        var foundIdx = _group.ToList().IndexOf(definition);
        _definitionIndex = foundIdx >= 0 ? foundIdx : 0;
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
            Hs: _hs,
            TestPP: _testPP,
            KneeValgus: _kneeValgus,
            TarsalValgus: _tarsalValgus,
            GaitDisturbance: _gaitDisturbance,
            PatientId: Patient.PatientId,
            SystemUserId: CurrentUser.SystemUserId,
            Description: _description);
        var test = await _medTestService.CreateAsync(createDto);
        _medTestId = test.MedTestId;

        _device.Start(_cts.Token);
        GoToStage(0);
    }

    private void GoToStage(int index)
    {
        if (index >= Definition.Stages.Count)
        {
            // Current definition exhausted — try to advance to the next definition in the group.
            int nextDefIndex = _definitionIndex + 1;
            if (nextDefIndex < _group.Count)
            {
                AdvanceToDefinition(nextDefIndex);
            }
            else
            {
                FinishSurvey();
            }
            return;
        }
        _stageIndex = index;
        StageNumber = index + 1;
        CurrentStage = Definition.Stages[index];
        IsLastStage = StageNumber == TotalStages;
        ProgressText = $"Krok {StageNumber} / {TotalStages}";
        OnPropertyChanged(nameof(IsNullMeasStage));
        OnPropertyChanged(nameof(IsBtnSampleStage));
        OnPropertyChanged(nameof(HasIsomReference));
        OnPropertyChanged(nameof(IsSpineDiagramVisible));
        OnPropertyChanged(nameof(HighlightC7));
        OnPropertyChanged(nameof(HighlightTH6));
        OnPropertyChanged(nameof(HighlightTH12));
        OnPropertyChanged(nameof(HighlightL3));
        OnPropertyChanged(nameof(HighlightS1));
        OnPropertyChanged(nameof(IsSummaryDefinition));
        // Only BTN_SAMPLE stages require the clinician to capture a reading before advancing.
        // BTN_NEXT and BTN_RESET stages advance freely — pre-set HasCapturedValue to true.
        HasCapturedValue = !IsBtnSampleStage;
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

    /// <summary>Switches the view-model to the next definition in the group (gap #9).
    /// The same MedTestId is reused — no new MedTest row is created.</summary>
    private void AdvanceToDefinition(int defIndex)
    {
        _definitionIndex = defIndex;
        var nextDef = _group[defIndex];
        // Swap the observable Definition property so bindings update
        Definition = nextDef;
        TotalStages = nextDef.Stages.Count;
        _stageIndex = -1;
        CapturedStages.Clear();
        OnPropertyChanged(nameof(Definition));
        OnPropertyChanged(nameof(IsSummaryDefinition));
        GoToStage(0);
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

        // Only BTN_SAMPLE stages produce a MedTestResult row (gap #2 + #3).
        // BTN_NEXT (instruction/computed) and BTN_RESET (repositioning) stages are skipped.
        if (IsBtnSampleStage)
        {
            StatusMessage = "Zapisywanie…";
            var dto = new SaveMeasurementDto(
                _medTestId,
                CurrentStage.Plane,
                CurrentStage.OrtMeas,
                CapturedValue, "°",
                _side);
            await _medTestService.SaveMeasurementAsync(dto);

            // Log to the captured history panel
            CapturedStages.Add(new CapturedStageRow(StageNumber, CurrentStage.Name, CapturedValue));
        }

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

            // Gap #4 — persist every frame for continuous Adams stages (OrtContinousMeas = true)
            if (CurrentStage?.OrtContinousMeas == true && _medTestId > 0)
            {
                var dto = new SaveContinuousFrameDto(
                    MedTestId: _medTestId,
                    OrtMeas: CurrentStage.OrtMeas,
                    Status: frame.Status.RawValue,
                    Signal: frame.Signal,
                    Battery: frame.Battery,
                    Shake: frame.Shake,
                    Roll: frame.Roll,
                    RollOffset: frame.RollOffset,
                    Tilt: frame.Tilt,
                    Way: frame.Way,
                    Space: frame.Space,
                    Force1: frame.Force1,
                    Force2: frame.Force2);
                _ = _medTestService.SaveContinuousFrameAsync(dto, _cts.Token);
            }
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
