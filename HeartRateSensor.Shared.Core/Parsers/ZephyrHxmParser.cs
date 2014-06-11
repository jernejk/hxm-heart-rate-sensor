namespace HeartRateSensor.Shared.Core.Parsers
{
    using HeartRateSensor.Shared.Core.Data;

    /// <summary>
    /// This is used for parsing data from Zephyr non BT LE heart rate sensors.
    /// I don't have any BT LE devices to test them. I believe they have a different set of APIs.
    /// </summary>
    public class ZephyrHxmParser : IHeartRateSensorParser
    {
        /// <summary>
        /// Parses data received from heart rate sensors.
        /// </summary>
        /// <param name="data">String received from heart rate sensor.</param>
        /// <returns>Returns parsed data from the device.</returns>
        public HeartBeatSensorData Parse(string data)
        {
            HeartBeatSensorData sensorData = new HeartBeatSensorData();

            if (data.Length > 13)
            {
                sensorData.Available = true;

                sensorData.HeartBeatPerMinute = (byte)data.Substring(12, 1)[0];
                sensorData.TotalHeartBeats = (byte)data.Substring(13, 1)[0];
                sensorData.Battery = (byte)data.Substring(11, 1)[0];
            }

            return sensorData;
        }
    }
}
