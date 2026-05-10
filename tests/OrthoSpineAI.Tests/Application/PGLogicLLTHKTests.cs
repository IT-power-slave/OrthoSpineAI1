using OrthoSpineAI.Application.Algorithm;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Tests.Application;

public class PGLogicLLTHKTests
{
    private readonly PGLogicLLTHK _logic = new();

    private static IReadOnlyDictionary<string, object> Params(int age, int ll, int thk) =>
        new Dictionary<string, object>
        {
            [AwwsParams.AGE] = age,
            [AwwsParams.LL]  = ll,
            [AwwsParams.THK] = thk,
        };

    // --- Healthy ---
    // Age 6-12: LL in [20,45]; Age 13+: THK in [15,50]

    [Theory]
    [InlineData(8,  20, 0)]   // young, LL at lower bound
    [InlineData(10, 45, 0)]   // young, LL at upper bound
    [InlineData(15, 0,  15)]  // older, THK at lower bound
    [InlineData(17, 0,  50)]  // older, THK at upper bound
    public void Perform_InHealthyRange_MarksHealthy(int age, int ll, int thk)
    {
        var result = _logic.Perform(Params(age, ll, thk));
        Assert.True(result[AwwsGroup.Healthy]);
    }

    [Theory]
    [InlineData(8,  19, 0)]   // young, LL below range
    [InlineData(8,  46, 0)]   // young, LL above range
    [InlineData(16, 0,  14)]  // older, THK below range
    [InlineData(16, 0,  51)]  // older, THK above range
    public void Perform_OutsideHealthyRange_NotHealthy(int age, int ll, int thk)
    {
        var result = _logic.Perform(Params(age, ll, thk));
        Assert.False(result[AwwsGroup.Healthy]);
    }

    // --- FlatBack ---
    // Age 6-12: LL<=15 AND THK<=15; Age 13+: LL<19 AND THK<=19

    [Theory]
    [InlineData(8,  15, 15)]  // young, both at ceiling
    [InlineData(16, 18, 19)]  // older, both within
    public void Perform_FlatBackCondition_MarksFlatBack(int age, int ll, int thk)
    {
        var result = _logic.Perform(Params(age, ll, thk));
        Assert.True(result[AwwsGroup.FlatBack]);
    }

    // --- KyphoticBack ---
    // Age 6-12: LL<=15 AND THK>=46; Age 13+: LL<19 AND THK>50

    [Theory]
    [InlineData(8,  10, 46)]  // young
    [InlineData(16, 10, 51)]  // older
    public void Perform_KyphoticCondition_MarksKyphoticBack(int age, int ll, int thk)
    {
        var result = _logic.Perform(Params(age, ll, thk));
        Assert.True(result[AwwsGroup.KyphoticBack]);
    }

    // --- LordoticBack ---
    // Age 6-12: LL>=46 AND THK<15; Age 13+: LL>=50 AND THK<19

    [Theory]
    [InlineData(8,  46, 10)]  // young
    [InlineData(16, 50, 10)]  // older
    public void Perform_LordoticCondition_MarksLordoticBack(int age, int ll, int thk)
    {
        var result = _logic.Perform(Params(age, ll, thk));
        Assert.True(result[AwwsGroup.LordoticBack]);
    }

    [Fact]
    public void Perform_AnyInput_RiskGroupsAndStaticsAlwaysTrue()
    {
        var result = _logic.Perform(Params(10, 30, 30));
        Assert.True(result[AwwsGroup.StaticsDisordersOfTheLowerLimbs]);
        Assert.True(result[AwwsGroup.IS_LowRiskGroup]);
        Assert.True(result[AwwsGroup.IS_MediumRiskGroup]);
        Assert.True(result[AwwsGroup.IS_HighRiskGroup]);
    }
}
