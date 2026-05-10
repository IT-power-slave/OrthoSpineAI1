using OrthoSpineAI.Application.Algorithm;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;
using System.Text.Json;

namespace OrthoSpineAI.Application.Services;

public class MedTestService : IMedTestService
{
    private readonly IMedTestRepository _repo;
    private readonly AwwsEngine _awwsEngine;

    public MedTestService(IMedTestRepository repo, AwwsEngine awwsEngine)
    {
        _repo = repo;
        _awwsEngine = awwsEngine;
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
            TestPP = dto.TestPP,
            KneeValgus = dto.KneeValgus,
            TarsalValgus = dto.TarsalValgus,
            GaitDisturbance = dto.GaitDisturbance,
            PatientId = dto.PatientId,
            SystemUserId = dto.SystemUserId
        };
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
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
            return EmptyResult();

        // Build parameter dictionary for AwwsEngine
        var p = new Dictionary<string, object>();

        // Scalar fields from MedTest
        p[AwwsParams.BEIGHTON]        = test.Beighton;
        p[AwwsParams.WEIGHT]          = (int)Math.Round(test.Weight);
        p[AwwsParams.HEIGHT]          = (int)Math.Round(test.Growth);
        p[AwwsParams.AGE]             = patientAgeYears;

        // Leg statics derived from examination flags
        p[AwwsParams.LEGSSTAT_DISTURBED] = test.KneeValgus || test.TarsalValgus || test.TestPP;
        p[AwwsParams.LEGSSTAT_CORRECT]   = !test.KneeValgus && !test.TarsalValgus && !test.TestPP;

        // Map saved measurements
        var results = test.Results;

        double Meas(ORT100Measurement key) =>
            results.FirstOrDefault(r => r.OrtMeas == key)?.PhysicalValue ?? 0.0;

        // Saggittal plane curvatures
        p[AwwsParams.LL]  = (int)Math.Round(Meas(ORT100Measurement.MEAS_LL));
        p[AwwsParams.THK] = (int)Math.Round(Meas(ORT100Measurement.MEAS_KP));
        p[AwwsParams.PT]  = (int)Math.Round(Meas(ORT100Measurement.MEAS_NM));

        // ATR: max of Adams test measurements (AC7..ASIPS)
        var atrValues = new[]
        {
            Meas(ORT100Measurement.MEAS_AC7),
            Meas(ORT100Measurement.MEAS_AT6),
            Meas(ORT100Measurement.MEAS_AT12),
            Meas(ORT100Measurement.MEAS_AL3),
            Meas(ORT100Measurement.MEAS_ASIPS)
        };
        int atrMax = (int)Math.Round(atrValues.Max());
        p[AwwsParams.ATR] = atrMax;
        p[AwwsParams.HS]  = atrMax; // HS ≈ ATR for simplified scoring

        // FLLD: derived from pelvic inclination (NM) and leg statics
        double nm = Meas(ORT100Measurement.MEAS_NM);
        p[AwwsParams.FLLD_POSITIVE] = nm > 5;
        p[AwwsParams.FLLD_NEGATIVE] = nm < -5;
        p[AwwsParams.FLLD_NEUTRAL]  = Math.Abs(nm) <= 5;

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
        "Brak danych — test nie został znaleziony.", string.Empty,
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
        t.Beighton, t.TestPP, t.KneeValgus, t.TarsalValgus,
        t.GaitDisturbance, t.PatientId, t.SystemUserId);
}
