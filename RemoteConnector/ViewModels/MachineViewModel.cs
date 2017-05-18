using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteConnector.ViewModels
{
    class MachineViewModel : ICommand, INotifyPropertyChanged, IDisposable
    {
        private SemaphoreSlim _s = new SemaphoreSlim(1);

        public event Action RunPuTTY;
        public event Action RunWinSCP;
        public event EventHandler CanExecuteChanged;

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        public string IPAddress { get; }

        public string MacAddress { get; }

        public string ImageUrl => $"https://github.com/identicons/ShTair.png";

        public bool IsDisposed { get; private set; }

        public string Name
        {
            get { return _Name; }
            private set
            {
                if (_Name == value) return;
                _Name = value;
                PropertyChanged?.Invoke(this, _NameChangedEventArgs);
            }
        }
        private string _Name;
        private PropertyChangedEventArgs _NameChangedEventArgs = new PropertyChangedEventArgs(nameof(Name));

        public string Status
        {
            get { return _Status; }
            private set
            {
                if (_Status == value) return;
                _Status = value;
                PropertyChanged?.Invoke(this, _StatusChangedEventArgs);
            }
        }
        private string _Status = "？";
        private PropertyChangedEventArgs _StatusChangedEventArgs = new PropertyChangedEventArgs(nameof(Status));

        #endregion

        public MachineViewModel(string ip, string mac)
        {
            IPAddress = ip;
            MacAddress = mac;
            Name = mac;

            Task.Run(() => Pinging());
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            switch ((string)parameter)
            {
                case "PuTTY": RunPuTTY?.Invoke(); break;
                case "WinSCP": RunWinSCP?.Invoke(); break;
            }
        }

        private async void Pinging()
        {
            var ping = new Ping();
            while (true)
            {
                await _s.WaitAsync();
                try
                {
                    if (IsDisposed) break;
                    var result = await ping.SendPingAsync(IPAddress);
                    Status = (result.Status == IPStatus.Success) ? "○" : "×";
                }
                finally { _s.Release(); }
                await Task.Delay(10000);
            }
        }

        public void Dispose()
        {
            _s.Wait();
            IsDisposed = true;
            _s.Release();
        }
    }
}
