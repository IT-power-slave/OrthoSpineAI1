using OrthoSpineAI.Application.Algorithm;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Tests.Application;

/// <summary>
/// Tests for <see cref="AwwsEngine"/> PiLS decision tree.
/// Each test maps to a named priority branch in <c>DeterminePilsVariant</c>.
/// </summary>
public class AwwsEngineTests
{
    private readonly AwwsEngine _engine = new();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Dictionary<string, object> BaseParams(
        int atr = 0,
        int beighton = 0,
        bool flldPos = false,
        bool flldNeg = false,
        int kp = 20,
        int ll = 20,
        int age = 8,
        int weight = 30,
        int height = 130) => new()
    {
        [AwwsParams.ATR]              = atr,
        [AwwsParams.HS]               = atr,
        [AwwsParams.BEIGHTON]         = beighton,
        [AwwsParams.FLLD_POSITIVE]    = flldPos,
        [AwwsParams.FLLD_NEGATIVE]    = flldNeg,
        [AwwsParams.FLLD_NEUTRAL]     = !flldPos && !flldNeg,
        [AwwsParams.THK]              = kp,
        [AwwsParams.LL]               = ll,
        [AwwsParams.AGE]              = age,
        [AwwsParams.WEIGHT]           = weight,
        [AwwsParams.HEIGHT]           = height,
        [AwwsParams.LEGSSTAT_CORRECT]   = true,
        [AwwsParams.LEGSSTAT_DISTURBED] = false,
        [AwwsParams.PT]               = 0,
    };

    // ── Variant 0 — insufficient data / healthy ───────────────────────────────

    [Fact]
    public void Evaluate_AllValuesNormal_ReturnsVariant0()
    {
        var p = BaseParams(atr: 0, beighton: 0);
        var result = _engine.Evaluate(p);
        Assert.Equal(0, result.PilsVariant);
        Assert.Equal(0, result.PilsControlKey);
    }

    // ── Variant 4 — ATR > 7° (highest priority) ───────────────────────────────

    [Theory]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(15)]
    public void Evaluate_AtrAbove7_ReturnsVariant4ControlKey6(int atr)
    {
        var p = BaseParams(atr: atr);
        var result = _engine.Evaluate(p);
        Assert.Equal(4, result.PilsVariant);
        Assert.Equal(6, result.PilsControlKey);
    }

    // ── Variant 3 — 5 < ATR ≤ 7 AND Beighton ≥ 6 ────────────────────────────

    [Theory]
    [InlineData(6, 6)]
    [InlineData(7, 9)]
    public void Evaluate_MediumAtrHighBeighton_ReturnsVariant3(int atr, int beighton)
    {
        var p = BaseParams(atr: atr, beighton: beighton);
        var result = _engine.Evaluate(p);
        Assert.Equal(3, result.PilsVariant);
        Assert.Equal(5, result.PilsControlKey);
    }

    // ── Variant 2 — 3 < ATR ≤ 5, Beighton ≤ 5, FLLD+ ────────────────────────

    [Fact]
    public void Evaluate_ModerateAtrFlldPos_YoungPatient_ReturnsVariant2ControlKey3()
    {
        var p = BaseParams(atr: 4, beighton: 3, flldPos: true, kp: 10, ll: 10, age: 8);
        var result = _engine.Evaluate(p);
        Assert.Equal(2, result.PilsVariant);
        Assert.Equal(3, result.PilsControlKey);
    }

    [Fact]
    public void Evaluate_ModerateAtrFlldPos_OlderPatient_ReturnsVariant2ControlKey4()
    {
        var p = BaseParams(atr: 4, beighton: 3, flldPos: true, kp: 10, ll: 10, age: 11);
        var result = _engine.Evaluate(p);
        Assert.Equal(2, result.PilsVariant);
        Assert.Equal(4, result.PilsControlKey);
    }

    // ── Variant 1 — 3 < ATR ≤ 5, Beighton ≤ 5, FLLD- ────────────────────────

    [Fact]
    public void Evaluate_ModerateAtrFlldNeg_YoungPatient_ReturnsVariant1ControlKey1()
    {
        var p = BaseParams(atr: 4, beighton: 3, flldNeg: true, kp: 25, ll: 25, age: 8);
        var result = _engine.Evaluate(p);
        Assert.Equal(1, result.PilsVariant);
        Assert.Equal(1, result.PilsControlKey);
    }

    [Fact]
    public void Evaluate_ModerateAtrFlldNeg_OlderPatient_ReturnsVariant1ControlKey2()
    {
        var p = BaseParams(atr: 4, beighton: 3, flldNeg: true, kp: 25, ll: 25, age: 11);
        var result = _engine.Evaluate(p);
        Assert.Equal(1, result.PilsVariant);
        Assert.Equal(2, result.PilsControlKey);
    }

    // ── Priority ordering ─────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_AtrAbove7_TakesPriorityOverBeighton()
    {
        // ATR > 7 should yield variant 4 even when Beighton ≥ 6
        var p = BaseParams(atr: 9, beighton: 8);
        var result = _engine.Evaluate(p);
        Assert.Equal(4, result.PilsVariant);
    }

    // ── Return value completeness ─────────────────────────────────────────────

    [Fact]
    public void Evaluate_AlwaysReturnsNonNullConclusionAndGroups()
    {
        var p = BaseParams();
        var result = _engine.Evaluate(p);
        Assert.NotNull(result.Conclusion);
        Assert.NotNull(result.GroupResults);
        Assert.NotEmpty(result.GroupResults);
    }

    [Fact]
    public void Evaluate_HighRiskVariant_HasNonEmptyControlRecommendation()
    {
        var p = BaseParams(atr: 9);
        var result = _engine.Evaluate(p);
        Assert.NotEmpty(result.ControlRecommendation);
    }
}
