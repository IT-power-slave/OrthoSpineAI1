using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Domain.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrthoSpineAI.UI.Reports;

/// <summary>
/// QuestPDF document that renders the <see cref="DiagnosticForm"/> aggregate
/// into a structured, printable A4 PDF report.
/// </summary>
internal sealed class DiagnosticReportDocument : IDocument
{
    private readonly DiagnosticForm _form;
    private readonly PatientDto _patient;

    private static readonly string AccentBlue   = "#1565C0";
    private static readonly string LightBlue    = "#E3F2FD";
    private static readonly string ActiveGreen  = "#E8F5E9";
    private static readonly string ActiveBorder = "#A5D6A7";
    private static readonly string InactiveGray = "#F5F5F5";
    private static readonly string TextDark     = "#212121";
    private static readonly string TextMuted    = "#757575";

    private string PilsColor => _form.PilsVariant switch
    {
        1 => "#4CAF50",
        2 => "#FFC107",
        3 => "#FF5722",
        4 => "#F44336",
        _ => "#9E9E9E"
    };

    private string PilsLabel => _form.PilsVariant switch
    {
        1 => "Wariant I – niskie ryzyko",
        2 => "Wariant II – umiarkowane ryzyko",
        3 => "Wariant III – wysokie ryzyko",
        4 => "Wariant IV – bardzo wysokie ryzyko",
        _ => $"Wariant {_form.PilsVariant}"
    };

