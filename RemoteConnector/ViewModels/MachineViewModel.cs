using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteConnector.ViewModels
{
    class MachineViewModel : ICommand, INotifyPropertyChanged, IDisposable
    {
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

            //Task.Run(() => LoadName());
            //Task.Run(() => Pinging());
        }

        //private async Task LoadName()
        //{
        //    using (var c = RPiContext.Create())
        //    {
        //        var m = await c.PiNames.FindAsync(MacAddress);
        //        if (string.IsNullOrWhiteSpace(m?.Name)) Name = "Unknown";
        //        else Name = m.Name;
        //    }
        //}

        //public async Task UpdateName(string name)
        //{
        //    Name = "Updating...";
        //    using (var c = RPiContext.Create())
        //    {
        //        var m = await c.PiNames.FindAsync(MacAddress);
        //        if (m == null) c.PiNames.Add(m = new PiName { Id = MacAddress });
        //        m.Name = name;
        //        await c.SaveChangesAsync();
        //    }
        //    await LoadName();
        //}

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            //    switch ((string)parameter)
            //    {
            //        case "PuTTY": RunPuTTY?.Invoke(); break;
            //        case "WinSCP": RunWinSCP?.Invoke(); break;
            //    }
        }

        //private async void Pinging()
        //{
        //    var ping = new Ping();
        //    while (!IsDisposed)
        //    {
        //        var result = await ping.SendPingAsync(IPAddress);
        //        Status = (result.Status == IPStatus.Success) ? "○" : "×";
        //        await Task.Delay(10000);
        //    }
        //}

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
