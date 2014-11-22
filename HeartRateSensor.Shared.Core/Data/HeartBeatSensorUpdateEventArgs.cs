namespace HeartRateSensor.Shared.Core.Data
{
    using System;

    /// <summary>
    /// This class is used to notify all the listeners when new data is available.
    /// Data is readonly to protect them from being modified.
    /// </summary>
    public class HeartBeatSensorUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the HeartBeatSensorUpdateEventArgs class.
        /// </summary>
        /// <param name="data">Data from heart beat monitor sensor.</param>
        public HeartBeatSensorUpdateEventArgs(HeartBeatSensorData data)
            : this(data, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the HeartBeatSensorUpdateEventArgs class.
        /// </summary>
        /// <param name="data">Data from heart beat monitor sensor.</param>
        /// <param name="error">Any exceptions that may have happened while trying to get data from sensor.</param>
        public HeartBeatSensorUpdateEventArgs(HeartBeatSensorData data, Exception error)
        {
            Data = data;
            Error = error;
        }

        /// <summary>
        /// Gets data from heart beat monitor sensor.
        /// </summary>
        public HeartBeatSensorData Data { get; private set; }

        public Exception Error { get; private set; }
    }
}
