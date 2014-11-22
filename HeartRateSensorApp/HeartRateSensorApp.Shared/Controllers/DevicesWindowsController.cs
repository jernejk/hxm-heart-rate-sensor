namespace HeartRateSensorApp.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Windows.Devices.Bluetooth.Rfcomm;

    using HeartRateSensor.Shared.Core.Controllers;
    using HeartRateSensor.Shared.Core.Data;

    using DeviceInformationCollection = Windows.Devices.Enumeration.DeviceInformationCollection;
    using PlatformDeviceInformation = Windows.Devices.Enumeration.DeviceInformation;

    public class DevicesWindowsController : DevicesController
    {
        /// <summary>
        /// Get all available BT devices.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">You have not set correct capabilities for your platform.</exception>
        /// <returns>Returns all available BT devices.</returns>
        public async override Task<List<DeviceInformation>> GetAllDevicesAsync()
        {
            var selector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
            DeviceInformationCollection platformDevices;

            try
            {
                platformDevices = await PlatformDeviceInformation.FindAllAsync(selector);
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
                    Debugger.Break();
                }

                throw;
            }

            var devices = new List<DeviceInformation>();
            foreach (var platformDevice in platformDevices)
            {
                var device = new DeviceInformation();
                device.DisplayName = platformDevice.Name;
                device.PlatformDeviceObject = platformDevice;

                devices.Add(device);
            }

            return devices;
        }
    }
}
