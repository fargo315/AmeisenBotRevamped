using AmeisenBotRevamped.AI.CombatEngine.Objects;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using System.Collections.Generic;

namespace AmeisenBotRevamped.AI.CombatEngine.SpellStrategies
{
    public class TestSpellStrategy : ISpellStrategy
    {
        public Spell GetSpellToCast(WowUnit player, WowUnit activeTarget)
        {
            return new Spell()
            {
                name = "TestSpell",
                castTime = 500,
                costs = 10,
                maxRange = 28,
                minRange = 0,
                rank = "TestRank",
                spellbookId = 1337,
                spellBookName = "TestBook"
            };
        }

        public void RefreshSpellbook(List<Spell> spells)
        {
            // TODO
        }
    }
}
