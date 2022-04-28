using System;
using System.Collections.Generic;
using System.Drawing;

namespace AutoAndroidEmulator
{
    public interface IPlayer
    {
        void QuitPlayer();
        void QuitAllPlayer();
        void LaunchPlayer();
        void ReBootPlayer();
        List<string> List();
        List<string> RunningList();
        bool IsRunning();

        void LaunchAndOpenApp(string package);

        void AddPlayer(string Name);
        void CopyPlayerTo(string name);
        void RemovePlayer();
        void ReNamePlayer(string newName);
        void ModifyPlayerProperty(string cmd);
        void BackupPlayer(string filePath);
        void RestorePlayer(string filePath);
        void ActionPlayer(string key, string value);
        void Scan(string filePath);

        void InstallAppFromFile(string fileName);
        void InstallAppFromPackage(string packageName);
        void UnInstallApp(string packageName);
        void OpenApp(string packageName);
        void KillApp(string packageName);
        void ClearAppData(string packageName);
        void Locate(string lng, string lat);
        void GolabalSetting(int fps, bool audio, bool fastPlay, bool cleanMode);

        List<PlayerDevice> GetDevices();
        List<PlayerDevice> GetDevicesRunning();

        string Execute(string cmd);
        string ExecuteADB(string cmd);

        void Pull(string remote, string local);

        void Push(string remote, string local);

        void BackupApp(string packageName, string filePath, Action<string> callbackBeforeCompress = null);

        void RestoreApp(string packageName, string filePath);

        void SetProp(string key, string value);
        string GetProp(string key);

        void DownCPU(string rate);
        void Back();
        void Home();
        void Menu();


        void SortWnd();
        void ZoomIn();
        void ZoomOut();

        void ChangeProxy(string address, int port);
        void RemoveProxy();

        Bitmap Capture(Rectangle? rec = null);

        Rectangle FindImg(Bitmap img, Rectangle? rec = null, TimeSpan? timeout = null, double tolerance = 0.2);
        Rectangle FindImg(Bitmap img, Bitmap source, TimeSpan? timeout = null, double tolerance = 0.2);

        bool WaitImg(Bitmap img, TimeSpan? timeout = null, double tolerance = 0.2, bool touch = false);
        bool WaitImg(Bitmap img, Rectangle rec, TimeSpan? timeout = null, double tolerance = 0.2, bool touch = false);
        bool WaitImg(Bitmap source, Bitmap img, TimeSpan? timeout = null, double tolerance = 0.2, bool touch = false);

        void Touch(Bitmap bm, double tolerance = 0.2);
        void Touch(Point p);
        void Input(string text);
        void ClearInput(int length = 10);

        IPInfo GetIPInfo();
    }
}
