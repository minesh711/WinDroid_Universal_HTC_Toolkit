using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AndroidDeviceConfig;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using RegawMOD.Android;
using WinDroid;

namespace WinDroid_Universal_HTC_Toolkit
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly List<DeviceConfig> _Devices = new List<DeviceConfig>();

        private bool _OperationRunning;

        private LogModel _Log = new LogModel();

        private DeviceModel _DeviceModel = new DeviceModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = DeviceModel;
            logBox.DataContext = Log;
        }

        public DeviceModel DeviceModel
        {
            get { return _DeviceModel; }
            set { _DeviceModel = value; }
        }

        public LogModel Log
        {
            get { return _Log; }
            set { _Log = value; }
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressDialogController controller = await this.ShowProgressAsync("Please wait...", String.Empty);
            controller.SetCancelable(false);

            controller.SetMessage("Initializing...");
            LogBaseSystemInfos();
            controller.SetProgress(0.16);

            controller.SetMessage("Preparing file system...");
            PrepareFileSystem();
            controller.SetProgress(0.3);

            controller.SetMessage("Loading device configs...");
            await LoadDeviceConfig();
            controller.SetProgress(0.6);

            controller.SetMessage("Searching your device...");
            controller.SetCancelable(true);
            await RecognizeDevice();
            controller.SetProgress(1);

            await controller.CloseAsync();
        }

        private void LogBaseSystemInfos()
        {
            Log.AddLogItem(Environment.OSVersion.ToString() + " " + (Environment.Is64BitOperatingSystem ? "64Bit" : "32bit"), "INFO");
            Log.AddLogItem(Assembly.GetExecutingAssembly().GetName().Name + " " + Assembly.GetExecutingAssembly().GetName().Version, "INFO");
            foreach (AssemblyName referencedAssembly in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            { 
                Log.AddLogItem(referencedAssembly.Name + " " + referencedAssembly.Version, "INFO");
            }
        }

        private async Task RecognizeDevice()
        {
            _OperationRunning = true;
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
                                    foreach (DeviceConfig deviceConfig in _Devices)
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

                                            Log.AddLogItem(deviceConfig.Vendor + " " + deviceConfig.Name + " detected!", "DEVICE");

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
            _OperationRunning = false;
        }

        private async Task LoadDeviceConfig()
        {
            foreach (string file in Directory.GetFiles("Data/Devices"))
            {
                _Devices.Add(DeviceConfig.LoadConfig(file));
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
            e.CanExecute = !_OperationRunning;
        }

        private async Task DownloadFile(string url, string path)
        {
            bool downloading = true;
            var controller = await this.ShowProgressAsync("Downloading...", "URL:" + url +"\nDestination:" + path);
            controller.SetCancelable(false);

            WebClient client = new WebClient();
            client.DownloadProgressChanged += (s, e) =>
            {
                controller.SetMessage(e.BytesReceived + "B/" + e.TotalBytesToReceive + "B");
                double progress = 0;
                if (e.ProgressPercentage > progress)
                {
                    controller.SetProgress(e.ProgressPercentage / 100.0d);
                    progress = e.ProgressPercentage;
                }
            };
            client.DownloadFileCompleted += (s, e) => { controller.CloseAsync();
                                                          downloading = false;
            };
            Log.AddLogItem("Download of " + url + "started!", "DOWNLOAD");
            await client.DownloadFileTaskAsync(new Uri(url), path);
        }

        private void ToggleLogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((Flyout)this.Flyouts.Items[0]).IsOpen = !((Flyout)this.Flyouts.Items[0]).IsOpen;
        }

        private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _OperationRunning = true;
            if (recoveriesList.SelectedItem != null)
            {

                if (DeviceModel.Device.State != DeviceState.FASTBOOT)
                {
                    DeviceModel.Device.RebootBootloader();
                    var controller = await this.ShowInputAsync(String.Empty, "Your device will boot to bootloader now. Please click \"Ok\" when your device has booted to bootloader.");
                }
                    var recovery = ((RecoveryModel) recoveriesList.SelectedItem);
                    if (!File.Exists("Data/Recoveries/" + recovery.Name + "_" + DeviceModel.GetHashCode() + ".img"))
                    {
                        await DownloadFile(recovery.DownloadUrl,
                            "Data/Recoveries/" + recovery.Name + "_" + DeviceModel.GetHashCode() + ".img");
                    }
                    var fCommand = Fastboot.FormFastbootCommand("flash", "recovery",
                        "Data/Recoveries/" + recovery.Name + "_" + DeviceModel.GetHashCode() + ".img");
                Fastboot.ExecuteFastbootCommand(fCommand);
            }
            _OperationRunning = false;
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

        #endregion IMultiValueConverter Members
    }

    public class DeviceStateToColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((DeviceState)value)
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

        #endregion IValueConverter Members
    }

    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color[] parameters = (Color[]) parameter;
            return ((bool) value) ? parameters[0] : parameters[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}