using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAndroidEmulator
{
    public class PlayerDevice
    {
        public int Index;
        public string Name;
        public int ProcessID;
        public int VboxPID;
        public string AdbDevice;
        public IntPtr TopHandle;
        public IntPtr BindHandle;
        public int AndroidState;
    }
}
