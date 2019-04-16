using AmeisenBotRevamped.CharacterManager.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.CharacterManager
{
    public static class ItemFactory
    {
        public static IItem BuildSpecificItem(RawItem rawItem)
        {
            switch (rawItem.type.ToUpper())
            {
                case "ARMOR": return new ArmorItem(rawItem);
                case "CONSUMEABLE": return new ConsumableItem(rawItem);
                case "CONTAINER": return new ContainerItem(rawItem);
                case "GEM": return new GemItem(rawItem);
                case "KEY": return new KeyItem(rawItem);
                case "MISCELLANEOUS": return new MiscellaneousItem(rawItem);
                case "MONEY": return new MoneyItem(rawItem);
                case "PROJECTILE": return new ProjectileItem(rawItem);
                case "QUEST": return new QuestItem(rawItem);
                case "QUIVER": return new QuiverItem(rawItem);
                case "REAGENT": return new ReagentItem(rawItem);
                case "RECIPE": return new RecipeItem(rawItem);
                case "TRADE GOODS": return new TradeGoodItem(rawItem);
                case "WEAPON": return new WeaponItem(rawItem);
                default: return new BasicItem(rawItem);
            }
        }
    }
}