    public DiagnosticReportDocument(DiagnosticForm form, PatientDto patient)
    {
        _form    = form;
        _patient = patient;
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title   = $"Raport AWWS – {_patient.FullName}",
        Author  = "OrthoSpineAI",
        Subject = "Diagnostyka postawy ciała",
    };

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30, Unit.Point);
            page.DefaultTextStyle(t => t.FontSize(10).FontColor(TextDark));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    // ── Header ──────────────────────────────────────────────────────────────

    private void ComposeHeader(IContainer c)
    {
        c.BorderBottom(1).BorderColor("#E0E0E0").PaddingBottom(8).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("RAPORT DIAGNOSTYCZNY AWWS / PiLS")
                    .FontSize(16).Bold().FontColor(AccentBlue);
                col.Item().Text("OrthoSpineAI — System oceny postawy ciała")
                    .FontSize(9).FontColor(TextMuted);
            });

            row.ConstantItem(120).AlignRight().Column(col =>
            {
                col.Item().Text(_form.ExaminationDate.ToLocalTime().ToString("dd.MM.yyyy"))
                    .FontSize(11).Bold();
                col.Item().Text(_form.ExaminationDate.ToLocalTime().ToString("HH:mm"))
                    .FontSize(9).FontColor(TextMuted);
            });
        });
    }

    // ── Content ─────────────────────────────────────────────────────────────

    private void ComposeContent(IContainer c)
    {
        c.PaddingTop(12).Column(col =>
        {
            // Patient & session info
            col.Item().Element(ComposePatientCard);
            col.Item().PaddingTop(10).Element(ComposePilsCard);
            col.Item().PaddingTop(10).Element(ComposeClinicalText);
            col.Item().PaddingTop(10).Element(ComposeParameterGroups);
        });
    }

    private void ComposePatientCard(IContainer c)
    {
        c.Background(LightBlue).Padding(10).Column(col =>
        {
            col.Item().Text("Dane pacjenta i badania").Bold().FontSize(11).FontColor(AccentBlue);
            col.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Column(inner =>
                {
                    inner.Item().LabelValue("Pacjent", _patient.FullName);
                    inner.Item().LabelValue("Definicja badania", _form.SurveyName);
                    if (!string.IsNullOrWhiteSpace(_form.PatientNotes))
                        inner.Item().LabelValue("Notatki kliniczne", _form.PatientNotes);
                });

                row.RelativeItem().Column(inner =>
                {
                    inner.Item().LabelValue("Wiek", $"{_form.AgeYears} lat");
                    inner.Item().LabelValue("Wzrost", $"{(int)_form.Height} cm");
                    inner.Item().LabelValue("Masa ciała", $"{(int)_form.Weight} kg");
                });
            });
        });
    }

    private void ComposePilsCard(IContainer c)
    {
        c.Row(row =>
        {
            // PiLS variant badge
            row.RelativeItem(1).Background(PilsColor).Padding(12).Column(col =>
            {
                col.Item().Text("Wariant PiLS").FontSize(9).FontColor(Colors.White).Italic();
                col.Item().Text(_form.PilsVariant.ToString()).FontSize(32).Bold().FontColor(Colors.White);
                col.Item().Text(PilsLabel).FontSize(9).FontColor(Colors.White);
            });

            // Control key
            row.ConstantItem(8);
            row.RelativeItem(1).Background(LightBlue).Padding(12).Column(col =>
            {
                col.Item().Text("Klucz kontroli").FontSize(9).FontColor(AccentBlue).Italic();
                col.Item().Text(_form.PilsControlKey.ToString()).FontSize(32).Bold().FontColor(AccentBlue);
                col.Item().Text("Aktywne grupy diagnostyczne").FontSize(9).FontColor(TextMuted);
            });
        });
    }

    private void ComposeClinicalText(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Element(x => SectionTitle(x, "Wniosek kliniczny"));
            col.Item().Border(1).BorderColor("#E0E0E0").Padding(10)
                .Text(_form.Conclusion).FontSize(10);

            col.Item().PaddingTop(6).Element(x => SectionTitle(x, "Zalecenie kontrolne"));
            col.Item().Border(1).BorderColor("#E0E0E0").Padding(10)
                .Text(_form.ControlRecommendation).FontSize(10);
        });
    }

    private void ComposeParameterGroups(IContainer c)
    {
        c.Column(col =>
        {
            col.Item().Element(x => SectionTitle(x, "Grupy parametrów diagnostycznych"));
            col.Item().PaddingTop(4).Column(groups =>
            {
                foreach (var group in _form.ParametersGroups)
                {
                    groups.Item().PaddingBottom(4).Element(g =>
                    {
                        var bg     = group.IsActive ? ActiveGreen  : InactiveGray;
                        var border = group.IsActive ? ActiveBorder : "#E0E0E0";

                        g.Border(1).BorderColor(border).Column(inner =>
                        {
                            // Group header row
                            inner.Item().Background(bg).Padding(6).Row(r =>
                            {
                                r.ConstantItem(14).AlignMiddle()
                                    .Text(group.IsActive ? "✓" : "–")
                                    .FontSize(10).Bold()
                                    .FontColor(group.IsActive ? "#388E3C" : "#9E9E9E");
                                r.RelativeItem().AlignMiddle()
                                    .Text(group.DisplayLabel).Bold().FontSize(10);
                            });

                            // Parameter rows
                            if (group.Parameters.Count > 0)
                            {
                                inner.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(c =>
                                    {
                                        c.RelativeColumn(3);
                                        c.RelativeColumn(1);
                                    });

                                    foreach (var p in group.Parameters)
                                    {
                                        table.Cell().PaddingLeft(20).PaddingVertical(2)
                                            .Text(p.Label).FontSize(9).FontColor(TextMuted);
                                        table.Cell().PaddingVertical(2).AlignRight().PaddingRight(8)
                                            .Text(p.Value).FontSize(9).Bold();
                                    }
                                });
                            }
                        });
                    });
                }
            });
        });
    }

    // ── Footer ──────────────────────────────────────────────────────────────

    private void ComposeFooter(IContainer c)
    {
        c.BorderTop(1).BorderColor("#E0E0E0").PaddingTop(6).Row(row =>
        {
            row.RelativeItem().Text($"OrthoSpineAI  ·  Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}")
                .FontSize(8).FontColor(TextMuted);
            row.ConstantItem(60).AlignRight()
                .Text(x =>
                {
                    x.Span("Strona ").FontSize(8).FontColor(TextMuted);
                    x.CurrentPageNumber().FontSize(8).FontColor(TextMuted);
                    x.Span(" / ").FontSize(8).FontColor(TextMuted);
                    x.TotalPages().FontSize(8).FontColor(TextMuted);
                });
        });
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static void SectionTitle(IContainer c, string title)
    {
        c.BorderBottom(1).BorderColor("#90CAF9").PaddingBottom(3)
            .Text(title).Bold().FontSize(11).FontColor("#1565C0");
    }
}

internal static class ContainerExtensions
{
    /// <summary>Renders a "Label: Value" row used inside info cards.</summary>
    internal static void LabelValue(this IContainer c, string label, string value)
    {
        c.PaddingBottom(3).Row(r =>
        {
            r.ConstantItem(110).Text(label + ":").FontSize(9).FontColor("#757575");
            r.RelativeItem().Text(value).FontSize(9).Bold();
        });
    }
}
