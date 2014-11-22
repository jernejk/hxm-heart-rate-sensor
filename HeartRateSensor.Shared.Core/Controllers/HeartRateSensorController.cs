﻿namespace HeartRateSensor.Shared.Core.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using HeartRateSensor.Shared.Core.Data;
    using HeartRateSensor.Shared.Core.Parsers;

    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;

    public abstract class HeartRateSensorController
    {
        protected readonly IHeartRateSensorParser heartRateSensorParser;
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

        public async Task Start()
        {
            int delay = (int)Math.Round(1000 / (float)updateFrequency);

            cancel = false;
            while (!cancel)
            {
                try
                {
                    reader.InputStreamOptions = InputStreamOptions.Partial;

                    uint count = await reader.LoadAsync(200);

                    byte[] bytes = new byte[count];
                    for (int i = 0; i < count; ++i)
                    {
                        bytes[i] = reader.ReadByte();
                    }

                    HeartBeatSensorData data = heartRateSensorParser.Parse(bytes);
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