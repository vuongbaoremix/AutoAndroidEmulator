using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAndroidEmulator
{
    public class LDPlayer : AbstractPlayer
    {
        private string _type;
        private string _nameOrId;
        protected string Idx => $"--{this._type} {this._nameOrId}";

        public LDPlayer(PlayerConfig config) : this(0, config)
        {

        }

        public LDPlayer(int index, PlayerConfig config) : base(config)
        {
            this._nameOrId = index.ToString();
            this._type = "index";
        }

        public LDPlayer(string name, PlayerConfig config) : base(config)
        {
            this._nameOrId = name;
            this._type = "name";
        }


        public override bool Connect()
        {
            this.PlayerDevice = this.GetDevicesRunning()
                .FirstOrDefault(x => this._type == "name" ? x.Name == this._nameOrId : x.Index.ToString() == this._nameOrId);

            return base.Connect();
        }
        protected override Rectangle GetClientSize(Size size)
        {
            return new Rectangle(1, 35, size.Width - 2, size.Height - 35);
        }

        public override string ExecuteADB(string cmd)
        {
            if (this.Config.UsePlayerADB)
                return this.Execute($"adb {this.Idx} --command \"{cmd.Replace("\"", "\"\"")}\"");

            return base.ExecuteADB(cmd);
        }

        public override void ActionPlayer(string key, string value)
        {
            this.Execute($"action {this.Idx} --key {key} --value {value}");
        }

        public override void AddPlayer(string name)
        {
            this.Execute($"add --name {name}");
        }

        public override void BackupPlayer(string filePath)
        {
            this.Execute($"backup {this.Idx} --file \"{filePath}\"");
        }

        public override void CopyPlayerTo(string name)
        {
            this.Execute($"copy --name {name} --from {this._nameOrId}");
        }

        public override void DownCPU(string rate)
        {
            this.Execute($"downcpu {this.Idx} --rate {rate}");
        }

        public override List<PlayerDevice> GetDevices()
        {
            List<PlayerDevice> devices = new List<PlayerDevice>();
            string[] row = this.Execute("list2").Trim().Split('\n');

            for (int i = 0; i < row.Length; i++)
            {
                string[] cols = row[i].Trim().Split(',');
                PlayerDevice device = new PlayerDevice();
                var index = 0;
                device.Index = int.Parse(cols[index++]);
                device.Name = cols[1];
                device.TopHandle = new IntPtr(Convert.ToInt32(cols[2], 16));
                device.BindHandle = new IntPtr(Convert.ToInt32(cols[3], 16));
                device.AndroidState = int.Parse(cols[4]);
                device.ProcessID = int.Parse(cols[5]);
                device.VboxPID = int.Parse(cols[6]);

                device.AdbDevice = "-1";

                devices.Add(device);
            }

            return devices;
        }

        public override List<PlayerDevice> GetDevicesRunning()
        {
            var devices = this.GetDevices();
            var running = this.RunningList();
            var adbDevices = this.Adb.Devices();
            int i = 0;
            return devices.Where(x =>
            {
                var isRunning = running.Contains(x.Name);
                if (isRunning && adbDevices.Count > i)
                {
                    x.AdbDevice = adbDevices[i++];
                }

                return isRunning;
            }).ToList();
        }

        public override IntPtr GetHandle()
        {
            var player = this.PlayerDevice;
            if (player == null || player.ProcessID == 0)
            {
                if (!this.Connect())
                {
                    throw new Exception($"Notfound {this.Idx}");
                }
            }
            var p = Process.GetProcessById(player.ProcessID);

            this.PlayerHandle = p?.MainWindowHandle;

            return this.PlayerHandle ?? IntPtr.Zero;
        }

        public override string GetProp(string key)
        {
            return this.Execute($"getprop {this.Idx} --key {key}");
        }

        public override void GolabalSetting(int fps, bool audio, bool fastPlay, bool cleanMode)
        {
            this.Execute($"globalsetting --fps {fps} --audio {(audio ? 1 : 0)} --fastplay {(fastPlay ? 1 : 0)} --cleanmode {(cleanMode ? 1 : 0)}");
        }

        public override void InstallAppFromPackage(string packageName)
        {
            this.Execute($"installapp {this.Idx} --packagename {packageName}");
        }

        public override bool IsRunning()
        {
            var rs = this.Execute($"isrunning {this.Idx}").Trim();

            return rs.Contains("running");
        }

        public override void LaunchAndOpenApp(string package)
        {
            this.Execute($"launchex {this.Idx} --packagename {package}");
        }

        public override void LaunchPlayer()
        {
            this.Execute($"launch {this.Idx}");
        }

        public override List<string> List()
        {
            var devices = this.Execute("list")
               .Trim()
               .Split('\n')
               .Select(x => x.Trim())
               .Where(x => !string.IsNullOrEmpty(x))
               .ToList();

            return devices;
        }

        public override void Locate(string lng, string lat)
        {
            this.Execute($"locate {this.Idx} --LLI {lng},{lat}");
        }

        /// <summary>
        /// [--resolution <w,h,dpi>]
        /// [--cpu<1 | 2 | 3 | 4>]
        /// [--memory<256 | 512 | 768 | 1024 | 1536 | 2048 | 4096 | 8192>]
        /// [--manufacturer asus]
        /// [--model ASUS_Z00DUO]
        /// [--pnumber 13800000000]
        /// [--imei<auto | 865166023949731>]
        /// [--imsi<auto | 460000000000000>]
        /// [--simserial<auto | 89860000000000000000>]
        /// [--androidid<auto | 0123456789abcdef>]
        /// [--mac<auto | 000000000000>]
        /// [--autorotate<1 | 0>
        /// [--lockwindow<1 | 0>
        /// </summary>
        /// <param name="cmd"></param>
        public override void ModifyPlayerProperty(string cmd)
        {
            this.Execute($"modify {this.Idx} {cmd}");
        }

        public override void QuitAllPlayer()
        {
            this.Execute($"quitall");
        }

        public override void QuitPlayer()
        {
            this.Execute($"quit {this.Idx}");
        }

        public override void ReBootPlayer()
        {
            this.Execute($"reboot {this.Idx}");
        }

        public override void RemovePlayer()
        {
            this.Execute($"remove {this.Idx}");

        }

        public override void ReNamePlayer(string newName)
        {
            this.Execute($"rename {this.Idx} --title {newName}");
        }

        public override void RestorePlayer(string filePath)
        {
            this.Execute($"restore {this.Idx} --file \"{filePath}\"");
        }

        public override List<string> RunningList()
        {
            var devices = this.Execute("runninglist")
                .Trim()
                .Split('\n')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            return devices;
        }

        public override void Scan(string filePath)
        {
            this.Execute($"scan {this.Idx} --file \"{filePath}\"");
        }

        public override void SetProp(string key, string value)
        {
            this.Execute($"setprop {this.Idx} --key {key} --value {value}");
        }

        public override void SortWnd()
        {
            this.Execute($"sortWnd");
        }

        public override void ZoomIn()
        {
            this.Execute($"zoomIn {this.Idx}");
        }

        public override void ZoomOut()
        {
            this.Execute($"zoomOut {this.Idx}");

        }
    }
}
