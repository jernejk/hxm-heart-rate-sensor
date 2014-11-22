namespace HeartRateSensorApp.Controllers
{
    using HeartRateSensor.Shared.Core.Controllers;
    using HeartRateSensor.Shared.Core.Data;
    using HeartRateSensor.Shared.Core.Parsers;

    using System;
    using System.Threading.Tasks;

    public class HeartRateSensorWindowsController : HeartRateSensorController
    {
        public HeartRateSensorWindowsController(IHeartRateSensorParser heartRateSensorParser, int updateFrequency = 5)
            : base(heartRateSensorParser, updateFrequency)
        {
        }

        public async override Task ConnectToDeviceAsync(DeviceInformation deviceInformation)
        {
            throw new NotImplementedException();
        }
    }
}
