using AmeisenBotRevamped.CharacterManager.Enums;
using System;

namespace AmeisenBotRevamped.CharacterManager.Objects
{
    public class WeaponItem : BasicItem
    {
        public WeaponType ArmorType => Enum.TryParse(Subtype.ToUpper().Replace("-", "").Replace(" ", "_"), out WeaponType armorType) ? armorType : WeaponType.MISCELLANEOUS;

        public WeaponItem(RawItem rawItem) : base(rawItem)
        {

        }
    }
}
