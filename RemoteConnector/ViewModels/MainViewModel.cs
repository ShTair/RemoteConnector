using Microsoft.Win32;
using Newtonsoft.Json;
using RemoteConnector.Models;
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

        private Dictionary<string, MachineInfo> _minfos;

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

            LoadMachineInfos();

            var t = Refresh();
        }

        private void LoadMachineInfos()
        {
            try
            {
                var minfos = JsonConvert.DeserializeObject<List<MachineInfo>>(Settings.Default.MachineInfos);
                _minfos = minfos.Distinct().ToDictionary(t => t.MacAddress);
            }
            catch
            {
                _minfos = new Dictionary<string, MachineInfo>();
            }
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
                item.RunPuTTY += () => StartPuTTY(item);
                item.RunWinSCP += () => StartWinSCP(item);
                item.RunBrowser += () => StartBrowser(item);
                Machines.Add(item);
            }

            Status = $"{Machines.Count} devices found.";
        }

        private void StartPuTTY(MachineViewModel machine)
        {
            if (!File.Exists(Settings.Default.PuTTYPath))
            {
                Status = "PuTTY Path Required";
                return;
            }
            if (string.IsNullOrWhiteSpace(machine.MachineInfo.PuTTYSession))
            {
                Status = "Session Name Required";
                return;
            }

            var keyName = $@"SOFTWARE\SimonTatham\PuTTY\Sessions\{machine.MachineInfo.PuTTYSession}";
            using (var key = Registry.CurrentUser.OpenSubKey(keyName, true))
            {
                if (key == null)
                {
                    Status = "Not Found Session";
                    return;
                }
                key.SetValue("HostName", $"{machine.MachineInfo.UserName}@{machine.IPAddress}");
            }

            var pi = new ProcessStartInfo
            {
                FileName = Settings.Default.PuTTYPath,
                Arguments = $"-load {machine.MachineInfo.PuTTYSession} -pw {machine.MachineInfo.Password}",
            };
            Process.Start(pi);
            Status = "Connecting...";
        }

        private void StartWinSCP(MachineViewModel machine)
        {
            if (!File.Exists(Settings.Default.WinSCPPath))
            {
                Status = "WinSCP Path Required";
                return;
            }

            var pi = new ProcessStartInfo
            {
                FileName = Settings.Default.WinSCPPath,
                Arguments = $"{machine.MachineInfo.UserName}:{machine.MachineInfo.Password}@{machine.IPAddress}",
            };
            Process.Start(pi);
            Status = "Connecting...";
        }

        private void StartBrowser(MachineViewModel machine)
        {
            Process.Start($"http://{machine.IPAddress}");
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

            var ms = r.Matches(data);
            return (from en in ms.Cast<Match>()
                    let mi = GetMachineInfo(en.Groups[2].Value)
                    where mi != null
                    select new MachineViewModel(en.Groups[1].Value, mi)).ToList();
        }

        private MachineInfo GetMachineInfo(string mac)
        {
            if (Settings.Default.MacAddresses.Cast<string>().Any(t => Regex.IsMatch(mac, t))
                || _minfos.ContainsKey(mac))
            {
                MachineInfo mi;
                if (!_minfos.TryGetValue(mac, out mi))
                {
                    mi = new MachineInfo { MacAddress = mac, Name = mac };
                    if (mac.StartsWith("b8-27-eb"))
                    {
                        mi.UserName = "pi";
                        mi.Password = "raspberry";
                        mi.PuTTYSession = "RaspberryPI";
                    }

                    _minfos[mac] = mi;
                    SaveMachineInfos();
                }

                return mi;
            }

            return null;
        }

        private void SaveMachineInfos()
        {
            var json = JsonConvert.SerializeObject(_minfos.Values);
            Settings.Default.MachineInfos = json;
            Settings.Default.Save();
        }
    }
}
