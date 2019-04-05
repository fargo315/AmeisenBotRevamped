using AmeisenBotRevamped.CharacterManager.Enums;
using AmeisenBotRevamped.CharacterManager.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmeisenBotRevamped.CharacterManager.Tests
{
    [TestClass()]
    public class ItemFactoryTests
    {
        [TestMethod()]
        public void BuildSpecificItemTest()
        {
            RawItem rawItem = new RawItem
            {
                cooldownEnd = "0",
                cooldownStart = "0",
                count = "1",
                curDurability = "100",
                equiplocation = "INVSLOT_HEAD",
                id = "4711",
                level = "25",
                link = "...",
                maxDurability = "100",
                maxStack = "1",
                minLevel = "0",
                name = "Armored Rubbish Bin",
                quality = "2",
                sellprice = "1250",
                subtype = "Mail",
                type = "armor"
            };

            IItem resultItem = ItemFactory.BuildSpecificItem(rawItem);
            Assert.IsInstanceOfType(resultItem, typeof(ArmorItem));
            Assert.AreEqual(EquipmentSlot.INVSLOT_HEAD, resultItem.EquipLocation);
        }

        [TestMethod()]
        public void BuildSpecificItemTriketTest()
        {
            RawItem rawItem = new RawItem
            {
                cooldownEnd = "0",
                cooldownStart = "0",
                count = "1",
                curDurability = "100",
                equiplocation = "INVSLOT_TRINKET",
                id = "4712",
                level = "25",
                link = "...",
                maxDurability = "100",
                maxStack = "1",
                minLevel = "0",
                name = "Trashball",
                quality = "2",
                sellprice = "1250",
                subtype = "Miscellaneous",
                type = "armor"
            };

            IItem resultItem = ItemFactory.BuildSpecificItem(rawItem);
            Assert.IsInstanceOfType(resultItem, typeof(ArmorItem));
            //Assert.AreEqual(EquipmentSlot.INVSLOT_TRINKET1, resultItem.EquipLocation);
            //Assert.AreEqual(EquipmentSlot.INVSLOT_TRINKET2, resultItem.EquipLocation);
        }
    }
}