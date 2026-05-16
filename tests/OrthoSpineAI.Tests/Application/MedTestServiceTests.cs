using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrthoSpineAI.Application.Algorithm;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Services;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Tests.Application;

public class MedTestServiceTests
{
    private readonly IMedTestRepository _repo = Substitute.For<IMedTestRepository>();
    private readonly AwwsEngine _engine = new();
    private readonly MedTestService _service;

    public MedTestServiceTests()
    {
        _service = new MedTestService(_repo, _engine, NullLogger<MedTestService>.Instance);
    }

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistsAndReturnsMappedDto()
    {
        var dto = new CreateMedTestDto(
            Description: "Test",
            MedTestDefinitionKey: "backbone",
            Weight: 50.0,
            Growth: 160.0,
            Beighton: 2,
            TestPP: false,
            KneeValgus: false,
            TarsalValgus: false,
            GaitDisturbance: false,
            PatientId: 1,
            SystemUserId: 1);

        var result = await _service.CreateAsync(dto);

        await _repo.Received(1).AddAsync(Arg.Any<MedTest>(), default);
        await _repo.Received(1).SaveChangesAsync(default);
        Assert.Equal("backbone", result.MedTestDefinitionKey);
        Assert.Equal(1, result.PatientId);
    }

    // ── GetByPatientAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetByPatientAsync_ReturnsMappedDtos()
    {
        _repo.GetByPatientIdAsync(5, default).Returns(new List<MedTest>
        {
            new() { MedTestId = 1, PatientId = 5, MedTestDefinitionKey = "backbone",
                    ExaminationDate = DateTime.UtcNow, Results = [], ContinuousResults = [] },
            new() { MedTestId = 2, PatientId = 5, MedTestDefinitionKey = "posture",
                    ExaminationDate = DateTime.UtcNow, Results = [], ContinuousResults = [] },
        });

        var result = await _service.GetByPatientAsync(5);

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(5, t.PatientId));
    }

    [Fact]
    public async Task GetByPatientAsync_NoTests_ReturnsEmpty()
    {
        _repo.GetByPatientIdAsync(99, default).Returns(new List<MedTest>());

        var result = await _service.GetByPatientAsync(99);

        Assert.Empty(result);
    }

    // ── FinishTestAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task FinishTestAsync_MissingTest_ThrowsMedTestNotFoundException()
    {
        _repo.GetByIdAsync(0, default).Returns((MedTest?)null);

        await Assert.ThrowsAsync<OrthoSpineAI.Domain.Exceptions.MedTestNotFoundException>(
            () => _service.FinishTestAsync(0, 10));
    }

    [Fact]
    public async Task FinishTestAsync_ValidTest_PersistsAwwsResultAndReturnsDto()
    {
        var test = new MedTest
        {
            MedTestId = 7,
            PatientId = 1,
            MedTestDefinitionKey = "backbone",
            ExaminationDate = new DateTime(2025, 1, 10),
            Weight = 45,
            Growth = 155,
            Beighton = 1,
            TestPP = false,
            KneeValgus = false,
            TarsalValgus = false,
            Results = new List<MedTestResult>
            {
                new() { OrtMeas = ORT100Measurement.MEAS_LL,    PhysicalValue = 30 },
                new() { OrtMeas = ORT100Measurement.MEAS_KP,    PhysicalValue = 25 },
                new() { OrtMeas = ORT100Measurement.MEAS_NM,    PhysicalValue = 3  },
                new() { OrtMeas = ORT100Measurement.MEAS_AC7,   PhysicalValue = 2  },
                new() { OrtMeas = ORT100Measurement.MEAS_AT6,   PhysicalValue = 1  },
                new() { OrtMeas = ORT100Measurement.MEAS_AT12,  PhysicalValue = 1  },
                new() { OrtMeas = ORT100Measurement.MEAS_AL3,   PhysicalValue = 1  },
                new() { OrtMeas = ORT100Measurement.MEAS_ASIPS, PhysicalValue = 1  },
            },
            ContinuousResults = []
        };
        _repo.GetByIdAsync(7, default).Returns(test);

        var result = await _service.FinishTestAsync(7, 10);

        await _repo.Received(1).SaveAwwsResultAsync(Arg.Any<AwwsResult>(), default);
        Assert.Equal(7, result.MedTestId);
        Assert.Equal("backbone", result.SurveyName);
        Assert.NotNull(result.Conclusion);
    }

    // ── GetDashboardAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboardAsync_ReturnsCounts()
    {
        _repo.CountTodayAsync(default).Returns(3);
        _repo.CountThisMonthAsync(default).Returns(15);
        _repo.GetRecentAsync(8, default).Returns(new List<MedTest>());

        var result = await _service.GetDashboardAsync(patientCount: 42);

        Assert.Equal(42, result.TotalPatients);
        Assert.Equal(3, result.TestsToday);
        Assert.Equal(15, result.TestsThisMonth);
        Assert.Empty(result.RecentTests);
    }

    // ── Gap #7: FLLD / LegsStatics mapping ────────────────────────────────

    private MedTest MakeTest(bool testPP, bool kneeValgus, bool tarsalValgus) => new()
    {
        MedTestId = 10,
        PatientId = 1,
        MedTestDefinitionKey = "backbone",
        ExaminationDate = DateTime.UtcNow,
        Weight = 50, Growth = 160, Beighton = 0,
        TestPP = testPP, KneeValgus = kneeValgus, TarsalValgus = tarsalValgus,
        Results = [], ContinuousResults = []
    };

    [Fact]
    public async Task FinishTestAsync_TestPPTrue_FlldPositiveAndNegativeFalse()
    {
        // TestPP=true → FLLD_POSITIVE, regardless of KneeValgus/TarsalValgus
        var test = MakeTest(testPP: true, kneeValgus: false, tarsalValgus: false);
        _repo.GetByIdAsync(10, default).Returns(test);

        var result = await _service.FinishTestAsync(10, 12);

        // FLLD_POSITIVE drives StaticsDisordersOfTheLowerLimbs → test should complete without error
        Assert.NotNull(result.Conclusion);
        await _repo.Received(1).SaveAwwsResultAsync(Arg.Any<AwwsResult>(), default);
    }

    [Fact]
    public async Task FinishTestAsync_TestPPFalse_FlldNegative()
    {
        // TestPP=false → FLLD_NEGATIVE, no statics disorder from FLLD
        var test = MakeTest(testPP: false, kneeValgus: false, tarsalValgus: false);
        _repo.GetByIdAsync(10, default).Returns(test);

        var result = await _service.FinishTestAsync(10, 12);

        Assert.NotNull(result.Conclusion);
    }

    [Fact]
    public async Task FinishTestAsync_KneeValgusTrue_LegsStatDisturbed_NotAffectedByTestPP()
    {
        // KneeValgus=true, TestPP=false → LEGSSTAT_DISTURBED=true (TestPP must NOT influence this)
        var test = MakeTest(testPP: false, kneeValgus: true, tarsalValgus: false);
        _repo.GetByIdAsync(10, default).Returns(test);

        var result = await _service.FinishTestAsync(10, 12);

        Assert.NotNull(result.Conclusion);
        await _repo.Received(1).SaveAwwsResultAsync(Arg.Any<AwwsResult>(), default);
    }

    [Fact]
    public async Task FinishTestAsync_NoValgus_LegsStatCorrect_EvenIfTestPPTrue()
    {
        // TestPP=true, KneeValgus=false, TarsalValgus=false → LEGSSTAT_CORRECT=true
        var test = MakeTest(testPP: true, kneeValgus: false, tarsalValgus: false);
        _repo.GetByIdAsync(10, default).Returns(test);

        var result = await _service.FinishTestAsync(10, 12);

        // Should not throw and should produce a valid result
        Assert.NotNull(result.Conclusion);
    }

    // ── Gap #19: BuildDiagnosticFormAsync ─────────────────────────────────

    [Fact]
    public async Task BuildDiagnosticFormAsync_ReturnsNull_WhenTestNotFound()
    {
        _repo.GetByIdAsync(999, default).Returns((MedTest?)null);
        _repo.GetAwwsResultAsync(999, default).Returns((AwwsResult?)null);

        var form = await _service.BuildDiagnosticFormAsync(999, 12);

        Assert.Null(form);
    }

    [Fact]
    public async Task BuildDiagnosticFormAsync_ReturnsNull_WhenAwwsResultNotFound()
    {
        var test = MakeTest(testPP: false, kneeValgus: false, tarsalValgus: false);
        _repo.GetByIdAsync(10, default).Returns(test);
        _repo.GetAwwsResultAsync(10, default).Returns((AwwsResult?)null);

        var form = await _service.BuildDiagnosticFormAsync(10, 12);

        Assert.Null(form);
    }

    [Fact]
    public async Task BuildDiagnosticFormAsync_PopulatesSessionMetadata()
    {
        var test = MakeTest(testPP: false, kneeValgus: false, tarsalValgus: false);
        test.Description = "notatka testowa";
        _repo.GetByIdAsync(10, default).Returns(test);
        _repo.GetAwwsResultAsync(10, default).Returns(MakeAwwsResult(pilsVariant: 2, controlKey: 3));

        var form = await _service.BuildDiagnosticFormAsync(10, 14);

        Assert.NotNull(form);
        Assert.Equal(10, form.MedTestId);
        Assert.Equal(1, form.PatientId);
        Assert.Equal("backbone", form.SurveyName);
        Assert.Equal("notatka testowa", form.PatientNotes);
        Assert.Equal(14, form.AgeYears);
        Assert.Equal(50.0, form.Weight);
        Assert.Equal(160.0, form.Height);
    }

    [Fact]
    public async Task BuildDiagnosticFormAsync_PopulatesAwwsOutcome()
    {
        var test = MakeTest(testPP: false, kneeValgus: false, tarsalValgus: false);
        _repo.GetByIdAsync(10, default).Returns(test);
        _repo.GetAwwsResultAsync(10, default).Returns(MakeAwwsResult(pilsVariant: 3, controlKey: 2,
            conclusion: "Wniosek testowy", recommendation: "Zalecenie testowe"));

        var form = await _service.BuildDiagnosticFormAsync(10, 10);

        Assert.Equal(3, form!.PilsVariant);
        Assert.Equal(2, form.PilsControlKey);
        Assert.Equal("Wniosek testowy", form.Conclusion);
        Assert.Equal("Zalecenie testowe", form.ControlRecommendation);
    }

    [Fact]
    public async Task BuildDiagnosticFormAsync_ContainsAllSevenParameterGroups()
    {
        var test = MakeTest(testPP: true, kneeValgus: true, tarsalValgus: false);
        _repo.GetByIdAsync(10, default).Returns(test);
        _repo.GetAwwsResultAsync(10, default).Returns(MakeAwwsResult());

        var form = await _service.BuildDiagnosticFormAsync(10, 12);

        Assert.NotNull(form);
        Assert.Equal(7, form.ParametersGroups.Count);
        var groupNames = form.ParametersGroups.Select(g => g.GroupName).ToList();
        Assert.Contains("PGLogicAnthropometric", groupNames);
        Assert.Contains("PGLogicAtr", groupNames);
        Assert.Contains("PGLogicBeightonScaleNumeric", groupNames);
        Assert.Contains("PGLogicFLLD", groupNames);
        Assert.Contains("PGLogicLegsStatics", groupNames);
        Assert.Contains("PGLogicLLTHK", groupNames);
        Assert.Contains("PGLogicPT", groupNames);
    }

    [Fact]
    public async Task BuildDiagnosticFormAsync_AnthropometricGroup_ContainsAgeHeightWeight()
    {
        var test = MakeTest(testPP: false, kneeValgus: false, tarsalValgus: false);
        _repo.GetByIdAsync(10, default).Returns(test);
        _repo.GetAwwsResultAsync(10, default).Returns(MakeAwwsResult());

        var form = await _service.BuildDiagnosticFormAsync(10, 15);

        var anthro = form!.ParametersGroups.Single(g => g.GroupName == "PGLogicAnthropometric");
        var keys = anthro.Parameters.Select(p => p.Key).ToList();
        Assert.Contains("AGE", keys);
        Assert.Contains("HEIGHT", keys);
        Assert.Contains("WEIGHT", keys);
        Assert.Equal("15 lat", anthro.Parameters.Single(p => p.Key == "AGE").Value);
        Assert.Equal("160 cm", anthro.Parameters.Single(p => p.Key == "HEIGHT").Value);
        Assert.Equal("50 kg",  anthro.Parameters.Single(p => p.Key == "WEIGHT").Value);
    }

    [Fact]
    public async Task BuildDiagnosticFormAsync_FlldGroup_ReflectsTestPP()
    {
        var test = MakeTest(testPP: true, kneeValgus: false, tarsalValgus: false);
        _repo.GetByIdAsync(10, default).Returns(test);
        _repo.GetAwwsResultAsync(10, default).Returns(MakeAwwsResult());

        var form = await _service.BuildDiagnosticFormAsync(10, 10);

        var flld = form!.ParametersGroups.Single(g => g.GroupName == "PGLogicFLLD");
        Assert.Equal("Tak", flld.Parameters.Single(p => p.Key == "FLLD_POSITIVE").Value);
        Assert.Equal("Nie", flld.Parameters.Single(p => p.Key == "FLLD_NEGATIVE").Value);
    }

    [Fact]
    public async Task BuildDiagnosticFormAsync_LegsStaticsGroup_ReflectsValgus()
    {
        var test = MakeTest(testPP: false, kneeValgus: true, tarsalValgus: false);
        _repo.GetByIdAsync(10, default).Returns(test);
        _repo.GetAwwsResultAsync(10, default).Returns(MakeAwwsResult());

        var form = await _service.BuildDiagnosticFormAsync(10, 10);

        var legs = form!.ParametersGroups.Single(g => g.GroupName == "PGLogicLegsStatics");
        Assert.Equal("Tak", legs.Parameters.Single(p => p.Key == "LEGSSTAT_DISTURBED").Value);
        Assert.Equal("Tak", legs.Parameters.Single(p => p.Key == "KneeValgus").Value);
        Assert.Equal("Nie", legs.Parameters.Single(p => p.Key == "TarsalValgus").Value);
    }

    [Fact]
    public async Task BuildDiagnosticFormAsync_GroupActiveState_MatchesStoredGroupResults()
    {
        var test = MakeTest(testPP: false, kneeValgus: false, tarsalValgus: false);
        _repo.GetByIdAsync(10, default).Returns(test);
        // PGLogicFLLD active, PGLogicAtr not active
        var awws = MakeAwwsResult();
        awws.GroupResultsJson = System.Text.Json.JsonSerializer.Serialize(
            new Dictionary<string, bool>
            {
                ["PGLogicFLLD"] = true,
                ["PGLogicAtr"]  = false,
            });
        _repo.GetAwwsResultAsync(10, default).Returns(awws);

        var form = await _service.BuildDiagnosticFormAsync(10, 10);

        Assert.True(form!.ParametersGroups.Single(g => g.GroupName == "PGLogicFLLD").IsActive);
        Assert.False(form.ParametersGroups.Single(g => g.GroupName == "PGLogicAtr").IsActive);
    }

    private static AwwsResult MakeAwwsResult(
        int pilsVariant = 1, int controlKey = 0,
        string conclusion = "Wynik OK", string recommendation = "Brak") => new()
    {
        MedTestId             = 10,
        PilsVariant           = pilsVariant,
        PilsControlKey        = controlKey,
        Conclusion            = conclusion,
        ControlRecommendation = recommendation,
        GroupResultsJson      = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, bool>()),
    };
}
