using System.ComponentModel;

namespace RemoteConnector.Models
{
    class MachineInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string MacAddress { get; set; }

        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name == value) return;
                _Name = value;
                PropertyChanged?.Invoke(this, _NameChangedEventArgs);
            }
        }
        private string _Name;
        private PropertyChangedEventArgs _NameChangedEventArgs = new PropertyChangedEventArgs(nameof(Name));

        public string UserName { get; set; }

        public string Password { get; set; }

        public string PuTTYSession { get; set; }
    }
}
