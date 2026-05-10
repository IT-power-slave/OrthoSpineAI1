using System;

namespace OrthoSpineAI.Domain.Models
{
    public class DeviceMeasurement
    {
        public int Status { get; set; }
        public int SignalStrengthDb { get; set; }
        public double BatteryVoltage { get; set; }
        public double ShakeAcceleration { get; set; }
        public double RollAngle { get; set; }
        public double RollOffset { get; set; }
        public double TiltAngle { get; set; }
        public int WayMm { get; set; }
        public int SpaceMm { get; set; }
        public double Force1Newtons { get; set; }
        public double Force2Newtons { get; set; }
        public DateTime Timestamp { get; set; }

        public DeviceMeasurement()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
