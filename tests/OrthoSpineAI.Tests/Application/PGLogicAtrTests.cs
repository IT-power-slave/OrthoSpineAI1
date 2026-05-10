using OrthoSpineAI.Application.Algorithm;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Tests.Application;

public class PGLogicAtrTests
{
    private static IReadOnlyDictionary<string, object> Params(int atr, int hs) =>
        new Dictionary<string, object>
        {
            [AwwsParams.ATR] = atr,
            [AwwsParams.HS]  = hs,
        };

    private readonly PGLogicAtr _logic = new();

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 0)]
    [InlineData(2, 3)]
    public void Perform_LowAtrAndHs_MarksHealthy(int atr, int hs)
    {
        var result = _logic.Perform(Params(atr, hs));
        Assert.True(result[AwwsGroup.Healthy]);
    }

    [Theory]
    [InlineData(3, 0)]
    [InlineData(4, 0)]
    [InlineData(0, 4)]
    [InlineData(0, 5)]
    public void Perform_MediumAtrOrHs_MarksLowRisk(int atr, int hs)
    {
        var result = _logic.Perform(Params(atr, hs));
        Assert.True(result[AwwsGroup.IS_LowRiskGroup]);
        Assert.False(result[AwwsGroup.Healthy]);
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(6, 0)]
    [InlineData(0, 6)]
    [InlineData(0, 7)]
    public void Perform_ElevatedAtrOrHs_MarksMediumRisk(int atr, int hs)
    {
        var result = _logic.Perform(Params(atr, hs));
        Assert.True(result[AwwsGroup.IS_MediumRiskGroup]);
        Assert.False(result[AwwsGroup.Healthy]);
        Assert.False(result[AwwsGroup.IS_LowRiskGroup]);
    }

    [Theory]
    [InlineData(7, 0)]
    [InlineData(10, 0)]
    [InlineData(0, 8)]
    [InlineData(0, 15)]
    public void Perform_HighAtrOrHs_MarksHighRisk(int atr, int hs)
    {
        var result = _logic.Perform(Params(atr, hs));
        Assert.True(result[AwwsGroup.IS_HighRiskGroup]);
        Assert.False(result[AwwsGroup.Healthy]);
    }

    [Fact]
    public void Perform_LowAtr_MarksAllNonScoliosisGroupsHealthy()
    {
        var result = _logic.Perform(Params(1, 1));
        Assert.True(result[AwwsGroup.Healthy]);
        Assert.True(result[AwwsGroup.FlatBack]);
        Assert.True(result[AwwsGroup.KyphoticBack]);
        Assert.True(result[AwwsGroup.LordoticBack]);
        Assert.True(result[AwwsGroup.StaticsDisordersOfTheLowerLimbs]);
    }

    [Fact]
    public void Perform_MissingKeys_DoesNotThrow()
    {
        var emptyParams = new Dictionary<string, object>();
        var ex = Record.Exception(() => _logic.Perform(emptyParams));
        Assert.Null(ex);
    }
}
