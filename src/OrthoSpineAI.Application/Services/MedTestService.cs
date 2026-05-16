using Microsoft.Extensions.Logging;
using OrthoSpineAI.Application.Algorithm;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Exceptions;
using OrthoSpineAI.Domain.Interfaces;
using OrthoSpineAI.Domain.Reports;
using System.Text.Json;

namespace OrthoSpineAI.Application.Services;

public class MedTestService : IMedTestService
{
    private readonly IMedTestRepository _repo;
    private readonly AwwsEngine _awwsEngine;
    private readonly ILogger<MedTestService> _logger;

    public MedTestService(IMedTestRepository repo, AwwsEngine awwsEngine, ILogger<MedTestService> logger)
    {
        _repo = repo;
        _awwsEngine = awwsEngine;
        _logger = logger;
    }

    public async Task<MedTestDto> CreateAsync(CreateMedTestDto dto, CancellationToken ct = default)
    {
        var entity = new MedTest
        {
            ExaminationDate = DateTime.UtcNow,
            Description = dto.Description,
            MedTestDefinitionKey = dto.MedTestDefinitionKey,
            Weight = dto.Weight,
            Growth = dto.Growth,
            Beighton = dto.Beighton,
            Hs = dto.Hs,
            TestPP = dto.TestPP,
            KneeValgus = dto.KneeValgus,
            TarsalValgus = dto.TarsalValgus,
            GaitDisturbance = dto.GaitDisturbance,
            PatientId = dto.PatientId,
            SystemUserId = dto.SystemUserId
        };
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
        _logger.LogInformation("MedTest created: {MedTestId} for PatientId={PatientId}", entity.MedTestId, dto.PatientId);
        return MapToDto(entity);
    }

    public async Task<IReadOnlyList<MedTestDto>> GetByPatientAsync(int patientId, CancellationToken ct = default)
    {
        var tests = await _repo.GetByPatientIdAsync(patientId, ct);
        return tests.Select(MapToDto).ToList();
    }

