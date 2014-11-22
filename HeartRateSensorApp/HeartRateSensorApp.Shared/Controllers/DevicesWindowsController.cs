using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using HeartRateSensor.Shared.Core.Controllers;
using HeartRateSensor.Shared.Core.Data;
using PlatformDeviceInformation = Windows.Devices.Enumeration.DeviceInformation;

namespace HeartRateSensorApp.Controllers
{
    public class DevicesWindowsController : DevicesController
    {
        public async override Task<List<DeviceInformation>> GetAllDevicesAsync()
        {
            var selector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
            var platformDevices = await PlatformDeviceInformation.FindAllAsync(selector);

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
