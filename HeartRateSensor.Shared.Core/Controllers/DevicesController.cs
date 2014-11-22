namespace HeartRateSensor.Shared.Core.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Abstract controller used for getting all available devices.
    /// </summary>
    public abstract class DevicesController
    {
        /// <summary>
        /// Get all available BT devices.
        /// </summary>
        /// <returns>Returns all available BT devices.</returns>
        public abstract Task<List<Data.DeviceInformation>> GetAllDevicesAsync();
    }
}
