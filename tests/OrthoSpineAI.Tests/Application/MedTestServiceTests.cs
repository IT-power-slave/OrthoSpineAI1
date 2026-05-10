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
        _service = new MedTestService(_repo, _engine);
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
    public async Task FinishTestAsync_MissingTest_ReturnsEmptyResult()
    {
        _repo.GetByIdAsync(0, default).Returns((MedTest?)null);

        var result = await _service.FinishTestAsync(0, 10);

        Assert.Equal(0, result.MedTestId);
        Assert.Contains("Brak danych", result.Conclusion);
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
}
