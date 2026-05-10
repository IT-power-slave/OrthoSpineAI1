using OrthoSpineAI.Application.Algorithm;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Tests.Application;

public class PGLogicBeightonScaleNumericTests
{
    private readonly PGLogicBeightonScaleNumeric _logic = new();

    private static IReadOnlyDictionary<string, object> Params(int age, int beighton) =>
        new Dictionary<string, object>
        {
            [AwwsParams.AGE]      = age,
            [AwwsParams.BEIGHTON] = beighton,
        };

    // ── IS_HighRiskGroup is always true ───────────────────────────────────────

    [Theory]
    [InlineData(5, 0)]
    [InlineData(10, 9)]
    [InlineData(18, 0)]
    public void Perform_AnyInput_HighRiskAlwaysTrue(int age, int beighton)
    {
        var result = _logic.Perform(Params(age, beighton));
        Assert.True(result[AwwsGroup.IS_HighRiskGroup]);
    }

    // ── Healthy group (age 5–15: beighton >= 4) ───────────────────────────────

    [Theory]
    [InlineData(8, 4)]
    [InlineData(15, 9)]
    public void Perform_YoungWithBeightonGe4_MarksHealthy(int age, int beighton)
    {
        var result = _logic.Perform(Params(age, beighton));
        Assert.True(result[AwwsGroup.Healthy]);
    }

    [Theory]
    [InlineData(8, 3)]
    [InlineData(8, 0)]
    public void Perform_YoungWithBeightonBelow4_NotHealthy(int age, int beighton)
    {
        var result = _logic.Perform(Params(age, beighton));
        Assert.False(result[AwwsGroup.Healthy]);
    }

    // ── Healthy group (age 16–18: beighton >= 3) ─────────────────────────────

    [Theory]
    [InlineData(16, 3)]
    [InlineData(18, 9)]
    public void Perform_OlderWithBeightonGe3_MarksHealthy(int age, int beighton)
    {
        var result = _logic.Perform(Params(age, beighton));
        Assert.True(result[AwwsGroup.Healthy]);
    }

    [Fact]
    public void Perform_OlderWithBeightonBelow3_NotHealthy()
    {
        var result = _logic.Perform(Params(17, 2));
        Assert.False(result[AwwsGroup.Healthy]);
    }

    // ── Groups that are always true regardless of input ───────────────────────

    [Fact]
    public void Perform_AnyInput_StaticsAndBackGroupsAlwaysTrue()
    {
        var result = _logic.Perform(Params(10, 0));
        Assert.True(result[AwwsGroup.StaticsDisordersOfTheLowerLimbs]);
        Assert.True(result[AwwsGroup.FlatBack]);
        Assert.True(result[AwwsGroup.KyphoticBack]);
        Assert.True(result[AwwsGroup.LordoticBack]);
    }
}
