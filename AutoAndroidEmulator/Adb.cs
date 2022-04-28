using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoAndroidEmulator
{

    public class Adb
    {
        public enum KeyCode
        {
            KEYCODE_0 = 0,
            KEYCODE_SOFT_LEFT = 1,
            KEYCODE_SOFT_RIGHT = 2,
            KEYCODE_HOME = 3,
            KEYCODE_BACK = 4,
            KEYCODE_CALL = 5,
            KEYCODE_ENDCALL = 6,
            KEYCODE_0_ = 7,
            KEYCODE_1 = 8,
            KEYCODE_2 = 9,
            KEYCODE_3 = 10,
            KEYCODE_4 = 11,
            KEYCODE_5 = 12,
            KEYCODE_6 = 13,
            KEYCODE_7 = 14,
            KEYCODE_8 = 0xF,
            KEYCODE_9 = 0x10,
            KEYCODE_STAR = 17,
            KEYCODE_POUND = 18,
            KEYCODE_DPAD_UP = 19,
            KEYCODE_DPAD_DOWN = 20,
            KEYCODE_DPAD_LEFT = 21,
            KEYCODE_DPAD_RIGHT = 22,
            KEYCODE_DPAD_CENTER = 23,
            KEYCODE_VOLUME_UP = 24,
            KEYCODE_VOLUME_DOWN = 25,
            KEYCODE_POWER = 26,
            KEYCODE_CAMERA = 27,
            KEYCODE_CLEAR = 28,
            KEYCODE_A = 29,
            KEYCODE_B = 30,
            KEYCODE_C = 0x1F,
            KEYCODE_D = 0x20,
            KEYCODE_E = 33,
            KEYCODE_F = 34,
            KEYCODE_G = 35,
            KEYCODE_H = 36,
            KEYCODE_I = 37,
            KEYCODE_J = 38,
            KEYCODE_K = 39,
            KEYCODE_L = 40,
            KEYCODE_M = 41,
            KEYCODE_N = 42,
            KEYCODE_O = 43,
            KEYCODE_P = 44,
            KEYCODE_Q = 45,
            KEYCODE_R = 46,
            KEYCODE_S = 47,
            KEYCODE_T = 48,
            KEYCODE_U = 49,
            KEYCODE_V = 50,
            KEYCODE_W = 51,
            KEYCODE_X = 52,
            KEYCODE_Y = 53,
            KEYCODE_Z = 54,
            KEYCODE_COMMA = 55,
            KEYCODE_PERIOD = 56,
            KEYCODE_ALT_LEFT = 57,
            KEYCODE_ALT_RIGHT = 58,
            KEYCODE_SHIFT_LEFT = 59,
            KEYCODE_SHIFT_RIGHT = 60,
            KEYCODE_TAB = 61,
            KEYCODE_SPACE = 62,
            KEYCODE_SYM = 0x3F,
            KEYCODE_EXPLORER = 0x40,
            KEYCODE_ENVELOPE = 65,
            KEYCODE_ENTER = 66,
            KEYCODE_DEL = 67,
            KEYCODE_GRAVE = 68,
            KEYCODE_MINUS = 69,
            KEYCODE_EQUALS = 70,
            KEYCODE_LEFT_BRACKET = 71,
            KEYCODE_RIGHT_BRACKET = 72,
            KEYCODE_BACKSLASH = 73,
            KEYCODE_SEMICOLON = 74,
            KEYCODE_APOSTROPHE = 75,
            KEYCODE_SLASH = 76,
            KEYCODE_AT = 77,
            KEYCODE_NUM = 78,
            KEYCODE_HEADSETHOOK = 79,
            KEYCODE_FOCUS = 80,
            KEYCODE_PLUS = 81,
            KEYCODE_MENU = 82,
            KEYCODE_NOTIFICATION = 83,
            KEYCODE_APP_SWITCH = 187

        }

        public string AdbDevice = "";
        public string ADBPath = "";

        public Func<string, string> FuncExecute = null;
        public Func<string, Stream> FuncGetStdout = null;

        public Adb(string adbPath)
        {
            this.ADBPath = adbPath;
        }

        public void SetDevice(string device)
        {
            this.AdbDevice = device;
        }


        public string Execute(string cmd)
        {
            if (FuncExecute != null)
                return FuncExecute(cmd);

            if (string.IsNullOrEmpty(this.AdbDevice))
                return Utils.ExecuteCMD($"{this.ADBPath}/adb.exe {cmd}");

            return Utils.ExecuteCMD($"{this.ADBPath}/adb.exe -s {this.AdbDevice} {cmd}");
        }

        public void Tap(int x, int y)
        {
            this.Execute($"shell input tap {x} {y}");
        }

        public void InputText(string text)
        {
            this.Execute($"shell input text \"{text}\"");
        }

        public void Key(KeyCode key)
        {
            this.Execute($"shell input keyevent {key}");
        }

        public void ChangeProxy(string address, int port)
        {
            Execute($"shell settings put global http_proxy {address}:{port}");
        }
        public void ClearAppData(string packageName)
        {
            Execute($"shell pm clear {packageName}");
        }

        public void Pull(string remote, string local)
        {
            Execute($"pull {remote} \"{local}\"");
        }

        public void Push(string remote, string local)
        {
            Execute($"push \"{local}\" {remote}");
        }

        public void RemoveProxy()
        {
            Execute("shell settings put global http_proxy :0");
        }

        public void OpenLink(string link, string package = null)
        {
            if (string.IsNullOrEmpty(package))
                package = "android.intent.action.VIEW";

            Execute($"shell \"am start -a {package} -d {link.Replace("&", @"\&")}\"");
        }

        public void OpenApp(string packageName)
        {
            Execute($"shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1");

            //Execute($"shell am start -n {packageName}/.ActivityName");
        }

        public void UnInstallApp(string packageName)
        {
            Execute($"uninstall {packageName}");
        }

        public void Kill(string packageName)
        {
            Execute($"shell am force-stop {packageName}");
        }

        public void Install(string apkPath)
        {
            Execute($"install {apkPath}");
        }

        public void ClearInput()
        {
            for (int i = 0; i < 15; i++)
            {
                Key(KeyCode.KEYCODE_DEL);
            }
        }

        public List<string> Devices()
        {
            List<string> list = new List<string>();
            string input = this.Execute("devices");
            string pattern = "(?<=List of devices attached)([^\\n]*\\n+)+";
            MatchCollection matchCollection = Regex.Matches(input, pattern, RegexOptions.Singleline);
            if (matchCollection.Count > 0)
            {
                string value = matchCollection[0].Groups[0].Value;
                string[] array = Regex.Split(value, "\r\n");
                string[] array2 = array;
                foreach (string text in array2)
                {
                    if (string.IsNullOrEmpty(text) || !(text != " "))
                    {
                        continue;
                    }

                    string[] array3 = text.Trim().Split('\t');
                    string text2 = array3[0];
                    string text3 = "";
                    try
                    {
                        text3 = array3[1];
                        if (text3 != "device")
                        {
                            continue;
                        }
                    }
                    catch
                    {
                    }

                    list.Add(text2.Trim());
                }
            }

            return list.Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        static byte[] repair(byte[] encoded)
        {
            List<byte> rs = new List<byte>(); 

            for (int i = 0; i < encoded.Length; i++)
            { 
                if (encoded.Length > i + 2 && encoded[i] == 0x0d && encoded[i + 1] == 0x0d && encoded[i + 2] == 0x0a)
                {
                    rs.Add(0x0a);
                    i += 2; 
                }
                else if (encoded.Length > i + 1 && encoded[i] == 0x0d && encoded[i + 1] == 0x0a)
                {
                    rs.Add(0x0a);
                    i ++; 
                }
                else
                {
                    rs.Add(encoded[i]);
                }
            }

            return rs.ToArray();
        }

        public Bitmap Capture(Rectangle? rec = null)
        {
            Process p;
            if (string.IsNullOrEmpty(this.AdbDevice))
                p = Utils.StartProcess($"{this.ADBPath}/adb.exe", "shell screencap -p");
            else
                p = Utils.StartProcess($"{this.ADBPath}/adb.exe", $"-s {this.AdbDevice} shell screencap -p");

            var stream = new MemoryStream();
            p.StandardOutput.BaseStream.CopyTo(stream);

            var buffer = repair(stream.ToArray());
            //List<byte> data = new List<byte>(1024 * 1024); 
            //int read = 0;
            //bool isCR = false;
            //do
            //{
            //    byte[] buf = new byte[1024];
            //    read = stream.Read(buf, 0, buf.Length);

            //    //convert CRLF to LF 
            //    for (int i = 0; i < read; i++)
            //    {
            //        if (isCR && buf[i] == 0x0A)
            //        {
            //            isCR = false;
            //            data.RemoveAt(data.Count - 1);
            //            data.Add(buf[i]);
            //            continue;
            //        }
            //        isCR = buf[i] == 0x0D;
            //        data.Add(buf[i]);
            //    }
            //}
            //while (read > 0);


            // File.WriteAllBytes("test.png", buffer);
            var m = new MemoryStream(buffer);
            var bm = new Bitmap(m);

            if (rec != null)
                return Utils.CropImage(bm, rec.Value);

            return bm;
        }

        public void KillServer()
        {
            Utils.StartProcess($"{this.ADBPath}/adb.exe", "kill-server", false, true).WaitForExit();
        }

        public void StartServer()
        {
            Utils.StartProcess($"{this.ADBPath}/adb.exe", "start-server", false, true).WaitForExit();
        }
    }
}
