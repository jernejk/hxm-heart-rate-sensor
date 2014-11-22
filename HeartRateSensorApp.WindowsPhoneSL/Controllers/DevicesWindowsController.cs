namespace HeartRateSensorApp.Controllers
{
    using HeartRateSensor.Shared.Core.Controllers;
    using HeartRateSensor.Shared.Core.Data;

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Windows.Networking.Proximity;

    public class DevicesWindowsController : DevicesController
    {
        public override async Task<List<DeviceInformation>> GetAllDevicesAsync()
        {
            var devices = new List<DeviceInformation>();

            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = string.Empty;
            var peers = await PeerFinder.FindAllPeersAsync();

            foreach (var peerInformation in peers)
            {
                var device = new DeviceInformation();
                device.DisplayName = peerInformation.DisplayName;
                device.PlatformDeviceObject = peerInformation;

                devices.Add(device);
            }

            return devices;
        }
    }
}
