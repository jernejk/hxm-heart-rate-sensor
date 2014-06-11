namespace HeartRateSensor.Shared.Core.Data
{
    public class HeartBeatSensorData
    {
        public bool Available { get; set; }

        public string DeviceName { get; set; }

        public int HeartBeatPerMinute { get; set; }

        public int TotalHeartBeats { get; set; }

        public int Battery { get; set; }
    }
}
