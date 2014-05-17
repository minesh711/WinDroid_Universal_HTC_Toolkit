using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using AndroidDeviceConfig;
using RegawMOD.Android;

namespace WinDroid
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<DeviceConfig> devices = new List<DeviceConfig>();

        private DeviceModel deviceModel = new DeviceModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = DeviceModel;
        }

        public DeviceModel DeviceModel
        {
            get { return deviceModel; }
            set { deviceModel = value; }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PrepareFileSystem();
            LoadDeviceConfig();
            RecognizeDevice();
        }

        private void RecognizeDevice()
        {
            AndroidController android = AndroidController.Instance;
            try
            {
                android.UpdateDeviceList();
                if (android.HasConnectedDevices)
                {
                    DeviceModel.Device = android.GetConnectedDevice(android.ConnectedDevices[0]);
                    if (DeviceModel.Device == null)
                    {
                    }
                    else
                    {
                        switch (DeviceModel.Device.State)
                        {
                            case DeviceState.ONLINE:
                                if (DeviceModel.Device.BuildProp.GetProp("ro.product.model") == null)
                                {
                                }
                                else
                                {
                                    foreach (DeviceConfig deviceConfig in devices)
                                    {
                                        bool found = false;
                                        foreach (DeviceVersion deviceVersion in deviceConfig.Versions)
                                        {
                                            bool matching = true;

                                            foreach (DeviceIdentifier deviceIdentifier in deviceVersion.Identifiers)
                                            {
                                                switch (deviceIdentifier.Type)
                                                {
                                                    case IdentifierType.CodeName:
                                                        matching =
                                                            DeviceModel.Device.BuildProp.GetProp("ro.build.product") ==
                                                            deviceIdentifier.AdditionalArgs[0];
                                                        break;

                                                    case IdentifierType.AndroidVersion:
                                                        matching =
                                                            DeviceModel.Device.BuildProp.GetProp(
                                                                "ro.build.version.release") ==
                                                            deviceIdentifier.AdditionalArgs[0];
                                                        break;
                                                }
                                            }

                                            if (!matching) continue;
                                            DeviceModel.Config = new ConfigModel(deviceConfig);
                                            DeviceModel.Version = new VersionModel(deviceVersion);

                                            found = true;
                                            break;
                                        }

                                        if (found) break;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void LoadDeviceConfig()
        {
            foreach (string file in Directory.GetFiles("Data/Devices"))
            {
                devices.Add(DeviceConfig.LoadConfig(file));
            }
        }

        private void PrepareFileSystem()
        {
            string[] neededDirectories =
            {
                "Data/", "Data/Backups", "Data/Installers", "Data/Logcats", "Data/Logs",
                "Data/Recoveries", "Data/Devices"
            };

            foreach (string dir in neededDirectories)
            {
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            }
        }

        private void ReloadDeviceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RecognizeDevice();
        }
    }
}