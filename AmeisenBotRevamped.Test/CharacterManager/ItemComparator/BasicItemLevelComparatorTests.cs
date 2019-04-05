using Microsoft.VisualStudio.TestTools.UnitTesting;
using AmeisenBotRevamped.CharacterManager.ItemComparator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using AmeisenBotRevamped.ObjectManager;
using AmeisenBotRevamped.CharacterManager.Objects;

namespace AmeisenBotRevamped.CharacterManager.ItemComparator.Tests
{
    [TestClass()]
    public class BasicItemLevelComparatorTests
    {
        [TestMethod()]
        public void CompareItemsTest()
        {
            TestWowDataAdapter dataAdapter = new TestWowDataAdapter();
            BasicItemLevelComparator basicItemLevelComparator = new BasicItemLevelComparator(dataAdapter);

            RawItem rawItem1 = new RawItem
            {
                cooldownEnd = "0",
                cooldownStart = "0",
                count = "1",
                curDurability = "100",
                equiplocation = "INVSLOT_HEAD",
                id = "4712",
                level = "85",
                link = "...",
                maxDurability = "100",
                maxStack = "1",
                minLevel = "60",
                name = "Ultra Rubbish Bin",
                quality = "2",
                sellprice = "1250",
                subtype = "Plate",
                type = "armor"
            };

            RawItem rawItem2 = new RawItem
            {
                cooldownEnd = "0",
                cooldownStart = "0",
                count = "1",
                curDurability = "100",
                equiplocation = "INVSLOT_HEAD",
                id = "4713",
                level = "65",
                link = "...",
                maxDurability = "100",
                maxStack = "1",
                minLevel = "40",
                name = "Ultra Rubbish Cowl",
                quality = "2",
                sellprice = "1250",
                subtype = "Cloth",
                type = "armor"
            };

            RawItem rawItem3 = new RawItem
            {
                cooldownEnd = "0",
                cooldownStart = "0",
                count = "1",
                curDurability = "100",
                equiplocation = "INVSLOT_HEAD",
                id = "4714",
                level = "5",
                link = "...",
                maxDurability = "100",
                maxStack = "1",
                minLevel = "40",
                name = "Rubbish Cowl",
                quality = "2",
                sellprice = "1250",
                subtype = "Cloth",
                type = "armor"
            };

            IItem resultItem1 = ItemFactory.BuildSpecificItem(rawItem1);
            IItem resultItem2 = ItemFactory.BuildSpecificItem(rawItem2);
            IItem resultItem3 = ItemFactory.BuildSpecificItem(rawItem3);

            Assert.IsInstanceOfType(resultItem1, typeof(ArmorItem));
            Assert.IsInstanceOfType(resultItem2, typeof(ArmorItem));
            Assert.IsInstanceOfType(resultItem3, typeof(ArmorItem));

            Assert.IsFalse(basicItemLevelComparator.CompareItems(resultItem1, resultItem3));
            Assert.IsTrue(basicItemLevelComparator.CompareItems(resultItem2, resultItem3));
        }
    }
}