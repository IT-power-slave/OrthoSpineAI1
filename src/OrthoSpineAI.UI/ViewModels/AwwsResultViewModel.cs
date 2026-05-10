using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using System.Text;
using System.Windows;

namespace OrthoSpineAI.UI.ViewModels;

public partial class AwwsResultViewModel : ViewModelBase
{
    public AwwsResultDto Result { get; }
    public PatientDto Patient { get; }

    public string PilsBadgeColor => Result.PilsVariant switch
    {
        1 => "#4CAF50",   // green  – healthy / low risk
        2 => "#FFC107",   // amber  – medium risk
        3 => "#FF5722",   // orange – high risk
        4 => "#F44336",   // red    – very high risk
        _ => "#9E9E9E"    // grey   – unknown
    };

    public string PilsLabel => Result.PilsVariant switch
    {
        1 => "Wariant I – niskie ryzyko",
        2 => "Wariant II – umiarkowane ryzyko",
        3 => "Wariant III – wysokie ryzyko",
        4 => "Wariant IV – bardzo wysokie ryzyko",
        _ => $"Wariant {Result.PilsVariant}"
    };

    public string ExaminationDateText
    {
        get => Result.ExaminationDate.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        set { /* read-only display property, setter required for TwoWay binding compatibility */ }
    }

    [ObservableProperty]
    private IReadOnlyList<GroupResultRow> _groupRows = [];

    public event Action? BackToPatients;
    public event Action<PatientDto>? NewSurveyRequested;

    public AwwsResultViewModel(AwwsResultDto result, PatientDto patient)
    {
        Result = result;
        Patient = patient;
        GroupRows = result.GroupResults
            .Select(kv => new GroupResultRow(kv.Key, kv.Value))
            .ToList();
    }

    [RelayCommand]
    private void Finish() => BackToPatients?.Invoke();

    [RelayCommand]
    private void NewSurvey() => NewSurveyRequested?.Invoke(Patient);

    [RelayCommand]
    private void CopyToClipboard()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Wynik badania AWWS/PiLS — {ExaminationDateText}");
        sb.AppendLine($"Pacjent: {Patient.FullName}");
        sb.AppendLine($"Definicja: {Result.SurveyName}");
        sb.AppendLine();
        sb.AppendLine($"Wariant PiLS: {Result.PilsVariant} — {PilsLabel}");
        sb.AppendLine($"Klucz kontroli: {Result.PilsControlKey}");
        sb.AppendLine();
        sb.AppendLine("Wniosek kliniczny:");
        sb.AppendLine(Result.Conclusion);
        sb.AppendLine();
        sb.AppendLine("Zalecenie kontrolne:");
        sb.AppendLine(Result.ControlRecommendation);
        sb.AppendLine();
        sb.AppendLine("Grupy diagnostyczne:");
        foreach (var row in GroupRows)
            sb.AppendLine($"  {(row.IsPositive ? "✓" : "–")} {row.Group}");

        Clipboard.SetText(sb.ToString());
    }
}

public record GroupResultRow(string Group, bool IsPositive);
