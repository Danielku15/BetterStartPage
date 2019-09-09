using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using BetterStartPage.Properties;

namespace BetterStartPage.Control.ViewModel
{
    [DataContract]
    internal class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
