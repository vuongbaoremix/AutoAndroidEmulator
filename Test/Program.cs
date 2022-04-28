using AutoAndroidEmulator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string adbPath = @"G:\LDPlayer\LDPlayer3.0";
            string cliPath = @"G:\LDPlayer\LDPlayer3.0\ldconsole.exe";

            var player = new LDPlayer("LDPlayer-2", new PlayerConfig()
            {
                ADBPath = adbPath,
                CLIPath = cliPath,
                UsePlayerADB = true,
                CaptureType = CaptureType.Adb
            });

            player.Connect();


            var bm = player.Capture();
        }
    }
}
