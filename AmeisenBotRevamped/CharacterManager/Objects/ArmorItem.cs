using AmeisenBotRevamped.CharacterManager.Enums;
using System;

namespace AmeisenBotRevamped.CharacterManager.Objects
{
    public class ArmorItem : BasicItem
    {
        public ArmorType ArmorType => Enum.TryParse(Subtype.ToUpper(), out ArmorType armorType) ? armorType : ArmorType.MISCELLANEOUS;

        public ArmorItem(RawItem rawItem) : base(rawItem)
        {

        }
    }
}
