namespace HeartRateSensor.Shared.Core.Controllers
{
    using System;
    using System.Threading.Tasks;

    using HeartRateSensor.Shared.Core.Data;
    using HeartRateSensor.Shared.Core.Parsers;

    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;

    /// <summary>
    /// This controller is used to connect and get data from a device.
    /// </summary>
    public abstract class HeartRateSensorController
    {
        protected readonly IHeartRateSensorParser heartRateSensorParser;
        protected string lastConnectedDeviceName;
        protected int updateFrequency;
        protected StreamSocket socket;
        protected DataReader reader;
        protected bool cancel;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeartRateSensorController"/> class.
        /// </summary>
        /// <param name="heartRateSensorParser">The heart rate sensor parser.</param>
        /// <param name="updateFrequency">The update frequency.</param>
        protected HeartRateSensorController(IHeartRateSensorParser heartRateSensorParser, int updateFrequency = 5)
        {
            this.heartRateSensorParser = heartRateSensorParser;
            this.updateFrequency = updateFrequency;
        }

        public event EventHandler<HeartBeatSensorUpdateEventArgs> Updated;

        public abstract Task ConnectToDeviceAsync(DeviceInformation deviceInformation);

        /// <summary>
        /// Starts processing sensor data from device.
        /// <remarks>You need to be connected to the device!</remarks>
        /// <remarks>This method will complete when called <see cref="Stop" /> method.</remarks>
        /// </summary>
        public async Task StartAsync()
        {
            int interval = (int)Math.Round(1000 / (float)updateFrequency);

            cancel = false;
            while (!cancel)
            {
                try
                {
                    reader.InputStreamOptions = InputStreamOptions.Partial;

                    // All messages should be size of 60 bytes however it is possible
                    // that device was unable to keep up all the data and receives more than 60 bytes.
                    // Also a different sensor can also have different message size.
                    uint count = await reader.LoadAsync(200);

                    // I had some weird issues with ReadBytes but ReadByte worked just fine.
                    byte[] bytes = new byte[count];
                    for (int i = 0; i < count; ++i)
                    {
                        bytes[i] = reader.ReadByte();
                    }

                    // Parse data and send new data to listeners.
                    HeartBeatSensorData data = heartRateSensorParser.Parse(bytes);
                    data.DeviceName = lastConnectedDeviceName;
                    RaiseUpdateEvent(data, null);

                    // Sleep until next interval
                    await Task.Delay(interval);
                }
                catch (Exception ex)
                {
                    RaiseUpdateEvent(null, ex);
                }
            }
        }

        /// <summary>
        /// Stops processing devices output but keeps connection.
        /// Call <see cref="StartAsync" /> to resume processing data.
        /// </summary>
        public void Stop()
        {
            cancel = true;
        }

        /// <summary>
        /// Terminate current connection.
        /// </summary>
        public void Disconnect()
        {
            Stop();

            if (reader != null)
            {
                reader.DetachStream();
                reader.Dispose();
                reader = null;
            }

            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }
        }

        private void RaiseUpdateEvent(HeartBeatSensorData data, Exception error)
        {
            if (Updated != null)
            {
                Updated(this, new HeartBeatSensorUpdateEventArgs(data, error));
            }
        }
    }
}
