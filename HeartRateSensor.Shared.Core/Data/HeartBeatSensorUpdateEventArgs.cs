namespace HeartRateSensor.Shared.Core.Data
{
    using System;

    public class HeartBeatSensorUpdateEventArgs : EventArgs
    {
        public HeartBeatSensorData Data { get; set; }
    }
}
