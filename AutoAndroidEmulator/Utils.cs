using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAndroidEmulator
{
    public static class Utils
    {
        /// <summary>
        /// Search small image in big image 
        /// </summary>
        /// <param name="smallBmp">Small image </param>
        /// <param name="bigBmp">Big image contain small image</param>
        /// <param name="tolerance">The larger the error, the less accurate it will be (range 0 -> 1)</param>
        /// <returns>The position of the small image is within the large image</returns>
        public static Rectangle Search(Bitmap smallBmp, Bitmap bigBmp, double tolerance = 0.1)
        {
            BitmapData smallData = smallBmp.LockBits(new Rectangle(0, 0, smallBmp.Width, smallBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData bigData = bigBmp.LockBits(new Rectangle(0, 0, bigBmp.Width, bigBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int smallStride = smallData.Stride;
            int bigStride = bigData.Stride;
            int bigWidth = bigBmp.Width;
            int bigHeight = bigBmp.Height - smallBmp.Height + 1;
            int smallWidth = smallBmp.Width * 3;
            int smallHeight = smallBmp.Height;
            Rectangle location = Rectangle.Empty;
            int margin = Convert.ToInt32(255.0 * tolerance);
            unsafe
            {
                byte* pSmall = (byte*)(void*)smallData.Scan0;
                byte* pBig = (byte*)(void*)bigData.Scan0;
                int smallOffset = smallStride - smallBmp.Width * 3;
                int bigOffset = bigStride - bigBmp.Width * 3;
                bool matchFound = true;
                for (int y = 0; y < bigHeight; y++)
                {
                    for (int x = 0; x < bigWidth; x++)
                    {
                        byte* pBigBackup = pBig;
                        byte* pSmallBackup = pSmall;
                        //Look for the small picture.
                        for (int i = 0; i < smallHeight; i++)
                        {
                            int j = 0;
                            matchFound = true;
                            for (j = 0; j < smallWidth; j++)
                            {
                                //With tolerance: pSmall value should be between margins.
                                int inf = pBig[0] - margin;
                                int sup = pBig[0] + margin;
                                if (sup < pSmall[0] || inf > pSmall[0])
                                {
                                    matchFound = false;
                                    break;
                                }
                                pBig++;
                                pSmall++;
                            }
                            if (!matchFound) break;
                            //We restore the pointers.
                            pSmall = pSmallBackup;
                            pBig = pBigBackup;
                            //Next rows of the small and big pictures.
                            pSmall += smallStride * (1 + i);
                            pBig += bigStride * (1 + i);
                        }
                        //If match found, we return.
                        if (matchFound)
                        {
                            location.X = x;
                            location.Y = y;
                            location.Width = smallBmp.Width;
                            location.Height = smallBmp.Height;
                            break;
                        }
                        //If no match found, we restore the pointers and continue.
                        else
                        {
                            pBig = pBigBackup;
                            pSmall = pSmallBackup;
                            pBig += 3;
                        }
                    }
                    if (matchFound) break;
                    pBig += bigOffset;
                }
            }
            bigBmp.UnlockBits(bigData);
            smallBmp.UnlockBits(smallData);
            return location;
        }

        public static Bitmap CropImage(Bitmap b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            using (Graphics g = Graphics.FromImage(nb))
            {
                g.DrawImage(b, -r.X, -r.Y);
                return nb;
            }
        }
        // I made the method generic instead
        public static bool IsDefault<T>(T val)
        {
            return EqualityComparer<T>.Default.Equals(val, default(T));
        }

        public static Process StartProcess(string fileName, string args, bool redirectIO = true, bool hidden = true)
        {
            Process p = new Process();
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = args;
            p.StartInfo.RedirectStandardInput = redirectIO;
            p.StartInfo.RedirectStandardOutput = redirectIO;
            p.StartInfo.UseShellExecute = !redirectIO;
            p.StartInfo.CreateNoWindow = hidden;
            if (hidden)
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            p.Start();

            return p;
        }

        public static string ExecuteCMD(string cmd)
        {
            var p = StartProcess("cmd.exe", $"/c {cmd}");

            return p.StandardOutput.ReadToEnd(); 
        }
    }
}
