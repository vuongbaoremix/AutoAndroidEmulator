using AutoAndroidEmulator;
using AutoAndroidEmulatorTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoAndroidEmulator.Tests
{
    [TestClass()]
    public class LDPlayerTests
    {
        static LDPLayer player;
        static LDPlayerTests()
        {
            player = new LDPLayer(Const.PlayerTest1, new PlayerConfig()
            {
                ADBPath = Const.ADBPath,
                CLIPath = Const.LDPlayerCLIPath
            });

            player.ModifyPlayerProperty("--resolution 540,960,240");
            player.ModifyPlayerProperty("--memory 1024");
        }

        [TestMethod()]
        public void LaunchPlayerTest()
        {
            player.QuitAllPlayer();
            Thread.Sleep(2000);

            player.LaunchPlayer();
            Thread.Sleep(2000);

            player.Connect();

            Assert.IsTrue(player.IsRunning());
        }


        [TestMethod()]
        public void FindImageTest()
        {
            Assert.IsTrue(player.WaitImg(Const.IMG_LOADED, timeout: TimeSpan.FromSeconds(20), tolerance: 0.2)); 
        }


        [TestMethod()]
        public void QuitPlayerTest()
        {
            Assert.IsTrue(player.IsRunning());

            player.QuitPlayer();

            Thread.Sleep(2000);

            Assert.IsFalse(player.IsRunning());
        }
    }
}