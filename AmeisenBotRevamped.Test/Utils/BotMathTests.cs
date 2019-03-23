using AmeisenBotRevamped.Utils;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AmeisenBotRevamped.Utils.Tests
{
    [TestClass()]
    public class BotMathTests
    {
        [TestMethod()]
        public void GetDistanceTest()
        {
            WowPosition a = new WowPosition() { x = 0, y = 0, z = 0 };
            WowPosition b = new WowPosition() { x = 1, y = 0, z = 0 };

            Assert.AreEqual(1, BotMath.GetDistance(a, b));
        }

        [TestMethod()]
        public void GetFacingAngleTest()
        {
            WowPosition a = new WowPosition() { x = 0, y = 0, z = 0, r = 2 };
            WowPosition b = new WowPosition() { x = 10, y = 10, z = 10, r = 1 };

            double angle = BotMath.GetFacingAngle(a, b);

            Assert.AreEqual(0.79, Math.Round(angle, 2));
        }

        [TestMethod()]
        public void IsFacingTest()
        {
            WowPosition a = new WowPosition() { x = 0, y = 0, z = 0, r = 1 };
            WowPosition b = new WowPosition() { x = 10, y = 10, z = 10, r = 1 };

            Assert.IsTrue(BotMath.IsFacing(a, b));
        }
    }
}