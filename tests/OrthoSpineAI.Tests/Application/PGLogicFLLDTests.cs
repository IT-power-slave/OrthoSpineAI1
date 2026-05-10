using OrthoSpineAI.Application.Algorithm;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Tests.Application;

public class PGLogicFLLDTests
{
    private readonly PGLogicFLLD _logic = new();

    private static IReadOnlyDictionary<string, object> Positive() =>
        new Dictionary<string, object>
        {
            [AwwsParams.FLLD_POSITIVE] = true,
            [AwwsParams.FLLD_NEGATIVE] = false,
        };

    private static IReadOnlyDictionary<string, object> Negative() =>
        new Dictionary<string, object>
        {
            [AwwsParams.FLLD_POSITIVE] = false,
            [AwwsParams.FLLD_NEGATIVE] = true,
        };

    [Fact]
    public void Perform_FlldPositive_MarksStaticsAndRiskGroups()
    {
        var result = _logic.Perform(Positive());

        Assert.True(result[AwwsGroup.StaticsDisordersOfTheLowerLimbs]);
        Assert.True(result[AwwsGroup.IS_LowRiskGroup]);
        Assert.True(result[AwwsGroup.IS_MediumRiskGroup]);
        Assert.False(result[AwwsGroup.Healthy]);
    }

    [Fact]
    public void Perform_FlldNegative_MarksHealthy()
    {
        var result = _logic.Perform(Negative());

        Assert.True(result[AwwsGroup.Healthy]);
        Assert.False(result[AwwsGroup.StaticsDisordersOfTheLowerLimbs]);
    }

    [Fact]
    public void Perform_AnyInput_BackAndHighRiskAlwaysTrue()
    {
        var result = _logic.Perform(Positive());

        Assert.True(result[AwwsGroup.FlatBack]);
        Assert.True(result[AwwsGroup.KyphoticBack]);
        Assert.True(result[AwwsGroup.LordoticBack]);
        Assert.True(result[AwwsGroup.IS_HighRiskGroup]);
    }
}
