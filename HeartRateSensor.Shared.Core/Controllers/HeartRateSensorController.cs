namespace HeartRateSensor.Shared.Core.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Windows.Foundation;

    using HeartRateSensor.Shared.Core.Data;
    using HeartRateSensor.Shared.Core.Parsers;

    using Windows.Devices.Bluetooth.Rfcomm;
    using Windows.Devices.Enumeration;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;

    public class HeartRateSensorController
    {
        private readonly IHeartRateSensorParser heartRateSensorParser;
        private int updateFrequency;
        private StreamSocket socket;
        private DataWriter writer;
        private DataReader reader;
        private bool cancel;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeartRateSensorController"/> class.
        /// </summary>
        /// <param name="heartRateSensorParser">The heart rate sensor parser.</param>
        /// <param name="updateFrequency">The update frequency.</param>
        public HeartRateSensorController(IHeartRateSensorParser heartRateSensorParser, int updateFrequency = 5)
        {
            this.heartRateSensorParser = heartRateSensorParser;
            this.updateFrequency = updateFrequency;
        }

        public event EventHandler<HeartBeatSensorUpdateEventArgs> Updated;

        public async Task ConnectToDevice(DeviceInformation deviceInformation)
        {
            var connectService = RfcommDeviceService.FromIdAsync(deviceInformation.Id);
            RfcommDeviceService rfcommService = await connectService;

            if (rfcommService != null)
            {
                socket = new StreamSocket();
                await
                    socket.ConnectAsync(
                        rfcommService.ConnectionHostName,
                        rfcommService.ConnectionServiceName,
                        SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

                writer = new DataWriter(socket.OutputStream);
                reader = new DataReader(socket.InputStream);

                // Wait for one cycle before continue.
                await Task.Delay(200);
            }
        }

        public async Task Start()
        {
            int delay = (int)Math.Round(1000 / (float)updateFrequency);

            cancel = false;
            while (!cancel)
            {
                string message = string.Empty;
                try
                {
                    reader.InputStreamOptions = InputStreamOptions.Partial;

                    uint count = await reader.LoadAsync(200);

                    while (count-- > 0)
                    {
                        message += (char)reader.ReadByte();
                    }

                    // HACK: A hack which is required on Windows platform for a unknown reason.
                    message = " " + message;

                    HeartBeatSensorData data = heartRateSensorParser.Parse(message);
                    RaiseUpdateEvent(data);

                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    // TODO: Do something more.
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Stops processing devices output but keeps connection.
        /// </summary>
        public void Stop()
        {
            cancel = true;
        }

        /// <summary>
        /// Terminate an connection.
        /// </summary>
        public void Disconnect()
        {
            if (reader != null)
            {
                reader.DetachStream();
                reader.Dispose();
                reader = null;
            }

            if (writer != null)
            {
                writer.DetachStream();
                writer.Dispose();
                writer = null;
            }

            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }
        }

        private void RaiseUpdateEvent(HeartBeatSensorData data)
        {
            if (Updated != null)
            {
                var eventArgs = new HeartBeatSensorUpdateEventArgs();
                eventArgs.Data = data;

                Updated(this, eventArgs);
            }
        }
    }
}
