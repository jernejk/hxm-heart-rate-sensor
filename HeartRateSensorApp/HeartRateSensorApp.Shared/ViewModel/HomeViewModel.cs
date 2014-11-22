using System;

namespace HeartRateSensorApp.ViewModel
{
    using HeartRateSensor.Shared.Core.Controllers;
    using HeartRateSensor.Shared.Core.Data;
    using HeartRateSensor.Shared.Core.Parsers;

    using HeartRateSensorApp.Commands;
    using HeartRateSensorApp.Controllers;

    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    /// <summary>
    /// This view model is used to demonstrate how you can use MVVM pattern to display HRM sensor data.
    /// It does not use any MVVM libraries for sake of independets from other libraries.
    /// 
    /// I recommend MVVM Light or similiar in a real project.
    /// </summary>
    public class HomeViewModel : INotifyPropertyChanged
    {
        private readonly HeartRateSensorController sensorController;
        private readonly IHeartRateSensorParser sensorParser;
        private readonly DevicesController devicesController;

        private DeviceInformation selectedDevice;
        private HeartBeatSensorData data;
        private bool isLoading;

        public event PropertyChangedEventHandler PropertyChanged;

        public HomeViewModel()
        {
            // This is platform independent implementation for parsing sensor data from Zephyr HxM BT device.
            // Replace ZephyrHxmParser with your own implementation if you need support for another BT 3.x HRM.
            sensorParser = new ZephyrHxmParser();

            // This are platform specific implementation controllers.
            // Because both controllers are in same namespace and have same class name on all platforms
            // there is no need for #if statements.
            sensorController = new HeartRateSensorWindowsController(sensorParser);
            devicesController = new DevicesWindowsController();

            Devices = new ObservableCollection<DeviceInformation>();
            ConnectToDeviceCommand = new RelayCommand(ConnectToDevice);

            sensorController.Updated += SensorControllerUpdated;

            LoadAsync();
        }

        /// <summary>
        /// Gets or sets list of available devices.
        /// </summary>
        public ObservableCollection<DeviceInformation> Devices { get; set; }

        /// <summary>
        /// Gets or sets currently selected device used by <see cref="ConnectToDeviceCommand" /> command.
        /// </summary>
        public DeviceInformation SelectedDevice
        {
            get
            {
                return selectedDevice;
            }

            set
            {
                if (selectedDevice != value)
                {
                    selectedDevice = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether we're in process of connecting to a device.
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return isLoading;
            }

            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets data from heart beat sensor.
        /// </summary>
        public HeartBeatSensorData HeartBeatSensorData
        {
            get
            {
                return data;
            }

            set
            {
                if (data != value)
                {
                    data = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets connect to device command.
        /// </summary>
        public ICommand ConnectToDeviceCommand { get; set; }

        public async void LoadAsync()
        {
            // While WinRT designer can show you actual available devices it can also cause weird stuff. (like "Element not found!" exception or VS crash)
#if SILVERLIGHT
            if (DesignerProperties.IsInDesignTool)
#elif NETFX_CORE
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
#endif
            {
                // Show some devices in design time.
                Devices.Add(new DeviceInformation { DisplayName = "Mock device" });
                Devices.Add(new DeviceInformation { DisplayName = "HxM device" });

                HeartBeatSensorData = new HeartBeatSensorData();
                HeartBeatSensorData.Battery = 90;
                HeartBeatSensorData.Distance = 23.3452;
                HeartBeatSensorData.Firmware = "9500.550.V1f";
                HeartBeatSensorData.Hardware = "9500.550.V1f";
                HeartBeatSensorData.HeartBeatPerMinute = 67;
                HeartBeatSensorData.TotalHeartBeats = 234;
                HeartBeatSensorData.Stride = 123;
                HeartBeatSensorData.Speed = 5.456;
                HeartBeatSensorData.IsAvailable = true;
                HeartBeatSensorData.IsStrideAvailabe = true;

                OnPropertyChanged("HeartBeatSensorData");

                return;
            }

            // Get devices in platform independent data structure.
            // Method sensorController.ConnectToDeviceAsync will figure out what to do with it.
            var devices = await devicesController.GetAllDevicesAsync();
            foreach (var device in devices)
            {
                Devices.Add(device);
            }
        }

        /// <summary>
        /// Connect to selected device.
        /// </summary>
        /// <param name="commandParameter">Not used.</param>
        public async void ConnectToDevice(object commandParameter)
        {
            if (SelectedDevice == null)
            {
                return;
            }

            IsLoading = true;

            try
            {
                await sensorController.ConnectToDeviceAsync(SelectedDevice);

                // This will start infinite loop until sensorController.Stop(); is called.
                sensorController.StartAsync();
            }
            catch (Exception exception)
            {
                if (exception.Message.StartsWith("No more data is available.") || exception.Message.StartsWith("Element not found."))
                {
                    // TODO: Probably incorrect device type.
                }
                else if (exception.Message.StartsWith("Class not registered"))
                {
                    // TODO: Issues with BT drivers?
                }
            }

            IsLoading = false;
        }

        /// <summary>
        /// Update view model data.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">New data from heart beat sensor</param>
        private void SensorControllerUpdated(object sender, HeartBeatSensorUpdateEventArgs e)
        {
            // TODO: Try to do something with the error
            if (e.Error != null)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }

            if (e.Data != null && e.Data.IsAvailable)
            {
                HeartBeatSensorData = e.Data;
            }
        }

        #region INotifyPropertyChanged helper
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
