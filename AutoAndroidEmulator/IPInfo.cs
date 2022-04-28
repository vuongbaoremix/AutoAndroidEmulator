using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoAndroidEmulator
{
    [Obfuscation(Exclude = false, Feature = "-rename")] 
    public class IPInfo
    {
        public string IP { set; get; }
        public string HostName { set; get; }
        public string City { set; get; }
        public string Region { set; get; }
        public string Country { set; get; }
        public string Loc { set; get; }
        public string Org { set; get; }
        public string Postal { set; get; }
        public string Timezone { set; get; } 
    }
}
