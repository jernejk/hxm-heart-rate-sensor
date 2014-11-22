namespace HeartRateSensor.Shared.Core.Parsers
{
    using HeartRateSensor.Shared.Core.Data;

    /// <summary>
    /// This interface is used for parsing data received from heart rate sensors.
    /// </summary>
    public interface IHeartRateSensorParser
    {
        /// <summary>
        /// Parses data received from heart rate sensors.
        /// </summary>
        /// <param name="data">String received from heart rate sensor.</param>
        /// <returns>Returns parsed data from the device.</returns>
        HeartBeatSensorData Parse(byte[] data);
    }
}
