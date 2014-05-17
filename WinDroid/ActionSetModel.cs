using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AndroidDeviceConfig;

namespace WinDroid
{
    public sealed class ActionSetModel : INotifyPropertyChanged
    {
        private readonly ActionSet actionSet;

        public ActionSetModel(ActionSet actionSet)
        {
            this.actionSet = actionSet;
        }

        public string Description
        {
            get { return actionSet.Description; }
            set
            {
                if (actionSet.Description == value) return;
                actionSet.Description = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void actions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}