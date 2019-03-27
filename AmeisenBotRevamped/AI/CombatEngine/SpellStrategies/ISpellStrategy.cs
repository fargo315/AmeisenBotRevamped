using AmeisenBotRevamped.AI.CombatEngine.Objects;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using System.Collections.Generic;

namespace AmeisenBotRevamped.AI.CombatEngine.SpellStrategies
{
    public interface ISpellStrategy
    {
        Spell GetSpellToCast(WowUnit player, WowUnit activeTarget);
        void RefreshSpellbook(List<Spell> spells);
    }
}
