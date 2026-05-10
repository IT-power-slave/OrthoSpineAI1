using OrthoSpineAI.Application.Algorithm;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Tests.Application;

public class PGLogicPTTests
{
    private readonly PGLogicPT _logic = new();

    private static IReadOnlyDictionary<string, object> Params(int pt) =>
        new Dictionary<string, object> { [AwwsParams.PT] = pt };

    // Healthy: PT in [10, 29]
    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(29)]
    public void Perform_PtInHealthyRange_MarksHealthy(int pt)
    {
        var result = _logic.Perform(Params(pt));
        Assert.True(result[AwwsGroup.Healthy]);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(30)]
    [InlineData(50)]
    public void Perform_PtOutsideHealthyRange_NotHealthy(int pt)
    {
        var result = _logic.Perform(Params(pt));
        Assert.False(result[AwwsGroup.Healthy]);
    }

    // FlatBack: PT in [5, 20]
    [Theory]
    [InlineData(5)]
    [InlineData(15)]
    [InlineData(20)]
    public void Perform_PtInFlatBackRange_MarksFlatBack(int pt)
    {
        var result = _logic.Perform(Params(pt));
        Assert.True(result[AwwsGroup.FlatBack]);
    }

    // KyphoticBack: PT in [10, 30]
    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(30)]
    public void Perform_PtInKyphoticRange_MarksKyphoticBack(int pt)
    {
        var result = _logic.Perform(Params(pt));
        Assert.True(result[AwwsGroup.KyphoticBack]);
    }

    // LordoticBack: PT in [20, 40]
    [Theory]
    [InlineData(20)]
    [InlineData(30)]
    [InlineData(40)]
    public void Perform_PtInLordoticRange_MarksLordoticBack(int pt)
    {
        var result = _logic.Perform(Params(pt));
        Assert.True(result[AwwsGroup.LordoticBack]);
    }

    [Fact]
    public void Perform_AnyInput_StaticsAndHighRiskAlwaysTrue()
    {
        var result = _logic.Perform(Params(0));
        Assert.True(result[AwwsGroup.IS_HighRiskGroup]);
        Assert.True(result[AwwsGroup.StaticsDisordersOfTheLowerLimbs]);
    }
}
