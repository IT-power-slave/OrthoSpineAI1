using OrthoSpineAI.Application.Algorithm;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Tests.Application;

public class PGLogicLegsStaticsTests
{
    private readonly PGLogicLegsStatics _logic = new();

    private static IReadOnlyDictionary<string, object> Disturbed() =>
        new Dictionary<string, object>
        {
            [AwwsParams.LEGSSTAT_DISTURBED] = true,
            [AwwsParams.LEGSSTAT_CORRECT]   = false,
        };

    private static IReadOnlyDictionary<string, object> Correct() =>
        new Dictionary<string, object>
        {
            [AwwsParams.LEGSSTAT_DISTURBED] = false,
            [AwwsParams.LEGSSTAT_CORRECT]   = true,
        };

    [Fact]
    public void Perform_LegStaticsDisturbed_MarksStaticsGroup()
    {
        var result = _logic.Perform(Disturbed());
        Assert.True(result[AwwsGroup.StaticsDisordersOfTheLowerLimbs]);
        Assert.False(result[AwwsGroup.Healthy]);
    }

    [Fact]
    public void Perform_LegStaticsCorrect_MarksHealthy()
    {
        var result = _logic.Perform(Correct());
        Assert.True(result[AwwsGroup.Healthy]);
        Assert.False(result[AwwsGroup.StaticsDisordersOfTheLowerLimbs]);
    }

    [Fact]
    public void Perform_AnyInput_BackAndRiskGroupsAlwaysTrue()
    {
        var result = _logic.Perform(Disturbed());
        Assert.True(result[AwwsGroup.FlatBack]);
        Assert.True(result[AwwsGroup.KyphoticBack]);
        Assert.True(result[AwwsGroup.LordoticBack]);
        Assert.True(result[AwwsGroup.IS_LowRiskGroup]);
        Assert.True(result[AwwsGroup.IS_MediumRiskGroup]);
        Assert.True(result[AwwsGroup.IS_HighRiskGroup]);
    }
}
