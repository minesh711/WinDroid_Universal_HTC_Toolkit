using System.ComponentModel;
using System.Runtime.CompilerServices;
using RegawMOD.Android;

namespace WinDroid
{
    public class DeviceModel : INotifyPropertyChanged
    {
        private ConfigModel config;
        private Device device;
        private VersionModel version;

        public ConfigModel Config
        {
            get { return config; }
            set
            {
                if (Config == value) return;
                config = value;
                OnPropertyChanged();
            }
        }

        public VersionModel Version
        {
            get { return version; }
            set
            {
                if (Version == value) return;
                version = value;
                OnPropertyChanged();
            }
        }

        public Device Device
        {
            get { return device; }
            set
            {
                if (Device == value) return;
                device = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}