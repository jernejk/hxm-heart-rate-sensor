namespace HeartRateSensor.Shared.Core.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Windows.Devices.Bluetooth.Rfcomm;
    using Windows.Devices.Enumeration;

    public class DevicesController
    {
        public async Task<DeviceInformationCollection> GetAllDevicesAsync()
        {
            return await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
        }
    }
}
