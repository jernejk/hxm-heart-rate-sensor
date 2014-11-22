namespace HeartRateSensor.Shared.Core.Data
{
    public class HeartBeatSensorData
    {
        public bool IsAvailable { get; set; }

        public bool IsStrideAvailabe { get; set; }

        public string DeviceName { get; set; }

        public string Firmware { get; set; }

        public string Hardware { get; set; }

        public int HeartBeatPerMinute { get; set; }

        public int TotalHeartBeats { get; set; }

        public int Battery { get; set; }

        public int Stride { get; set; }

        public double Distance { get; set; }

        public double Speed { get; set; }
    }
}