    public async Task SaveMeasurementAsync(SaveMeasurementDto dto, CancellationToken ct = default)
    {
        var result = new MedTestResult
        {
            MedTestId = dto.MedTestId,
            Plane = dto.Plane,
            OrtMeas = dto.OrtMeas,
            PhysicalValue = dto.PhysicalValue,
            PhysicalUnit = dto.PhysicalUnit,
            Side = dto.Side
        };
        await _repo.AddResultAsync(result, ct);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task SaveContinuousFrameAsync(SaveContinuousFrameDto dto, CancellationToken ct = default)
    {
        var result = new MedTestContinuousResult
        {
            MedTestId = dto.MedTestId,
            OrtMeas = dto.OrtMeas,
            Status = dto.Status,
            Signal = dto.Signal,
            Battery = dto.Battery,
            Shake = dto.Shake,
            Roll = dto.Roll,
            RollOffset = dto.RollOffset,
            Tilt = dto.Tilt,
            Way = dto.Way,
            Space = dto.Space,
            Force1 = dto.Force1,
            Force2 = dto.Force2,
            Timestamp = DateTime.UtcNow
        };
        await _repo.AddContinuousResultAsync(result, ct);
        await _repo.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Loads a completed MedTest with its results, builds the AWWS parameter dictionary,
    /// runs the engine, and returns the diagnostic result.
    /// </summary>
    public async Task<AwwsResultDto> FinishTestAsync(int medTestId, int patientAgeYears, CancellationToken ct = default)
    {
        var test = await _repo.GetByIdAsync(medTestId, ct);
        if (test is null)
        {
            _logger.LogWarning("FinishTestAsync — MedTest not found: {MedTestId}", medTestId);
            throw new MedTestNotFoundException(medTestId);
        }

        // Build parameter dictionary for AwwsEngine
        var p = new Dictionary<string, object>();

        // Scalar fields from MedTest
        p[AwwsParams.BEIGHTON]        = test.Beighton;
        p[AwwsParams.HS]              = test.Hs;
        p[AwwsParams.WEIGHT]          = (int)Math.Round(test.Weight);
        p[AwwsParams.HEIGHT]          = (int)Math.Round(test.Growth);
        p[AwwsParams.AGE]             = patientAgeYears;

        // Leg statics — driven by knee/tarsal valgus only (docs §4 PGLogicLegsStatics)
        p[AwwsParams.LEGSSTAT_DISTURBED] = test.KneeValgus || test.TarsalValgus;
        p[AwwsParams.LEGSSTAT_CORRECT]   = !test.KneeValgus && !test.TarsalValgus;

        // Map saved measurements
        var results = test.Results;

        double Meas(ORT100Measurement key) =>
            results.FirstOrDefault(r => r.OrtMeas == key)?.PhysicalValue ?? 0.0;

        // Saggittal plane curvatures
        p[AwwsParams.LL]  = (int)Math.Round(Meas(ORT100Measurement.MEAS_LL));
        p[AwwsParams.THK] = (int)Math.Round(Meas(ORT100Measurement.MEAS_KP));
        p[AwwsParams.PT]  = (int)Math.Round(Meas(ORT100Measurement.MEAS_NM));

        // ATR_max: computed from continuous Roll readings during Adams test (gap #5)
        // per docs: ATR_max = max(|Roll|) over continuous frames for AC7, AT6, AT12, AL3, ASIPS
        var adamsOrtMeas = new[]
        {
            ORT100Measurement.MEAS_AC7,
            ORT100Measurement.MEAS_AT6,
            ORT100Measurement.MEAS_AT12,
            ORT100Measurement.MEAS_AL3,
            ORT100Measurement.MEAS_ASIPS
        };
        var continuousRolls = test.ContinuousResults
            .Where(r => adamsOrtMeas.Contains(r.OrtMeas))
            .Select(r => Math.Abs(r.Roll))
            .ToList();
        int atrMax = continuousRolls.Count > 0
            ? (int)Math.Round(continuousRolls.Max())
            : 0;
        p[AwwsParams.ATR] = atrMax;

        // FLLD: functional leg-length discrepancy — driven by TestPP flag (docs §4 PGLogicFLLD)
        p[AwwsParams.FLLD_POSITIVE] = test.TestPP;
        p[AwwsParams.FLLD_NEGATIVE] = !test.TestPP;
        p[AwwsParams.FLLD_NEUTRAL]  = false;

        var engine = _awwsEngine;
        var dto = engine.Evaluate(p);

        // Persist the result
        var entity = new AwwsResult
        {
            MedTestId           = medTestId,
            PilsVariant         = dto.PilsVariant,
            PilsControlKey      = dto.PilsControlKey,
            Conclusion          = dto.Conclusion,
            ControlRecommendation = dto.ControlRecommendation,
            GroupResultsJson    = JsonSerializer.Serialize(dto.GroupResults)
        };
        await _repo.SaveAwwsResultAsync(entity, ct);
        _logger.LogInformation("FinishTestAsync completed: MedTestId={MedTestId} Variant={Variant}", medTestId, dto.PilsVariant);

        return dto with
        {
            MedTestId       = medTestId,
            PatientId       = test.PatientId,
            ExaminationDate = test.ExaminationDate,
            SurveyName      = test.MedTestDefinitionKey
        };
    }

    /// <summary>Loads a previously persisted AWWS result for a completed med test.</summary>
    public async Task<AwwsResultDto?> GetAwwsResultAsync(int medTestId, CancellationToken ct = default)
    {
        var test = await _repo.GetByIdAsync(medTestId, ct);
        var entity = await _repo.GetAwwsResultAsync(medTestId, ct);
        if (entity is null || test is null) return null;

        var groups = JsonSerializer.Deserialize<Dictionary<string, bool>>(entity.GroupResultsJson)
                     ?? new Dictionary<string, bool>();

        return new AwwsResultDto(
            medTestId,
            test.PatientId,
            test.ExaminationDate,
            test.MedTestDefinitionKey,
            entity.PilsVariant,
            entity.PilsControlKey,
            entity.Conclusion,
            entity.ControlRecommendation,
            groups);
    }

    private static AwwsResultDto EmptyResult() => new(0, 0, DateTime.UtcNow, string.Empty, 0, 0,
        "Brak danych — wynik AWWS nie istnieje.", string.Empty,
        new Dictionary<string, bool>());

    public async Task<DashboardDto> GetDashboardAsync(int patientCount, CancellationToken ct = default)
    {
        var today = await _repo.CountTodayAsync(ct);
        var month = await _repo.CountThisMonthAsync(ct);
        var recent = await _repo.GetRecentAsync(8, ct);

        var recentDtos = new List<RecentTestDto>();
        foreach (var t in recent)
        {
            var awws = await _repo.GetAwwsResultAsync(t.MedTestId, ct);
            recentDtos.Add(new RecentTestDto(
                t.MedTestId,
                t.PatientId,
                t.Patient is not null ? $"{t.Patient.FirstName} {t.Patient.LastName}" : "—",
                t.ExaminationDate,
                t.MedTestDefinitionKey,
                awws?.PilsVariant ?? 0));
        }

        return new DashboardDto(patientCount, today, month, recentDtos);
    }

    private static MedTestDto MapToDto(MedTest t) => new(
        t.MedTestId, t.ExaminationDate, t.Description,
        t.MedTestDefinitionKey, t.Weight, t.Growth,
        t.Beighton, t.Hs, t.TestPP, t.KneeValgus, t.TarsalValgus,
        t.GaitDisturbance, t.PatientId, t.SystemUserId);

    /// <summary>
    /// Builds the <see cref="DiagnosticForm"/> aggregate from the persisted MedTest and its
    /// stored AwwsResult, grouping AWWS parameters into labelled <see cref="IParametersGroup"/>
    /// instances that match the PG-Logic structure described in Appendix E of the docs.
    /// </summary>
    public async Task<DiagnosticForm?> BuildDiagnosticFormAsync(
        int medTestId, int patientAgeYears, CancellationToken ct = default)
    {
        var test = await _repo.GetByIdAsync(medTestId, ct);
        var awws = await _repo.GetAwwsResultAsync(medTestId, ct);
        if (test is null || awws is null) return null;

        var groups = JsonSerializer.Deserialize<Dictionary<string, bool>>(awws.GroupResultsJson)
                     ?? [];

        bool GroupActive(string name) => groups.TryGetValue(name, out var v) && v;

        string YesNo(bool v) => v ? "Tak" : "Nie";

        double Meas(ORT100Measurement key) =>
            test.Results.FirstOrDefault(r => r.OrtMeas == key)?.PhysicalValue ?? 0.0;

        var adamsOrtMeas = new[]
        {
            ORT100Measurement.MEAS_AC7, ORT100Measurement.MEAS_AT6,
            ORT100Measurement.MEAS_AT12, ORT100Measurement.MEAS_AL3,
            ORT100Measurement.MEAS_ASIPS
        };
        int atrMax = test.ContinuousResults
            .Where(r => adamsOrtMeas.Contains(r.OrtMeas))
            .Select(r => (int)Math.Round(Math.Abs(r.Roll)))
            .DefaultIfEmpty(0).Max();

        var paramGroups = new List<IParametersGroup>
        {
            new ParametersGroup("PGLogicAnthropometric", "Dane antropometryczne",
                GroupActive("PGLogicAnthropometric"),
                [
                    new ParameterEntry("AGE",    "Wiek",    $"{patientAgeYears} lat"),
                    new ParameterEntry("HEIGHT", "Wzrost",  $"{(int)Math.Round(test.Growth)} cm"),
                    new ParameterEntry("WEIGHT", "Masa",    $"{(int)Math.Round(test.Weight)} kg"),
                ]),

            new ParametersGroup("PGLogicAtr", "ATR / Wynik Huntera (AWWS)",
                GroupActive("PGLogicAtr"),
                [
                    new ParameterEntry("ATR", "ATR_max (Kąt rotacji tułowia)", $"{atrMax}°"),
                    new ParameterEntry("HS",  "HS (Wynik Huntera)",            $"{test.Hs}"),
                ]),

            new ParametersGroup("PGLogicBeightonScaleNumeric", "Skala Beightona",
                GroupActive("PGLogicBeightonScaleNumeric"),
                [
                    new ParameterEntry("BEIGHTON", "Wynik Beightona", $"{test.Beighton} pkt"),
                ]),

            new ParametersGroup("PGLogicFLLD", "FLLD – różnica długości kończyn",
                GroupActive("PGLogicFLLD"),
                [
                    new ParameterEntry("FLLD_POSITIVE", "FLLD dodatnie (TestPP)", YesNo(test.TestPP)),
                    new ParameterEntry("FLLD_NEGATIVE", "FLLD ujemne",            YesNo(!test.TestPP)),
                ]),

            new ParametersGroup("PGLogicLegsStatics", "Statyka kończyn dolnych",
                GroupActive("PGLogicLegsStatics"),
                [
                    new ParameterEntry("LEGSSTAT_DISTURBED", "Zaburzenia statyki", YesNo(test.KneeValgus || test.TarsalValgus)),
                    new ParameterEntry("KneeValgus",         "Koślawość kolan",   YesNo(test.KneeValgus)),
                    new ParameterEntry("TarsalValgus",       "Koślawość stępu",   YesNo(test.TarsalValgus)),
                ]),

            new ParametersGroup("PGLogicLLTHK", "Krzywizny strzałkowe kręgosłupa",
                GroupActive("PGLogicLLTHK"),
                [
                    new ParameterEntry("LL",  "Lordoza lędźwiowa (LL)",  $"{(int)Math.Round(Meas(ORT100Measurement.MEAS_LL))}°"),
                    new ParameterEntry("THK", "Kifoza piersiowa (THK)",  $"{(int)Math.Round(Meas(ORT100Measurement.MEAS_KP))}°"),
                ]),

            new ParametersGroup("PGLogicPT", "Nachylenie miednicy",
                GroupActive("PGLogicPT"),
                [
                    new ParameterEntry("PT", "PT – nachylenie miednicy (NM)", $"{(int)Math.Round(Meas(ORT100Measurement.MEAS_NM))}°"),
                ]),
        };

        return new DiagnosticForm
        {
            MedTestId             = test.MedTestId,
            PatientId             = test.PatientId,
            ExaminationDate       = test.ExaminationDate,
            SurveyName            = test.MedTestDefinitionKey,
            PatientNotes          = test.Description,
            Weight                = test.Weight,
            Height                = test.Growth,
            AgeYears              = patientAgeYears,
            PilsVariant           = awws.PilsVariant,
            PilsControlKey        = awws.PilsControlKey,
            Conclusion            = awws.Conclusion,
            ControlRecommendation = awws.ControlRecommendation,
            ParametersGroups      = paramGroups,
        };
    }
}
