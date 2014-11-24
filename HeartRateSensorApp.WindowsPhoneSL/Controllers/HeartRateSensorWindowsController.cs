namespace HeartRateSensorApp.Controllers
{
    using HeartRateSensor.Shared.Core.Controllers;
    using HeartRateSensor.Shared.Core.Data;
    using HeartRateSensor.Shared.Core.Parsers;

    using System;
    using System.Threading.Tasks;

    using Windows.Networking.Proximity;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;

    public class HeartRateSensorWindowsController : HeartRateSensorController
    {
        public HeartRateSensorWindowsController(IHeartRateSensorParser heartRateSensorParser, int updateFrequency = 5)
            : base(heartRateSensorParser, updateFrequency)
        {
        }

        public async override Task ConnectToDeviceAsync(DeviceInformation deviceInformation)
        {
            var peer = deviceInformation.PlatformDeviceObject as PeerInformation;
            if (peer == null)
            {
                throw new NotSupportedException("Unknown platform device object in device information.");
            }

            lastConnectedDeviceName = peer.DisplayName;

            socket = new StreamSocket();

            // Note: If either parameter is null or empty, the call will throw an exception
            await socket.ConnectAsync(peer.HostName, "{00001101-0000-1000-8000-00805f9b34fb}");
            reader = new DataReader(socket.InputStream);
        }
    }
}
