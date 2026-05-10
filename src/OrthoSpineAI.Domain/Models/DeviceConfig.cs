using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Domain.Models;

/// <summary>
/// Configuration frame sent to the ORT100 device (SOrtometrCfgFrame).
/// </summary>
public sealed class DeviceConfig
{
    public ORT100Mode Mode { get; init; }
    public bool ZeroAngle { get; init; }
    public bool ZeroAngleDef { get; init; }
    public bool ZeroWay { get; init; }

    /// <summary>Upper OLED display text — max 10 ASCII characters, padded with spaces.</summary>
    public string TextUp { get; init; } = string.Empty;

    /// <summary>Lower OLED display text — max 10 ASCII characters, padded with spaces.</summary>
    public string TextDown { get; init; } = string.Empty;

    public static DeviceConfig FromResetFlag(ORT100Mode mode, ORT100ResetFlag reset, string textUp = "", string textDown = "")
    {
        return new DeviceConfig
        {
            Mode = mode,
            ZeroAngle = reset == ORT100ResetFlag.ZERO_ANGLE || reset == ORT100ResetFlag.ZERO_WAY_ANGLE,
            ZeroAngleDef = reset == ORT100ResetFlag.ZERO_ANGLE_DEF || reset == ORT100ResetFlag.ZERO_WAY_ANGLE_DEF,
            ZeroWay = reset == ORT100ResetFlag.ZERO_WAY || reset == ORT100ResetFlag.ZERO_WAY_ANGLE || reset == ORT100ResetFlag.ZERO_WAY_ANGLE_DEF,
            TextUp = textUp.PadRight(10)[..10],
            TextDown = textDown.PadRight(10)[..10]
        };
    }
}
