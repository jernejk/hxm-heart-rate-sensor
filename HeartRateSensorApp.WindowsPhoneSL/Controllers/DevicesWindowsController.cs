namespace HeartRateSensorApp.Controllers
{
    using HeartRateSensor.Shared.Core.Controllers;
    using HeartRateSensor.Shared.Core.Data;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Windows.Networking.Proximity;

    public class DevicesWindowsController : DevicesController
    {
        /// <summary>
        /// Get all available BT devices.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">You have not set correct capabilities for your platform.</exception>
        /// <returns>Returns all available BT devices.</returns>
        public override async Task<List<DeviceInformation>> GetAllDevicesAsync()
        {
            var devices = new List<DeviceInformation>();

            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = string.Empty;
            IReadOnlyList<PeerInformation> peers;

            try
            {
                peers = await PeerFinder.FindAllPeersAsync();
            }
            catch (UnauthorizedAccessException)
            {
                if (Debugger.IsAttached)
                {
                    // This exception likely occured becaused you have not added this to Package.appxmanifest:
                    /*
                    <m2:DeviceCapability Name="bluetooth.rfcomm">
                      <m2:Device Id="any">
                        <m2:Function Type="name:serialPort" />
                      </m2:Device>
                    </m2:DeviceCapability>
                    */

                    // Also for Windows Phone 8.1 SL you need to add networking (ID_CAP_NETWORKING) and
                    // promixity (ID_CAP_PROXIMITY) capabilities in WMAppMenifest.xml.
                    Debugger.Break();
                }

                throw;
            }

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
