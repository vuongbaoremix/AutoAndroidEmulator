using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoAndroidEmulator
{
    public enum CaptureType
    {
        WinApi = 0,
        Adb,
        // TODO: Implement
        Minicap
    }

    public enum TouchType
    {
        WinApi = 0,
        Adb,
        // TODO: Implement 
        MouseClick
    }

    public enum InputType
    {
        WinApi = 0,
        Adb
    }

    public enum FindImageType
    {
        Default = 0,
        // TODO: Implement
        OpenCV,
        DeepLearning,
        Service
    }

    public class PlayerConfig
    {
        public CaptureType CaptureType = CaptureType.WinApi;
        public TouchType TouchType = TouchType.WinApi;
        public InputType InputType = InputType.WinApi;
        public FindImageType FindImageType = FindImageType.Default;

        public string CLIPath = "";
        public string ADBPath = "";

        public bool UsePlayerADB = true;

        public int Delay = 50;
    }


    public abstract class AbstractPlayer : IPlayer
    {
        public PlayerConfig Config { protected set; get; }
        public PlayerDevice PlayerDevice { protected set; get; } = null;
        public IntPtr? PlayerHandle { protected set; get; }
        public Adb Adb { protected set; get; }

        public AbstractPlayer(PlayerConfig config)
        {
            this.Config = config;
            this.Adb = new Adb(config.ADBPath);
        }

        public virtual bool Connect()
        {
            if (this.PlayerDevice != null)
            {
                var p = Process.GetProcessById(this.PlayerDevice.ProcessID);
                this.PlayerHandle = p.MainWindowHandle;

                this.Adb.SetDevice(this.PlayerDevice.AdbDevice);
                this.Adb.FuncExecute = this.ExecuteADB;

                return true;
            }

            return false;
        }

        protected T FuncTimeout<T>(Func<T> callback, TimeSpan? timeout)
        {
            var time = Environment.TickCount;

            do
            {
                var rs = callback();
                if (!Utils.IsDefault(rs))
                    return rs;

                Thread.Sleep(this.Config.Delay);
            } while (timeout != null && (Environment.TickCount - time < timeout.Value.TotalMilliseconds));

            return default(T);
        }

        protected virtual Rectangle GetClientSize(Size size)
        {
            return Rectangle.Empty;
        }

        public virtual Bitmap Capture(Rectangle? rec = null)
        {
            Bitmap bm = null;
            switch (this.Config.CaptureType)
            {
                case CaptureType.WinApi:
                    bm = WinApi.ScreenCapture.GetScreenshot(this.PlayerHandle ?? GetHandle());
                    break;
                case CaptureType.Adb:
                    bm = this.Adb.Capture();
                    break;
                case CaptureType.Minicap:
                    break;
                default:
                    break;
            }
            if (bm == null)
                throw new NotImplementedException(nameof(CaptureType));

            var clientSize = GetClientSize(bm.Size);
            if (!clientSize.IsEmpty)
            {
                bm = Utils.CropImage(bm, clientSize);
            }

            if (rec != null)
                bm = Utils.CropImage(bm, rec.Value);

            return bm;
        }

        public virtual string Execute(string cmd)
        {
            return Utils.ExecuteCMD($"{this.Config.CLIPath} {cmd}");
        }

        public virtual string ExecuteADB(string cmd)
        {
            string s = "";

            if (string.IsNullOrEmpty(this.PlayerDevice?.AdbDevice ?? ""))
                s = Utils.ExecuteCMD($"{this.Config.ADBPath}/adb.exe {cmd}");

            else
                s = Utils.ExecuteCMD($"{this.Config.ADBPath}/adb.exe -s {this.PlayerDevice.AdbDevice} {cmd}");

            if (s.Contains("device not found"))
            {
                throw new DeviceNotFoundException();
            }

            return s;
        }

        protected virtual Rectangle InternalFind(Bitmap img, Bitmap source, double tolerance = 0.2)
        {
            var w = Stopwatch.StartNew();
            var searchImage = (Bitmap)img.Clone();
            try
            {
                switch (this.Config.FindImageType)
                {
                    case FindImageType.Default:
                        return Utils.Search(searchImage, source, tolerance);
                    case FindImageType.OpenCV:
                        break;
                    case FindImageType.DeepLearning:
                        break;
                    case FindImageType.Service:
                        break;
                    default:
                        break;
                }
            }
            finally
            {
                w.Stop();
                System.Diagnostics.Debug.WriteLine($"FindImage Time: {w.ElapsedMilliseconds}");
                searchImage.Dispose();
            }

            return Rectangle.Empty;
        }

        public virtual Rectangle FindImg(Bitmap img, Rectangle? rec = null, TimeSpan? timeout = null, double tolerance = 0.2)
        {
            return FuncTimeout(() =>
            {
                var source = this.Capture(rec);
                return InternalFind(img, source, tolerance);
            }, timeout);
        }

        public virtual Rectangle FindImg(Bitmap img, Bitmap source, TimeSpan? timeout = null, double tolerance = 0.2)
        {
            return FuncTimeout(() =>
            {
                return InternalFind(img, source, tolerance);
            }, timeout);
        }

        public bool WaitImg(Bitmap img, TimeSpan? timeout = null, double tolerance = 0.2, bool touch = false)
        {
            return FuncTimeout(() =>
            {
                var source = this.Capture();
                try
                {
                    var rs = InternalFind(img, source, tolerance);
                    if (touch && !rs.IsEmpty)
                    {
                        this.Touch(new Point(rs.X + rs.Width / 2, rs.Y + rs.Height / 2));
                    }

                    return !rs.IsEmpty;
                }
                finally
                {
                    source.Dispose();
                    GC.Collect();
                }
            }, timeout);
        }

        public bool WaitImg(Bitmap img, Rectangle rec, TimeSpan? timeout = null, double tolerance = 0.2, bool touch = false)
        {
            return FuncTimeout(() =>
            {
                var source = this.Capture(rec);
                var rs = InternalFind(img, source, tolerance);
                if (touch && !rs.IsEmpty)
                {
                    this.Touch(new Point(rs.X + rs.Width / 2, rs.Y + rs.Height / 2));
                }

                return !rs.IsEmpty;
            }, timeout);
        }

        public bool WaitImg(Bitmap source, Bitmap img, TimeSpan? timeout = null, double tolerance = 0.2, bool touch = false)
        {
            return FuncTimeout(() =>
            {
                var rs = InternalFind(img, source, tolerance);
                if (touch && !rs.IsEmpty)
                {
                    this.Touch(new Point(rs.X + rs.Width / 2, rs.Y + rs.Height / 2));
                }

                return !rs.IsEmpty;
            }, timeout);
        }

        public virtual void Touch(Bitmap bm, double tolerance = 0.2)
        {
            var rec = this.FindImg(bm, tolerance: tolerance);
            if (rec.Width == 0)
                return;

            this.Touch(new Point(rec.X + rec.Width / 2, rec.Y + rec.Height / 2));
        }

        public virtual void Touch(Point p)
        {
            switch (this.Config.TouchType)
            {
                case TouchType.WinApi:
                    WinApi.MouseAndKeyboard.Click(this.PlayerHandle ?? this.GetHandle(), p.X, p.Y);
                    break;
                case TouchType.Adb:
                    this.Adb.Tap(p.X, p.Y);
                    break;
                case TouchType.MouseClick:
                    break;
                default:
                    break;
            }
        }

        public virtual void ChangeProxy(string address, int port)
        {
            this.Adb.ChangeProxy(address, port);
        }

        public virtual void ClearAppData(string packageName)
        {
            this.Adb.ClearAppData(packageName);
        }

        public virtual void Back()
        {
            this.Adb.Key(Adb.KeyCode.KEYCODE_BACK);
        }

        public virtual void Home()
        {
            this.Adb.Key(Adb.KeyCode.KEYCODE_HOME);
        }

        public virtual void Input(string text)
        {
            switch (this.Config.InputType)
            {
                case InputType.WinApi:
                    WinApi.MouseAndKeyboard.SendKey(this.PlayerHandle ?? GetHandle(), text);
                    break;
                case InputType.Adb:
                    this.Adb.InputText(text);
                    break;
            }
        }

        public virtual void UnInstallApp(string packageName)
        {
            this.Adb.UnInstallApp(packageName);
        }

        public virtual void Menu()
        {
            this.Adb.Key(Adb.KeyCode.KEYCODE_MENU);
        }


        public virtual void Pull(string remote, string local)
        {
            this.Adb.Pull(remote, local);
        }

        public virtual void Push(string remote, string local)
        {
            this.Adb.Push(remote, local);
        }

        public virtual void RemoveProxy()
        {
            this.Adb.RemoveProxy();
        }

        public virtual void OpenApp(string packageName)
        {
            this.Adb.OpenApp(packageName);
        }

        public virtual void OpenLink(string link, string packge = null)
        {
            this.Adb.OpenLink(link, packge);
        }

        public virtual void InstallAppFromFile(string fileName)
        {
            this.Adb.Install(fileName);
        }

        public virtual void KillApp(string packageName)
        {
            this.Adb.Kill(packageName);
        }

        public virtual void ClearInput(int length = 10)
        {
            switch (this.Config.InputType)
            {
                case InputType.WinApi:
                    for (int i = 0; i < length; i++)
                    {
                        WinApi.MouseAndKeyboard.SendKey(this.PlayerHandle ?? this.GetHandle(), ConsoleKey.Backspace);
                        Thread.Sleep(10);
                    }
                    break;

                case InputType.Adb:
                    this.Adb.ClearInput();
                    break;
            }
        }


        public virtual void BackupApp(string packageName, string filePath, Action<string> callbackBeforeCompress = null)
        {
            var tmpPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(tmpPath))
                Directory.CreateDirectory(tmpPath);

            //set premission
            this.ExecuteADB($@"shell ""su -c 'chmod 777 /data/data/{packageName}'""");
            this.ExecuteADB($@"shell ""su -c 'chmod 777 /data/data/{packageName}/shared_prefs'""");
            this.ExecuteADB($@"shell ""su -c 'chmod 777 /data/data/{packageName}/shared_prefs/*.*'""");

            //pull
            this.Pull($"/data/data/{packageName}", tmpPath);

            if (callbackBeforeCompress != null)
            {
                callbackBeforeCompress(tmpPath); 
            }

            File.WriteAllText(Path.Combine(tmpPath, "package.txt"), packageName);

            ZipFile.CreateFromDirectory(tmpPath, filePath);

            Directory.Delete(tmpPath, true);
        }

        public virtual void RestoreApp(string packageName, string filePath)
        {
            var tmpPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var packageInfoPath = Path.Combine(tmpPath, "package.txt");

            ZipFile.ExtractToDirectory(filePath, tmpPath);

            string oldpackage = packageName;
            if (File.Exists(packageInfoPath))
            {
                oldpackage = File.ReadAllText(packageInfoPath);
            }

            this.ExecuteADB($@"shell ""su -c 'chmod 777 /data/data/{packageName}'""");
            this.ExecuteADB($@"shell mkdir /data/data/{packageName}/shared_prefs");
            this.ExecuteADB($@"shell ""su -c 'chmod 777 /data/data/{packageName}/shared_prefs'""");

            string[] files = Directory.GetFiles(Path.Combine(tmpPath, "shared_prefs"));
            foreach (string file in files)
            {
                string newPath = file;
                string fileName = Path.GetFileName(file);

                if (fileName.Contains(oldpackage))
                {
                    fileName = fileName.Replace(oldpackage, packageName);
                    newPath = Path.Combine(tmpPath, "shared_prefs", fileName);
                    File.Move(file, newPath);
                }
            }

            this.Push($"/data/data/{packageName}", tmpPath);

            Directory.Delete(tmpPath, true);
        }

        public abstract void ActionPlayer(string key, string value);

        public abstract void AddPlayer(string Name);

        public abstract void BackupPlayer(string filePath);

        public abstract bool IsRunning();

        public abstract IntPtr GetHandle();

        public abstract void CopyPlayerTo(string name);

        public abstract void DownCPU(string rate);

        public abstract List<PlayerDevice> GetDevices();

        public abstract List<PlayerDevice> GetDevicesRunning();

        public abstract List<string> List();

        public abstract List<string> RunningList();

        public abstract string GetProp(string key);

        public abstract void GolabalSetting(int fps, bool audio, bool fastPlay, bool cleanMode);

        public abstract void InstallAppFromPackage(string packageName);

        public abstract void LaunchAndOpenApp(string package);

        public abstract void LaunchPlayer();

        public abstract void Locate(string lng, string lat);

        public abstract void ModifyPlayerProperty(string cmd);

        public abstract void QuitAllPlayer();

        public abstract void QuitPlayer();

        public abstract void ReBootPlayer();

        public abstract void RemovePlayer();

        public abstract void ReNamePlayer(string newName);

        public abstract void RestorePlayer(string filePath);

        public abstract void Scan(string filePath);

        public abstract void SetProp(string key, string value);

        public abstract void SortWnd();

        public abstract void ZoomIn();

        public abstract void ZoomOut();
    }
}
