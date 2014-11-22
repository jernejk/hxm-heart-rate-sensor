namespace HeartRateSensor.Shared.Core.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class DevicesController
    {
        public abstract Task<List<Data.DeviceInformation>> GetAllDevicesAsync();
    }
}
