using System;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace HeartRateSensorApp.Controllers
{
    using System.Threading.Tasks;

    using HeartRateSensor.Shared.Core.Controllers;
    using HeartRateSensor.Shared.Core.Data;
    using HeartRateSensor.Shared.Core.Parsers;

    public class HeartRateSensorWindowsController : HeartRateSensorController
    {
        public HeartRateSensorWindowsController(IHeartRateSensorParser heartRateSensorParser, int updateFrequency = 5) : base(heartRateSensorParser, updateFrequency)
        {
        }

        public async override Task ConnectToDeviceAsync(DeviceInformation deviceInformation)
        {
            var device = deviceInformation.PlatformDeviceObject as Windows.Devices.Enumeration.DeviceInformation;
            if (device == null)
            {
                throw new NotSupportedException("Unknown platform device object in device information.");
            }

            var connectService = RfcommDeviceService.FromIdAsync(device.Id); 
             RfcommDeviceService rfcommService = await connectService; 
 

             if (rfcommService != null) 
             { 
                 socket = new StreamSocket(); 
                 await 
                     socket.ConnectAsync( 
                         rfcommService.ConnectionHostName, 
                         rfcommService.ConnectionServiceName, 
                         SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication); 
 

                 reader = new DataReader(socket.InputStream); 
 

                 // Wait for one cycle before continue. 
                 await Task.Delay(200); 
             } 
        }
    }
}
