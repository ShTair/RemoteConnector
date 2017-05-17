using Microsoft.Win32;
using RemoteConnector.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RemoteConnector.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MachineViewModel> Machines { get; }

        public string Putty
        {
            get { return Settings.Default.PuTTYPath; }
            set { Settings.Default.PuTTYPath = value; }
        }

        public string Session
        {
            get { return Settings.Default.PuTTYSession; }
            set { Settings.Default.PuTTYSession = value; }
        }

        public string WinSCP
        {
            get { return Settings.Default.WinSCPPath; }
            set { Settings.Default.WinSCPPath = value; }
        }

        public string Macs => "";
        //{
        //    get { return Settings.Default.Macs; }
        //    set { Settings.Default.Macs = value; }
        //}

        public string Status
        {
            get { return _Status; }
            set
            {
                if (_Status == value) return;
                _Status = value;
                PropertyChanged?.Invoke(this, _StatusChangedEventArgs);
            }
        }
        private string _Status;
        private PropertyChangedEventArgs _StatusChangedEventArgs = new PropertyChangedEventArgs(nameof(Status));

        public MainViewModel()
        {
            Status = "Ready.";
            Settings.Default.PropertyChanged += (_, e) => PropertyChanged?.Invoke(this, e);
            Settings.Default.PropertyChanged += (_, __) => Settings.Default.Save();
            Machines = new ObservableCollection<MachineViewModel>();
            var t = Refresh();
        }

        public async Task Refresh()
        {
            Status = "Searching...";
            var vms = await Task.Run(() => Check());
            foreach (var item in Machines)
            {
                item.Dispose();
            }
            Machines.Clear();
            foreach (var item in vms)
            {
                item.RunPuTTY += () => StartPuTTY(item.IPAddress);
                item.RunWinSCP += () => StartWinSCP(item.IPAddress);
                Machines.Add(item);
            }
            Status = $"{Machines.Count} RaspberryPis found.";
        }

        private void StartPuTTY(string host)
        {
            if (!File.Exists(Putty))
            {
                Status = "PuTTY Path Required";
                return;
            }
            if (string.IsNullOrWhiteSpace(Session))
            {
                Status = "Session Name Required";
                return;
            }

            var keyName = $@"SOFTWARE\SimonTatham\PuTTY\Sessions\{Session}";
            using (var key = Registry.CurrentUser.OpenSubKey(keyName, true))
            {
                if (key == null)
                {
                    Status = "Not Found Session";
                    return;
                }
                key.SetValue("HostName", "pi@" + host);
            }

            var pi = new ProcessStartInfo
            {
                FileName = Putty,
                Arguments = $"-load {Session} -pw raspberry",
            };
            Process.Start(pi);
            Status = "Connecting...";
        }

        private void StartWinSCP(string host)
        {
            if (!File.Exists(WinSCP))
            {
                Status = "WinSCP Path Required";
                return;
            }

            var pi = new ProcessStartInfo
            {
                FileName = WinSCP,
                Arguments = $"pi:raspberry@{host}",
            };
            Process.Start(pi);
            Status = "Connecting...";
        }

        private async Task<IList<MachineViewModel>> Check()
        {
            var ips = await Dns.GetHostAddressesAsync(Dns.GetHostName());
            var ipv4s = ips.Select(t => t.GetAddressBytes()).Where(t => t.Length == 4).Where(t => t[0] == 192);
            var targets = ipv4s.SelectMany(t => Enumerable.Range(1, 254).Select(i => $"192.168.{t[2]}.{i}"));
            var results = await Task.WhenAll(targets.Select(t =>
            {
                var p = new Ping();
                return p.SendPingAsync(t, 2000);
            }));

            var pi = new ProcessStartInfo
            {
                FileName = "arp",
                Arguments = "-a",
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
            };

            var r = new Regex(@"(\d+\.\d+\.\d+\.\d+)\s+([0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2})", RegexOptions.Compiled);

            string data;
            using (var p = Process.Start(pi))
            {
                data = await p.StandardOutput.ReadToEndAsync();
            }

            var macs = Macs.Split(',');
            var ms = r.Matches(data);
            return (from en in ms.Cast<Match>()
                    let vm = new MachineViewModel(en.Groups[1].Value, en.Groups[2].Value)
                    where macs.Any(t => vm.MacAddress.StartsWith(t))
                    select vm).ToList();
        }
    }
}
