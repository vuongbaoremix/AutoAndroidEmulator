using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAndroidEmulator
{
    public class DeviceNotFoundException : Exception
    {
        public DeviceNotFoundException() : base("Device not found")
        {
        }

        public DeviceNotFoundException(string message) : base(message)
        {
        }
    }
}
