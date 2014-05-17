using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AndroidDeviceConfig;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using RegawMOD.Android;

namespace WinDroid
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<DeviceConfig> devices = new List<DeviceConfig>();

        private bool operationRunning;

        private LogModel log = new LogModel();

        public sealed class LogModel : INotifyPropertyChanged
        {
            private StringBuilder logBuilder = new StringBuilder();

            public string LogText
            {
                get { return logBuilder.ToString(); }
            }

            public void AddLogItem(string text, string tag)
            {
                logBuilder.AppendFormat("{0}[{1}]:{2}\n", DateTime.Now.ToLongTimeString(), tag, text);
                OnPropertyChanged("LogText");
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private DeviceModel deviceModel = new DeviceModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = DeviceModel;
            logBox.DataContext = Log;
        }

        public DeviceModel DeviceModel
        {
            get { return deviceModel; }
            set { deviceModel = value; }
        }

        public LogModel Log
        {
            get { return log; }
            set { log = value; }
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressDialogController controller = await this.ShowProgressAsync("Please wait...", String.Empty);
            controller.SetCancelable(false);

            controller.SetMessage("Preparing file system...");
            PrepareFileSystem();
            controller.SetProgress(0.3);

            controller.SetMessage("Loading device configs...");
            LoadDeviceConfig();
            controller.SetProgress(0.6);

            controller.SetMessage("Searching your device...");
            controller.SetCancelable(true);
            RecognizeDevice();
            controller.SetProgress(1);

            await controller.CloseAsync();
        }

        private void RecognizeDevice()
        {
            operationRunning = true;
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

                                            Log.AddLogItem(deviceConfig.ToString()+ " detected!", "DEVICE");

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
            operationRunning = false;
        }

        private void LoadDeviceConfig()
        {
            foreach (string file in Directory.GetFiles("Data/Devices"))
            {
                devices.Add(DeviceConfig.LoadConfig(file));
                Log.AddLogItem("config file \"" + file + "\" loaded!", "CONFIG");
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
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Log.AddLogItem(dir + " created!", "FILESYSTEM");
                }
            }
        }

        private void ReloadDeviceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RecognizeDevice();
        }

        private void ReloadDeviceCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !operationRunning;
        }

        private async void DownloadFile(string url, string path)
        {
            var controller = await this.ShowProgressAsync("Downloading...", String.Empty);
            var progress = 0;
            controller.SetCancelable(false);
            WebClient client = new WebClient();
            client.DownloadProgressChanged += (s, e) =>
            {
                controller.SetMessage(e.BytesReceived + "B/" + e.TotalBytesToReceive + "B");
                if (e.ProgressPercentage > progress)
                {
                    controller.SetProgress(e.ProgressPercentage/100.0d);
                    progress = e.ProgressPercentage;
                }
            };
            client.DownloadFileCompleted += (s, e) => controller.CloseAsync();
            client.DownloadFileAsync(new Uri(url), path);
        }

        private void ToggleLogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((Flyout) this.Flyouts.Items[0]).IsOpen = !((Flyout) this.Flyouts.Items[0]).IsOpen;
        }
    }

    public class StringConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                object value = values[i];

                if (value is string)
                {
                    stringBuilder.Append(value + " ");
                }
            }
            return stringBuilder.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class DeviceStateToColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((DeviceState) value)
            {
                case DeviceState.ONLINE:
                    return Colors.Green;
                    break;
                case DeviceState.UNKNOWN:
                    return Colors.Gray;
                    break;
                case DeviceState.FASTBOOT:
                case DeviceState.RECOVERY:
                    return Colors.Orange;
                    break;
                case DeviceState.OFFLINE:
                    return Colors.Red;
                default:
                    return Colors.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}