using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;

namespace OrthoSpineAI.UI.ViewModels;

/// <summary>
/// Collects anthropometric data (weight, height, Beighton, examination flags)
/// before a MedTest is created.
/// </summary>
public partial class PreTestViewModel : ViewModelBase
{
    public PatientDto Patient { get; }
    public SurveyDefinitionDto Definition { get; }

    [ObservableProperty] private double _weight = 30;
    [ObservableProperty] private double _growth = 140;
    [ObservableProperty] private int _beighton;
    [ObservableProperty] private int _hs;
    [ObservableProperty] private bool _testPP;
    [ObservableProperty] private bool _kneeValgus;
    [ObservableProperty] private bool _tarsalValgus;
    [ObservableProperty] private bool _gaitDisturbance;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _validationMessage = string.Empty;

    public string BmiText
    {
        get
        {
            if (Weight <= 0 || Growth <= 0) return "—";
            double bmi = Weight / Math.Pow(Growth / 100.0, 2);
            return $"{bmi:F1}";
        }
    }

    public string BmiCategory
    {
        get
        {
            if (Weight <= 0 || Growth <= 0) return string.Empty;
            double bmi = Weight / Math.Pow(Growth / 100.0, 2);
            return bmi switch
            {
                < 18.5 => "Niedowaga",
                < 25.0 => "Prawidłowa",
                < 30.0 => "Nadwaga",
                _      => "Otyłość"
            };
        }
    }

    partial void OnWeightChanged(double _) { OnPropertyChanged(nameof(BmiText)); OnPropertyChanged(nameof(BmiCategory)); }
    partial void OnGrowthChanged(double _) { OnPropertyChanged(nameof(BmiText)); OnPropertyChanged(nameof(BmiCategory)); }

    public event Action<PreTestViewModel>? Confirmed;
    public event Action? Cancelled;

    public PreTestViewModel(PatientDto patient, SurveyDefinitionDto definition)
    {
        Patient = patient;
        Definition = definition;
    }

    [RelayCommand]
    private void Confirm()
    {
        ValidationMessage = string.Empty;
        if (Weight <= 0 || Weight > 300)
        {
            ValidationMessage = "Podaj prawidłową masę ciała (kg).";
            return;
        }
        if (Growth <= 0 || Growth > 250)
        {
            ValidationMessage = "Podaj prawidłowy wzrost (cm).";
            return;
        }
        if (Beighton < 0 || Beighton > 9)
        {
            ValidationMessage = "Skala Beightona: wartość od 0 do 9.";
            return;
        }
        if (Hs < 0 || Hs > 20)
        {
            ValidationMessage = "Hump Score: wartość od 0 do 20.";
            return;
        }
        Confirmed?.Invoke(this);
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke();
}
