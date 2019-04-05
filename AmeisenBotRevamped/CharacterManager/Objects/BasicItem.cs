using AmeisenBotRevamped.CharacterManager.Enums;
using System;

namespace AmeisenBotRevamped.CharacterManager.Objects
{
    public class BasicItem : IItem
    {
        public int Id { get; private set; }
        public string Type { get; private set; }
        public string Subtype { get; private set; }
        public string Name { get; private set; }
        public string ItemLink { get; private set; }
        public EquipmentSlot EquipLocation { get; private set; }
        public ItemQuality ItemQuality { get; private set; }
        public int ItemLevel { get; private set; }
        public int RequiredLevel { get; private set; }
        public int Price { get; private set; }
        public int Count { get; private set; }
        public int MaxStack { get; private set; }
        public int Durability { get; private set; }
        public int MaxDurability { get; private set; }

        public BasicItem(RawItem rawItem)
        {
            Id = int.TryParse(rawItem.id, out int pId) ? pId : -1;
            Type = rawItem.type;
            Subtype = rawItem.subtype;
            Name = rawItem.name;
            ItemLink = rawItem.link;
            EquipLocation = Enum.TryParse(rawItem.equiplocation, out EquipmentSlot equipmentSlot) ? equipmentSlot : EquipmentSlot.NOT_EQUIPABLE;
            ItemQuality = int.TryParse(rawItem.quality, out int pQuality) ? (ItemQuality)pQuality : ItemQuality.POOR;
            ItemLevel = int.TryParse(rawItem.level, out int pItemLevel) ? pItemLevel : 0;
            RequiredLevel = int.TryParse(rawItem.minLevel, out int pRequiredLevel) ? pRequiredLevel : 0;
            Price = int.TryParse(rawItem.sellprice, out int pItemPrice) ? pItemPrice : 0;
            Count = int.TryParse(rawItem.count, out int pCount) ? pCount : 0;
            MaxStack = int.TryParse(rawItem.maxStack, out int pMaxStack) ? pMaxStack : 0;
            Durability = int.TryParse(rawItem.curDurability, out int pDurability) ? pDurability : 0;
            MaxDurability = int.TryParse(rawItem.maxDurability, out int pMaxDurability) ? pMaxDurability : 0;
        }        
    }
}
