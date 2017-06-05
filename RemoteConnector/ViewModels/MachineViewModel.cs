using RemoteConnector.Models;
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

        public MachineInfo MachineInfo { get; }

        public string IPAddress { get; }

        public string ImageUrl => $"https://robohash.org/{MachineInfo.MacAddress.GetHashCode()}?set=set3&bgset=bg1&size=40x40";

        public bool IsDisposed { get; private set; }

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

        public MachineViewModel(string ip, MachineInfo mi)
        {
            IPAddress = ip;
            MachineInfo = mi;

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
