namespace OrthoSpineAI.Domain.Models;

public interface IDeviceDriver : IDisposable
{
    /// <summary>Sets MAC address. Returns empty string on success, error message otherwise.</summary>
    string Initialize(string macAddress);

    /// <summary>Starts BLE session and background receive loop.</summary>
    bool Start(CancellationToken cancellationToken = default);

    /// <summary>Stops BLE session and terminates background loop.</summary>
    bool Stop();

    /// <summary>Sends configuration frame to device (mode, zeroing flags, OLED text).</summary>
    void SendConfig(DeviceConfig config);

    /// <summary>Raised on every received telemetry frame. Thread-safe; handlers must marshal to UI thread.</summary>
    event EventHandler<DeviceFrame> FrameReceived;

    bool IsConnected { get; }
}
