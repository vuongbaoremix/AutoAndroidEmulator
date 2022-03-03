using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAndroidEmulatorTests
{
    internal static class Const
    {
        public const string LDPlayerCLIPath = @"G:\ld-RONIN-4.0\ld-RONIN-4.0\ldconsole.exe";
        public const string ADBPath = @"G:\ld-RONIN-4.0\ld-RONIN-4.0";
        public const string PlayerTest1 = "LDPlayer-Test1";
        public const string PlayerTest2 = "LDPlayer-Test2";

        static Bitmap loadImage(string name)
        {
            return new Bitmap($"data/{name}.png");
        }

        public static readonly Bitmap IMG_LOADED = loadImage("loaded");
    }
}
