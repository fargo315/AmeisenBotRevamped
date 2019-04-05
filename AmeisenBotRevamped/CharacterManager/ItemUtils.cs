using AmeisenBotRevamped.CharacterManager.Enums;
using AmeisenBotRevamped.CharacterManager.Objects;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;

namespace AmeisenBotRevamped.CharacterManager
{
    public abstract class ItemUtils
    {
        public static bool IsItemUsefulForMe(WowPlayer wowPlayer, IItem item)
        {
            if (item.RequiredLevel > wowPlayer.Level)
            {
                return false;
            }

            if (item is ArmorItem 
                && IsArmorEquipableForCharacter(wowPlayer, (ArmorItem)item))
            {
                return true;
            }

            if (item is WeaponItem 
                && IsWeaponEquipableForCharacter(wowPlayer, (WeaponItem)item))
            {
                return true;
            }

            return false;
        }

        private static bool IsWeaponEquipableForCharacter(WowPlayer wowPlayer, WeaponItem item)
        {
            return false;
        }

        public static bool IsArmorEquipableForCharacter(WowPlayer wowPlayer, ArmorItem item)
        {
            // TODO: handle class specific items

            if (item.ArmorType == ArmorType.MISCELLANEOUS)
            {
                return true;
            }

            switch (wowPlayer.Class)
            {
                case WowClass.DeathKnight: return item.ArmorType == ArmorType.PLATE;
                case WowClass.Druid: return item.ArmorType == ArmorType.LEATHER;
                case WowClass.Hunter: return wowPlayer.Level < 40 ? item.ArmorType == ArmorType.LEATHER : item.ArmorType == ArmorType.MAIL;
                case WowClass.Mage: return item.ArmorType == ArmorType.CLOTH;
                case WowClass.Paladin: return wowPlayer.Level < 40 ? item.ArmorType == ArmorType.MAIL : item.ArmorType == ArmorType.PLATE;
                case WowClass.Priest: return item.ArmorType == ArmorType.CLOTH;
                case WowClass.Rogue: return item.ArmorType == ArmorType.LEATHER;
                case WowClass.Shaman: return wowPlayer.Level < 40 ? item.ArmorType == ArmorType.LEATHER : item.ArmorType == ArmorType.MAIL;
                case WowClass.Warlock: return item.ArmorType == ArmorType.CLOTH;
                case WowClass.Warrior: return wowPlayer.Level < 40 ? item.ArmorType == ArmorType.MAIL : item.ArmorType == ArmorType.PLATE;
            }

            return false;
        }
    }
}
