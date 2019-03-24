using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmeisenBotRevamped.Test
{
    [TestClass]
    public class AmeisenBotTests
    {
        private AmeisenBot ameisenBot;

        [TestInitialize]
        public void Setup()
        {
            TestWowDataAdapter dataAdapter = new TestWowDataAdapter();
            ameisenBot = new AmeisenBot(null, dataAdapter, null, null);
        }

        [TestMethod]
        public void AmeisenBotSetup()
        {
            TestWowDataAdapter adapter = new TestWowDataAdapter();
            AmeisenBot bot = new AmeisenBot(null, adapter, null, null);
        }

        [TestMethod]
        public void AmeisenBotBasicInformation()
        {
            Equals("TCharacter", ameisenBot.CharacterName);
            Equals("TRealm", ameisenBot.RealmName);

            Assert.IsTrue(ameisenBot.WowDataAdapter.IsWorldLoaded);
            Equals(WowGameState.World, ameisenBot.WowDataAdapter.GameState);

            Equals(0x1, ameisenBot.WowDataAdapter.PlayerGuid);
            Equals(0x2, ameisenBot.WowDataAdapter.TargetGuid);
            Equals(0x10, ameisenBot.WowDataAdapter.PetGuid);

            ((TestWowDataAdapter)ameisenBot.WowDataAdapter).SetIsWorldLoaded(false);
            ((TestWowDataAdapter)ameisenBot.WowDataAdapter).SetIsWorldLoaded(true);
        }

        [TestMethod]
        public void AmeisenBotObjectManagerQueryGuid()
        {
            Equals(ameisenBot.ObjectManager.WowObjects[0], ameisenBot.ObjectManager.GetWowObjectByGuid(0x1));
            Equals(ameisenBot.ObjectManager.WowObjects[1], ameisenBot.ObjectManager.GetWowObjectByGuid(0x2));
            Equals(ameisenBot.ObjectManager.WowObjects[2], ameisenBot.ObjectManager.GetWowObjectByGuid(0x10));
            Equals(ameisenBot.ObjectManager.WowObjects[3], ameisenBot.ObjectManager.GetWowObjectByGuid(0x50));
        }

        [TestMethod]
        public void AmeisenBotObjectManagerQueryFalseInput()
        {
            Equals(null, ameisenBot.ObjectManager.GetWowObjectByGuid(0x1000));
            Equals(null, ameisenBot.ObjectManager.GetWowObjectByGuid(0));
        }

        [TestMethod]
        public void AmeisenBotObjectManagerQueryObjectTypes()
        {
            Assert.IsTrue(ameisenBot.ObjectManager.GetWowObjectByGuid(0x1) is WowPlayer);
            Assert.IsTrue(ameisenBot.ObjectManager.GetWowObjectByGuid(0x2) is WowPlayer);
            Assert.IsTrue(ameisenBot.ObjectManager.GetWowObjectByGuid(0x10) is WowUnit);
            Assert.IsTrue(ameisenBot.ObjectManager.GetWowObjectByGuid(0x50) is WowObject);
        }
    }
}
