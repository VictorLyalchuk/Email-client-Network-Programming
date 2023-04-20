using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MailClient
{
    public class TimeValue : INotifyPropertyChanged
    {
        static private double mytime;
        public double MyTime
        {
            get
            {
                return mytime;
            }
            set
            {
                mytime = value;
                OnPropertyChanged();
            }
        }
        public TimeValue(){}
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}