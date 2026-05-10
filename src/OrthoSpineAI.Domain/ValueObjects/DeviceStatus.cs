namespace OrthoSpineAI.Domain.ValueObjects;

/// <summary>
/// Decoded status word from SOrtometrDataFrame.status (Int32 bitfield).
/// </summary>
public sealed class DeviceStatus
{
    public int RawValue { get; }

    public ORT100ModeRaw Mode => (ORT100ModeRaw)(RawValue & 0x1F);
    public bool ForceSensorConnected => (RawValue >> 5 & 1) == 1;
    public bool UsbConnected => (RawValue >> 6 & 1) == 1;
    public bool BatteryLow => (RawValue >> 7 & 1) == 1;
    public bool BatteryCharging => (RawValue >> 8 & 1) == 1;
    public bool BleError => (RawValue >> 9 & 1) == 1;
    public bool AccelerometerError => (RawValue >> 10 & 1) == 1;
    public bool WayError => (RawValue >> 11 & 1) == 1;
    public bool SpaceError => (RawValue >> 12 & 1) == 1;
    public bool OledError => (RawValue >> 13 & 1) == 1;
    public bool NextButton => (RawValue >> 14 & 1) == 1;
    public bool SampleButton => (RawValue >> 15 & 1) == 1;
    public bool CalButton => (RawValue >> 16 & 1) == 1;
    public bool PowerButton => (RawValue >> 17 & 1) == 1;
    public bool NextButtonLong => (RawValue >> 19 & 1) == 1;
    public bool SampleButtonLong => (RawValue >> 20 & 1) == 1;
    public bool CalButtonLong => (RawValue >> 21 & 1) == 1;
    public bool PowerButtonLong => (RawValue >> 22 & 1) == 1;

    public bool HasAnyError => BleError || AccelerometerError || WayError || SpaceError || OledError;

    public DeviceStatus(int rawValue)
    {
        RawValue = rawValue;
    }

    /// <summary>Raw mode bits (0–4) mapped directly from status word.</summary>
    public enum ORT100ModeRaw
    {
        Manual = 0,
        SeqA1 = 1,
        SeqA2 = 2,
        SeqA3 = 3,
        SeqA4 = 4,
        SeqLS1 = 5,
        SeqLS2 = 6,
        SeqLS3 = 7,
        SeqLS4 = 8,
        SeqLS5 = 9,
        SeqLB1 = 10,
        SeqLB2 = 11,
        SeqLB3 = 12,
        SeqLB4 = 13,
        SeqLB5 = 14,
        SeqAD1 = 15,
        SeqAD2 = 16,
        SeqAD3 = 17,
        SeqAD4 = 18,
        SeqAD5 = 19
    }
}
