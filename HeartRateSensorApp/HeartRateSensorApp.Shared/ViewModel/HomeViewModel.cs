using HeartRateSensorApp.Controllers;

namespace HeartRateSensorApp.ViewModel
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    using HeartRateSensor.Shared.Core.Controllers;
    using HeartRateSensor.Shared.Core.Data;
    using HeartRateSensor.Shared.Core.Parsers;

    using HeartRateSensorApp.Commands;

    public class HomeViewModel : INotifyPropertyChanged
    {
        private HeartRateSensorController sensorController;
        private IHeartRateSensorParser sensorParser;
        private DevicesController devicesController;
        private DeviceInformation selectedDevice;
        private HeartBeatSensorData data;
        private bool isLoading;

        public event PropertyChangedEventHandler PropertyChanged;

        public HomeViewModel()
        {
            sensorParser = new ZephyrHxmParser();
            sensorController = new HeartRateSensorWindowsController(sensorParser);
            devicesController = new DevicesWindowsController();

            Devices = new ObservableCollection<DeviceInformation>();
            ConnectToDeviceCommand = new RelayCommand(ConnectToDevice);

            sensorController.Updated += sensorController_Updated;

            LoadAsync();
        }

        public ObservableCollection<DeviceInformation> Devices { get; set; }

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

        public ICommand ConnectToDeviceCommand { get; set; }

        public async void LoadAsync()
        {
            var devices = await devicesController.GetAllDevicesAsync();

            foreach (var device in devices)
            {
                Devices.Add(device);
            }
        }

        public async void ConnectToDevice(object obj)
        {
            if (SelectedDevice == null)
            {
                return;
            }

            IsLoading = true;

            try
            {
                await sensorController.ConnectToDeviceAsync(SelectedDevice);
                sensorController.Start();
            }
            catch
            {
            }

            IsLoading = false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        void sensorController_Updated(object sender, HeartRateSensor.Shared.Core.Data.HeartBeatSensorUpdateEventArgs e)
        {
            if (e.Data != null && e.Data.IsAvailable)
            {
                HeartBeatSensorData = e.Data;
            }
        }
    }
}
