using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AndroidDeviceConfig;

namespace WinDroid
{
    public sealed class ConfigModel : INotifyPropertyChanged
    {
        private readonly DeviceConfig config;
        private ObservableCollection<VersionModel> versions;

        public ConfigModel(DeviceConfig config)
        {
            this.config = config;
            Versions = new ObservableCollection<VersionModel>();
            Versions.CollectionChanged += versions_CollectionChanged;

            foreach (DeviceVersion deviceVersion in this.config.Versions)
            {
                Versions.Add(new VersionModel(deviceVersion));
            }
        }

        public string Vendor
        {
            get { return config.Vendor; }
            set
            {
                if (config.Vendor == value) return;
                config.Vendor = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return config.Name; }
            set
            {
                if (config.Name == value) return;
                config.Name = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<VersionModel> Versions
        {
            get { return versions; }
            set
            {
                if (Versions == value) return;
                versions = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void versions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Versions");
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}