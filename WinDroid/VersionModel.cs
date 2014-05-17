using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AndroidDeviceConfig;

namespace WinDroid
{
    public sealed class VersionModel : INotifyPropertyChanged
    {
        private ObservableCollection<ActionSetModel> actionSets;
        private ObservableCollection<RecoveryModel> recoveries;
        private DeviceVersion version;

        public VersionModel(DeviceVersion version)
        {
            this.version = version;
            recoveries = new ObservableCollection<RecoveryModel>();
            recoveries.CollectionChanged += recoveries_CollectionChanged;

            foreach (Recovery recovery in this.version.Recoveries)
            {
                recoveries.Add(new RecoveryModel(recovery));
            }

            ActionSets = new ObservableCollection<ActionSetModel>();
            ActionSets.CollectionChanged += actionSets_CollectionChanged;

            foreach (ActionSet actionSet in this.version.PossibleActions)
            {
                ActionSets.Add(new ActionSetModel(actionSet));
            }
        }

        public ObservableCollection<RecoveryModel> Recoveries
        {
            get { return recoveries; }
            set
            {
                if (Recoveries == value) return;
                recoveries = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ActionSetModel> ActionSets
        {
            get { return actionSets; }
            set
            {
                if (ActionSets == value) return;
                actionSets = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void actionSets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("ActionSets");
        }

        private void recoveries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Recoveries");
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}