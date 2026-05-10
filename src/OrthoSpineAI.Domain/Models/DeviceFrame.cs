using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Domain.ValueObjects;

namespace OrthoSpineAI.Domain.Models;

/// <summary>
/// Decoded BLE data frame from the ORT100 device (SOrtometrDataFrame).
/// All float fields are IEEE 754 little-endian as received from hardware.
/// </summary>
public sealed class DeviceFrame
{
    public DeviceStatus Status { get; init; } = new DeviceStatus(0);
    public int Signal { get; init; }
    public double Battery { get; init; }
    public double Shake { get; init; }
    public double Roll { get; init; }
    public double RollOffset { get; init; }
    public double Tilt { get; init; }
    public int Way { get; init; }
    public int Space { get; init; }
    public double Force1 { get; init; }
    public double Force2 { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
