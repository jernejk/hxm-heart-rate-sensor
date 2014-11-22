using System.Collections.Generic;

namespace HeartRateSensor.Shared.Core.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Windows.Devices.Enumeration;

    public abstract class DevicesController
    {
        public abstract Task<List<HeartRateSensor.Shared.Core.Data.DeviceInformation>> GetAllDevicesAsync();
    }
}
