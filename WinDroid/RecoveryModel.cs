using System.ComponentModel;
using System.Runtime.CompilerServices;
using AndroidDeviceConfig;

namespace WinDroid
{
    public sealed class RecoveryModel : INotifyPropertyChanged
    {
        private Recovery recovery;

        public RecoveryModel(Recovery recovery)
        {
            this.recovery = recovery;
        }

        public string Name
        {
            get { return recovery.Name; }
            set
            {
                if (Name == value) return;
                recovery.Name = value;
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