using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.UI.Reports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using System.Text;
using System.Windows;

namespace OrthoSpineAI.UI.ViewModels;

public partial class AwwsResultViewModel : ViewModelBase
{
    private readonly IMedTestService? _medTestService;

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

    public AwwsResultViewModel(AwwsResultDto result, PatientDto patient,
        IMedTestService? medTestService = null)
    {
        Result = result;
        Patient = patient;
        _medTestService = medTestService;
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

    [RelayCommand]
    private async Task ExportReportAsync()
    {
        if (_medTestService is null) return;

        var form = await _medTestService.BuildDiagnosticFormAsync(Result.MedTestId, Patient.AgeYears);
        if (form is null)
        {
            MessageBox.Show("Nie można wygenerować raportu — brak danych.", "Eksport raportu",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("══════════════════════════════════════════════════════");
        sb.AppendLine("       RAPORT DIAGNOSTYCZNY OrthoSpineAI / AWWS");
        sb.AppendLine("══════════════════════════════════════════════════════");
        sb.AppendLine($"Pacjent       : {Patient.FullName}");
        sb.AppendLine($"Data badania  : {form.ExaminationDate.ToLocalTime():dd.MM.yyyy HH:mm}");
        sb.AppendLine($"Definicja     : {form.SurveyName}");
        sb.AppendLine($"Wiek / Wzrost / Masa: {form.AgeYears} lat / {(int)form.Height} cm / {(int)form.Weight} kg");
        if (!string.IsNullOrWhiteSpace(form.PatientNotes))
            sb.AppendLine($"Notatki       : {form.PatientNotes}");
        sb.AppendLine();
        sb.AppendLine($"Wariant PiLS  : {form.PilsVariant}");
        sb.AppendLine($"Klucz kontroli: {form.PilsControlKey}");
        sb.AppendLine();
        sb.AppendLine("── Wniosek kliniczny ─────────────────────────────────");
        sb.AppendLine(form.Conclusion);
        sb.AppendLine();
        sb.AppendLine("── Zalecenie kontrolne ───────────────────────────────");
        sb.AppendLine(form.ControlRecommendation);
        sb.AppendLine();
        sb.AppendLine("── Grupy parametrów diagnostycznych ──────────────────");
        foreach (var group in form.ParametersGroups)
        {
            var mark = group.IsActive ? "[✓]" : "[ ]";
            sb.AppendLine($"{mark} {group.DisplayLabel}");
            foreach (var p in group.Parameters)
                sb.AppendLine($"     {p.Label,-40} {p.Value}");
        }
        sb.AppendLine("══════════════════════════════════════════════════════");

        var fileName = $"Raport_{Patient.LastName}_{form.ExaminationDate.ToLocalTime():yyyyMMdd_HHmm}.txt";
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath = Path.Combine(desktopPath, fileName);
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);

        MessageBox.Show($"Raport zapisany na pulpicie:\n{fileName}", "Eksport raportu",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        if (_medTestService is null) return;

        var form = await _medTestService.BuildDiagnosticFormAsync(Result.MedTestId, Patient.AgeYears);
        if (form is null)
        {
            MessageBox.Show("Nie można wygenerować raportu PDF — brak danych.", "Eksport PDF",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // QuestPDF community licence (free for open-source / non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;

        var fileName   = $"Raport_{Patient.LastName}_{form.ExaminationDate.ToLocalTime():yyyyMMdd_HHmm}.pdf";
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var filePath   = Path.Combine(desktopPath, fileName);

        var document = new DiagnosticReportDocument(form, Patient);
        await Task.Run(() => document.GeneratePdf(filePath));

        var answer = MessageBox.Show(
            $"Raport PDF zapisany na pulpicie:\n{fileName}\n\nCzy otworzyć plik?",
            "Eksport PDF", MessageBoxButton.YesNo, MessageBoxImage.Information);

        if (answer == MessageBoxResult.Yes)
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath)
                { UseShellExecute = true });
    }
}

public record GroupResultRow(string Group, bool IsPositive);
