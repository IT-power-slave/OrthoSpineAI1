using OrthoSpineAI.Domain.Models;
using OrthoSpineAI.Domain.ValueObjects;

namespace OrthoSpineAI.Infrastructure.Devices;

/// <summary>
/// BLE device driver for ORT100 orthometr.
/// Production use: replace SimulateReceiveLoopAsync with P/Invoke to cdortometr.dll.
/// Thread-safe: FrameReceived is raised from background thread; callers must marshal to UI thread.
/// </summary>
public sealed class BleDeviceDriver : IDeviceDriver
{
    private string _macAddress = string.Empty;
    private CancellationTokenSource? _cts;
    private volatile bool _isConnected;
    private bool _disposed;
    private readonly object _lock = new();
    private readonly Random _rng = new();

    public event EventHandler<DeviceFrame>? FrameReceived;

    public bool IsConnected => _isConnected;

    public string Initialize(string macAddress)
    {
        _macAddress = macAddress;
        // TODO: P/Invoke → CDClientOrtometr.SetMacAddress(_macAddress)
        return string.Empty; // empty = success
    }

    public bool Start(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_isConnected) return true;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _isConnected = true;
        }
        _ = Task.Run(() => SimulateReceiveLoopAsync(_cts.Token), _cts.Token);
        return true;
    }

    public bool Stop()
    {
        lock (_lock)
        {
            if (!_isConnected) return true;
            _cts?.Cancel();
            _isConnected = false;
        }
        return true;
    }

    public void SendConfig(DeviceConfig config)
    {
        // TODO: P/Invoke → CDClientOrtometr.SendConfig(ref cfgFrame)
        // cfgFrame.mode_set    = (uint)config.Mode
        // cfgFrame.b_zero_angle     = config.ZeroAngle
        // cfgFrame.b_zero_angle_def = config.ZeroAngleDef
        // cfgFrame.b_zero_way       = config.ZeroWay
        // cfgFrame.text_up     = ASCII-padded config.TextUp
    }

    /// <summary>
    /// Simulates 10 Hz BLE frame stream. Replace with real DLL frame decoding.
    /// Raises FrameReceived on background thread — consumers must dispatch to UI thread.
    /// </summary>
    private async Task SimulateReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var frame = BuildSimulatedFrame();
                FrameReceived?.Invoke(this, frame);
                await Task.Delay(100, ct); // 10 Hz
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _isConnected = false;
        }
    }

    private DeviceFrame BuildSimulatedFrame()
    {
        double roll = Math.Round(_rng.NextDouble() * 90 - 45, 2);
        double tilt = Math.Round(_rng.NextDouble() * 20 - 10, 2);
        int way = _rng.Next(0, 600);

        return new DeviceFrame
        {
            Status = new DeviceStatus(0),
            Signal = _rng.Next(-90, -40),
            Battery = 3.7 + _rng.NextDouble() * 0.5,
            Shake = Math.Round(_rng.NextDouble() * 0.1, 3),
            Roll = roll,
            RollOffset = 0,
            Tilt = tilt,
            Way = way,
            Space = 120,
            Force1 = 0,
            Force2 = 0,
            Timestamp = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        _cts?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
