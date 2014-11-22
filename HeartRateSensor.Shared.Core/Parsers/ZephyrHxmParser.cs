namespace HeartRateSensor.Shared.Core.Parsers
{
    using HeartRateSensor.Shared.Core.Data;

    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// This is used for parsing data from Zephyr non BT LE heart rate sensors.
    /// I don't have any BT LE devices to test them. I believe they have a different set of APIs.
    /// </summary>
    public class ZephyrHxmParser : IHeartRateSensorParser
    {
        /// <summary>
        /// Parses data received from heart rate sensors.
        /// </summary>
        /// <param name="bytes">String received from heart rate sensor.</param>
        /// <returns>Returns parsed data from the device.</returns>
        public HeartBeatSensorData Parse(byte[] bytes)
        {
            HeartBeatSensorData sensorData = new HeartBeatSensorData();

            int offset;
            HeaderData header = GetHeader(bytes, out offset);
            if (header.IsValid)
            {
                sensorData.Firmware = header.Firmware;
                sensorData.Hardware = header.Hardware;

                PayloadData payload = GetData(bytes, ref offset);

                sensorData.IsAvailable = true;

                sensorData.Battery = payload.Battery;
                sensorData.HeartBeatPerMinute = payload.HeartRate;
                sensorData.TotalHeartBeats = payload.HeartBeatNumber;

                if (payload.ContainsStrideData)
                {
                    sensorData.Speed = payload.Speed;
                    sensorData.Distance = payload.Distance;
                    sensorData.Stride = payload.Stride;
                }
            }

            return sensorData;
        }

        private static HeaderData GetHeader(byte[] bytes, out int offset)
        {
            HeaderData header = new HeaderData();
            offset = 0;

            // STX or start of text
            const byte stx = 0x02;
            
            // ETX or end of text
            const byte etx = 0x03;

            // Multiple messages in a single package and header does not start at the start.
            if (bytes[offset] != stx)
            {
                offset = 1;

                // Search for pattern ETX STX (0302)
                while (offset < bytes.Length && !(bytes[offset -1] == etx && bytes[offset] == stx))
                {
                    ++offset;
                }

                // Header was not found.
                if (offset >= bytes.Length)
                {
                    return header;
                }
            }

            // Sometimes we get multiple STX.
            while (offset < bytes.Length && bytes[offset] != stx)
            {
                ++offset;
            }

            // Incomplete package. May happen time to time.
            if (offset + 13 > bytes.Length)
            {
                return header;
            }

            byte messageId = bytes[++offset];
            byte dlc = bytes[++offset];

            // Firmware full name
            ++offset;
            header.Firmware = GetFullName(bytes, "9500", ref offset);

            // Hardware ID
            header.Hardware = GetFullName(bytes, "9800", ref offset);

            // Incomplete package. May happen time to time.
            int endPackage = offset + dlc + 1 - 8;
            if (endPackage > bytes.Length || messageId != 0x26)
            {
                return header;
            }

            // TODO: Use this for additional validations?
            //if (bytes[bytes.Length - 1] != etx)
            //{
            //    return header;
            //}

            // TODO: CRC check

            header.IsValid = true;
            return header;
        }

        private static PayloadData GetData(byte[] bytes, ref int offset)
        {
            PayloadData payload = new PayloadData();

            // Offset 11 - 13
            payload.Battery = bytes[offset++];
            payload.HeartRate = bytes[offset++];
            payload.HeartBeatNumber = bytes[offset++];

            // TODO: Data seems to behave weird if at first couple of reads and if data is not read fast enough.
            // 15 heart beat timestamps by 2 bytes
            offset += 2 * 15;

            // TODO: Figure out why reserved does not start at offset 44 but it moves between 50-80.
            // For whatever reason reserved bytes (000000) is always moving when sensor does not give exactly 60 bytes.
            // After finding this, we can get distance, speed and strides.
            int numberOfZeroes = 0;
            int length = bytes.Length;
            while (offset + 8 < length && numberOfZeroes < 6)
            {
                if (bytes[offset] == 0)
                {
                    ++numberOfZeroes;
                }
                else
                {
                    numberOfZeroes = 0;
                }

                ++offset;
            }

            if (numberOfZeroes >= 6)
            {
                payload.ContainsStrideData = true;

                // Distance in 1/16m in 2 bytes.
                int rawDistance = GetShort(bytes, offset);
                offset += 2;

                int rawSpeed = GetShort(bytes, offset);
                offset += 2;

                payload.Stride = bytes[offset];
                ++offset;

                // Distance in 1/16m in 2 bytes.
                const double distanceOneStep = 1.0 / 16.0;
                payload.Distance = rawDistance*distanceOneStep;

                const double speedOneStep = 1.0 / 256.0;
                payload.Speed = rawSpeed*speedOneStep;
            }

            return payload;
        }

        /// <summary>
        /// Read firmware and hardware full name in format 9500.xxxx.Vyz and 9800.xxxx.Vyz.
        /// TODO: Verrify if values are parsed correctly.
        /// </summary>
        /// <param name="bytes">Data from device</param>
        /// <param name="prefix">Prefix for well formatted string. (eg. 9500 for firmware and 9800 for hardware)</param>
        /// <param name="offset">Current offset</param>
        /// <returns>Returns well formatted string.</returns>
        private static string GetFullName(byte[] bytes, string prefix, ref int offset)
        {
            // little endian
            int id = GetShort(bytes, offset);
            offset += 2;

            char majorVersion = (char)bytes[offset + 1];
            char minorVersion = (char)bytes[offset];
            offset += 2;

            // Value should be from a-z as character but instead it is \0.
            if (minorVersion == 0)
            {
                minorVersion = 'A';
            }

            return string.Format("{0}.{1}.V{2}{3}", prefix, id.ToString("0000"), majorVersion, minorVersion);
        }

        private static int GetShort(byte[] bytes, int offset)
        {
            return (bytes[offset + 1] << 8) + bytes[offset];
        }

        private struct HeaderData
        {
            public string Firmware;
            public string Hardware;

            public bool IsValid;
        }

        private struct PayloadData
        {
            public byte Battery;
            public byte HeartRate;
            public byte HeartBeatNumber;
            public double Distance;
            public double Speed;
            public byte Stride;

            public bool ContainsStrideData;
        }
    }
}
