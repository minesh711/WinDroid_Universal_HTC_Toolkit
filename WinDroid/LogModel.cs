using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace WinDroid_Universal_HTC_Toolkit
{
    public sealed class LogModel : INotifyPropertyChanged
    {
        private StringBuilder logBuilder = new StringBuilder();

        public string LogText
        {
            get { return logBuilder.ToString(); }
        }

        public void AddLogItem(string text, string tag)
        {
            logBuilder.AppendFormat("{0}[{1}]:{2}\n", DateTime.Now.ToLongTimeString(), tag.ToUpperInvariant(), text);
            OnPropertyChanged("LogText");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
