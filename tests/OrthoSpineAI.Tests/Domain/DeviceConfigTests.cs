using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.Models;

namespace OrthoSpineAI.Tests.Domain;

/// <summary>
/// Unit tests for DeviceConfig.FromResetFlag — verifies gap #15: OrtResetFlag zeroing
/// booleans are decoded correctly so the simulator (and real driver) apply the right offsets.
/// </summary>
public class DeviceConfigTests
{
    // ── NONE ────────────────────────────────────────────────────────────────

    [Fact]
    public void FromResetFlag_None_AllZeroFlagsFalse()
    {
        var cfg = DeviceConfig.FromResetFlag(ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE);

        Assert.False(cfg.ZeroAngle);
        Assert.False(cfg.ZeroAngleDef);
        Assert.False(cfg.ZeroWay);
    }

    // ── ZERO_ANGLE ───────────────────────────────────────────────────────────

    [Fact]
    public void FromResetFlag_ZeroAngle_SetsOnlyZeroAngle()
    {
        var cfg = DeviceConfig.FromResetFlag(ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_ANGLE);

        Assert.True(cfg.ZeroAngle);
        Assert.False(cfg.ZeroAngleDef);
        Assert.False(cfg.ZeroWay);
    }

    // ── ZERO_ANGLE_DEF ───────────────────────────────────────────────────────

    [Fact]
    public void FromResetFlag_ZeroAngleDef_SetsOnlyZeroAngleDef()
    {
        var cfg = DeviceConfig.FromResetFlag(ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_ANGLE_DEF);

        Assert.False(cfg.ZeroAngle);
        Assert.True(cfg.ZeroAngleDef);
        Assert.False(cfg.ZeroWay);
    }

    // ── ZERO_WAY ─────────────────────────────────────────────────────────────

    [Fact]
    public void FromResetFlag_ZeroWay_SetsOnlyZeroWay()
    {
        var cfg = DeviceConfig.FromResetFlag(ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY);

        Assert.False(cfg.ZeroAngle);
        Assert.False(cfg.ZeroAngleDef);
        Assert.True(cfg.ZeroWay);
    }

    // ── ZERO_WAY_ANGLE ───────────────────────────────────────────────────────

    [Fact]
    public void FromResetFlag_ZeroWayAngle_SetsZeroAngleAndZeroWay()
    {
        var cfg = DeviceConfig.FromResetFlag(ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE);

        Assert.True(cfg.ZeroAngle);
        Assert.False(cfg.ZeroAngleDef);
        Assert.True(cfg.ZeroWay);
    }

    // ── ZERO_WAY_ANGLE_DEF ───────────────────────────────────────────────────

    [Fact]
    public void FromResetFlag_ZeroWayAngleDef_SetsZeroAngleDefAndZeroWay()
    {
        var cfg = DeviceConfig.FromResetFlag(ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF);

        Assert.False(cfg.ZeroAngle);
        Assert.True(cfg.ZeroAngleDef);
        Assert.True(cfg.ZeroWay);
    }

    // ── Text truncation ──────────────────────────────────────────────────────

    [Fact]
    public void FromResetFlag_TextUp_TruncatedToTenChars()
    {
        var cfg = DeviceConfig.FromResetFlag(ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,
            textUp: "ABCDEFGHIJKLMNOP");

        Assert.Equal(10, cfg.TextUp.Length);
        Assert.Equal("ABCDEFGHIJ", cfg.TextUp);
    }

    [Fact]
    public void FromResetFlag_TextUp_PaddedToTenChars()
    {
        var cfg = DeviceConfig.FromResetFlag(ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,
            textUp: "AB");

        Assert.Equal(10, cfg.TextUp.Length);
        Assert.StartsWith("AB", cfg.TextUp);
    }
}
