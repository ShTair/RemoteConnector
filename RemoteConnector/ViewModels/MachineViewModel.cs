using RemoteConnector.Models;
using RemoteConnector.Properties;
using System;
using System.ComponentModel;
using System.IO;
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
        public event Action RunBrowser;
        public event EventHandler CanExecuteChanged;

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        public MachineInfo MachineInfo { get; }

        public string IPAddress { get; }

        public string ImageUrl => string.Format(Settings.Default.IconBaseUrl, MachineInfo.MacAddress.GetHashCode());

        public bool IsDisposed { get; private set; }

        public string Status
        {
            get { return _Status; }
            private set
            {
                if (_Status == value) return;
                _Status = value;
                PropertyChanged?.Invoke(this, _StatusChangedEventArgs);
                File.AppendAllLines($"log_{MachineInfo.Name}_{MachineInfo.MacAddress}.txt", new[] { $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}\t{Status}\t{IPAddress}" });
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
                case "Browser": RunBrowser?.Invoke(); break;
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
                    var result = await ping.SendPingAsync(IPAddress, 10000);
                    Status = (result.Status == IPStatus.Success) ? "○" : "×";
                }
                finally { _s.Release(); }
                await Task.Delay(20000);
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
