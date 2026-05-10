using NSubstitute;
using OrthoSpineAI.Application.Services;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Tests.Application;

public class SurveyServiceTests
{
    private readonly ISurveyRepository _repo = Substitute.For<ISurveyRepository>();
    private readonly SurveyService _service;

    public SurveyServiceTests()
    {
        _service = new SurveyService(_repo);
    }

    // ── GetAllDefinitionsAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetAllDefinitionsAsync_ReturnsOnlyRootLevelKeys()
    {
        _repo.GetAllDefinitionsAsync(default).Returns(new List<MedTestDefinition>
        {
            new() { MedTestDefinitionId = 1, Key = "backbone",   Name = "Backbone",   Stages = [] },
            new() { MedTestDefinitionId = 2, Key = "backbone.1", Name = "Stage 1",    Stages = [] },
            new() { MedTestDefinitionId = 3, Key = "posture",    Name = "Posture",    Stages = [] },
        });

        var result = await _service.GetAllDefinitionsAsync();

        Assert.Equal(2, result.Count);
        Assert.All(result, d => Assert.DoesNotContain('.', d.Key));
    }

    [Fact]
    public async Task GetAllDefinitionsAsync_EmptyRepository_ReturnsEmpty()
    {
        _repo.GetAllDefinitionsAsync(default).Returns(new List<MedTestDefinition>());

        var result = await _service.GetAllDefinitionsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllDefinitionsAsync_MapsNameAndIdCorrectly()
    {
        _repo.GetAllDefinitionsAsync(default).Returns(new List<MedTestDefinition>
        {
            new() { MedTestDefinitionId = 42, Key = "spine", Name = "Spine Test", Stages = [] },
        });

        var result = await _service.GetAllDefinitionsAsync();

        Assert.Single(result);
        Assert.Equal(42, result[0].MedTestDefinitionId);
        Assert.Equal("Spine Test", result[0].Name);
        Assert.Equal("spine", result[0].Key);
    }

    // ── GetSurveyGroupAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetSurveyGroupAsync_ReturnsAllMatchingDefinitions()
    {
        _repo.GetDefinitionsByKeyPrefixAsync("backbone", default).Returns(new List<MedTestDefinition>
        {
            new() { MedTestDefinitionId = 1, Key = "backbone",   Name = "Backbone",   Stages = [] },
            new() { MedTestDefinitionId = 2, Key = "backbone.1", Name = "Step 1",     Stages = [] },
            new() { MedTestDefinitionId = 3, Key = "backbone.2", Name = "Step 2",     Stages = [] },
        });

        var result = await _service.GetSurveyGroupAsync("backbone");

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetSurveyGroupAsync_MapsStagesCorrectly()
    {
        var stage = new MedTestStage
        {
            MedTestStageId = 10,
            Name = "Stage A",
            Tip = "tip",
            TipControl = "tipCtrl",
            MainSurveyControl = "ctrl",
            MedTestDefinitionId = 1
        };
        _repo.GetDefinitionsByKeyPrefixAsync("spine", default).Returns(new List<MedTestDefinition>
        {
            new() { MedTestDefinitionId = 1, Key = "spine", Name = "Spine", Stages = [stage] },
        });

        var result = await _service.GetSurveyGroupAsync("spine");

        Assert.Single(result);
        Assert.Single(result[0].Stages);
        Assert.Equal(10, result[0].Stages[0].MedTestStageId);
        Assert.Equal("Stage A", result[0].Stages[0].Name);
    }

    [Fact]
    public async Task GetSurveyGroupAsync_NoMatch_ReturnsEmpty()
    {
        _repo.GetDefinitionsByKeyPrefixAsync("unknown", default).Returns(new List<MedTestDefinition>());

        var result = await _service.GetSurveyGroupAsync("unknown");

        Assert.Empty(result);
    }
}
