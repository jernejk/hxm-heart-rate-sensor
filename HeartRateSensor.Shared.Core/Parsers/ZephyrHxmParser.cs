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
                sensorData.HeartBeatTimestamps = payload.HeartBeatTimestamps;

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

            // Message ID
            const byte messageId = 0x26;

            // Multiple messages in a single package and header does not start at the start.
            if (bytes[0] != stx || bytes[1] != messageId)
            {
                offset = 1;

                // Search for pattern ETX STX MessageID (0x030226)
                while (offset + 13 < bytes.Length && !(bytes[offset - 1] == etx && bytes[offset] == stx && bytes[offset + 1] == messageId))
                {
                    ++offset;
                }

                // Header was not found.
                if (offset >= bytes.Length)
                {
                    return header;
                }
            }

            // Incomplete package or something is weird. May happen time to time.
            if (offset + 13 > bytes.Length || messageId != bytes[++offset])
            {
                return header;
            }

            // Should be 55
            byte dlc = bytes[++offset];

            ++offset;

            // Firmware full name
            header.Firmware = GetFullName(bytes, "9500", ref offset);

            // Hardware ID
            header.Hardware = GetFullName(bytes, "9800", ref offset);

            // Incomplete package. May happen time to time.
            int endPackage = offset + dlc + 1 - 8;
            if (endPackage > bytes.Length)
            {
                return header;
            }

            bool isValid = true;

            // In most of the cases this should be false.
            // This will most likely be true if there are multiple messages or message is corrupted/partial.
            if (bytes[bytes.Length] - 1 != etx)
            {
                int endOffset = offset + dlc + 1;

                // Search for pattern ETX STX (0x0302)
                while (endOffset < bytes.Length && !(bytes[endOffset - 1] == etx && bytes[endOffset] == stx))
                {
                    ++endOffset;
                }

                if (endOffset == bytes.Length)
                {
                    isValid = false;
                }
            }

            // TODO: CRC check

            header.IsValid = isValid;
            return header;
        }

        private static PayloadData GetData(byte[] bytes, ref int offset)
        {
            PayloadData payload = new PayloadData();

            // Offset 11 - 13
            payload.Battery = bytes[offset++];
            payload.HeartRate = bytes[offset++];
            payload.HeartBeatNumber = bytes[offset++];

            // TODO: When message is not 60 bytes, odd stuff are happening and may result in corrupted results.
            // 15 heart beat timestamps by 2 bytes.
            int[] timestamps = new int[15];
            for (int i = 0; i < 15; ++i)
            {
                timestamps[i] = GetShort(bytes, offset);
                offset += 2;
            }

            payload.HeartBeatTimestamps = timestamps;

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

                int rawDistance = GetShort(bytes, offset);
                offset += 2;

                int rawSpeed = GetShort(bytes, offset);
                offset += 2;

                payload.Stride = bytes[offset];
                ++offset;

                // Distance in 1/16 m in 2 bytes.
                const double distanceOneStep = 1.0 / 16.0;
                payload.Distance = rawDistance*distanceOneStep;

                // Speed in 1/256 m/s in 2 bytes. 0 - 15.996 m/s.
                const double speedOneStep = 1.0 / 256.0;
                payload.Speed = rawSpeed*speedOneStep;
            }

            return payload;
        }

        /// <summary>
        /// Read firmware and hardware full name in format 9500.xxxx.Vyz and 9800.xxxx.Vyz.
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

            public int[] HeartBeatTimestamps;

            public bool ContainsStrideData;
        }
    }
}
