using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.AI.CombatEngine.Objects;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.Utils;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotRevamped.AI.CombatEngine.SpellStrategies
{
    public class WarriorFury : ISpellStrategy
    {
        private bool IsBattleShoutKnown { get; set; }
        private bool IsBerserkerRageKnown { get; set; }
        private bool IsBerserkerStanceKnown { get; set; }
        private bool IsBloodthirstKnown { get; set; }
        private bool IsDeathWishKnown { get; set; }
        private bool IsEnragedRegenerationKnown { get; set; }
        private bool IsExecuteKnown { get; set; }
        private bool IsHamstringKnown { get; set; }
        private bool IsHeroicStrikeKnown { get; set; }
        private bool IsHeroicThrowKnown { get; set; }
        private bool IsInMainCombo { get; set; }
        private bool IsInterceptKnown { get; set; }
        private bool IsRecklessnessKnown { get; set; }
        private bool IsSlamKnown { get; set; }
        private bool IsWhirlwindKnown { get; set; }

        private List<Spell> Spells { get; set; }

        private IWowDataAdapter WowDataAdapter { get; }
        private IWowActionExecutor WowActionExecutor { get; }

        public WarriorFury(IWowDataAdapter wowDataAdapter, IWowActionExecutor wowActionExecutor, List<Spell> spells)
        {
            WowDataAdapter = wowDataAdapter;
            WowActionExecutor = wowActionExecutor;

            RefreshSpellbook(spells);

            IsInMainCombo = false;
        }

        public Spell GetSpellToCast(WowUnit player, WowUnit activeTarget)
        {
            List<string> myAuras = WowActionExecutor.GetAuras("player");
            List<string> targetAuras = WowActionExecutor.GetAuras("activeTarget");

            Spell spellToUse = null;
            double targetDistance = BotMath.GetDistance(player.Position, activeTarget.Position);

            // if we are low on HP try to use Enraged Regeneration
            if (player.Health / player.MaxHealth * 100 < 40
                && IsEnragedRegenerationKnown)
            {
                spellToUse = TryUseSpell("Enraged Regeneration", player);
                if (spellToUse != null) { return spellToUse; }
            }

            // hold Berserker Rage on cooldown
            if (IsBerserkerRageKnown && !IsInMainCombo)
            {
                spellToUse = TryUseSpell("Berserker Rage", player);
                if (spellToUse != null) { return spellToUse; }
            }

            // main spell rotation
            if (targetDistance < 3)
            {
                // if we got enough rage and nothing better to do, use Heroic Strike
                if (player.Energy > 50 && activeTarget.Health / activeTarget.MaxHealth * 100 > 15
                    && IsHeroicStrikeKnown)
                {
                    // Heroic Strike wont't interrupt main-combo
                    spellToUse = TryUseSpell("Heroic Strike", player);
                    if (spellToUse != null) { return spellToUse; }
                }

                // if we are in our main-combo, use the second part of it, whirlwind
                if (IsWhirlwindKnown && IsInMainCombo && IsBerserkerStanceKnown)
                {
                    // dont't interrupt main-combo
                    spellToUse = TryUseSpell("Whirlwind", player);
                    if (spellToUse != null) { IsInMainCombo = false; }
                    // normally whirlwind has 10s cooldown, bloodthirst only 5 so use it if we are still in that 10s
                    else if (IsBloodthirstKnown) { spellToUse = TryUseSpell("Bloodthirst", player); }

                    return spellToUse;
                }
                else if (!IsBerserkerStanceKnown)
                {
                    IsInMainCombo = false;
                }

                // use hamstring so our enemy can't escape
                if (IsHamstringKnown && !targetAuras.Contains("hamstring"))
                {
                    spellToUse = TryUseSpell("Hamstring", player);
                    if (spellToUse != null) { return spellToUse; }
                }

                // when da slam procs, use it
                if (IsSlamKnown && myAuras.Contains("slam!"))
                {
                    spellToUse = TryUseSpell("Slam", player);
                    if (spellToUse != null) { return spellToUse; }
                }

                // start our main-combo
                if (IsBloodthirstKnown)
                {
                    spellToUse = TryUseSpell("Bloodthirst", player);

                    if (spellToUse != null)
                    {
                        IsInMainCombo = true;
                        return spellToUse;
                    }
                }

                // hold Recklessness on cooldown
                if (IsRecklessnessKnown && !IsInMainCombo)
                {
                    spellToUse = TryUseSpell("Recklessness", player);
                    if (spellToUse != null) { return spellToUse; }
                }

                // hold Death Wish on cooldown
                if (IsDeathWishKnown && !IsInMainCombo)
                {
                    spellToUse = TryUseSpell("Death Wish", player);
                    if (spellToUse != null) { return spellToUse; }
                }

                // hold Battleshout on cooldown
                if (IsBattleShoutKnown && !IsInMainCombo && !myAuras.Contains("battle shout"))
                {
                    spellToUse = TryUseSpell("Battle Shout", player);
                    if (spellToUse != null) { return spellToUse; }
                }

                if (!IsInMainCombo
                    && player.Energy > 50
                    && IsExecuteKnown
                    && activeTarget.Health / activeTarget.MaxHealth * 100 < 15)
                {
                    spellToUse = TryUseSpell("Execute", player);
                    if (spellToUse != null) { return spellToUse; }
                }
            }
            else if (targetDistance > 8 && targetDistance < 25
                && IsInterceptKnown)
            {
                // try to charge to our activeTarget
                spellToUse = TryUseSpell("Intercept", player);
                if (spellToUse != null) { return spellToUse; }
            }
            else if (targetDistance < 30
                && IsHeroicThrowKnown)
            {
                // if there is really nothing other to do, throw something
                spellToUse = TryUseSpell("Heroic Throw", player);
                if (spellToUse != null) { return spellToUse; }
            }
            return null;
        }

        private Spell TryUseSpell(string spellname, WowUnit player)
        {
            Spell spellToUse = Spells.Find(spell => spell.name == spellname);

            if (spellToUse == null) { return null; }
            if (player.Energy < spellToUse.costs) { return null; }

            if (WowActionExecutor.GetSpellCooldown(spellToUse.name) < 0)
            {
                IsInMainCombo = false;
                return spellToUse;
            }
            return null;
        }

        public void RefreshSpellbook(List<Spell> spells)
        {
            Spells = spells;

            IsSlamKnown = Spells.Any(spell => spell.name == "Slam");
            IsBloodthirstKnown = Spells.Any(spell => spell.name == "Bloodthirst");
            IsWhirlwindKnown = Spells.Any(spell => spell.name == "Whirlwind");
            IsBerserkerRageKnown = Spells.Any(spell => spell.name == "Berserker Rage");
            IsHeroicStrikeKnown = Spells.Any(spell => spell.name == "Heroic Strike");
            IsHeroicThrowKnown = Spells.Any(spell => spell.name == "Heroic Throw");
            IsExecuteKnown = Spells.Any(spell => spell.name == "Execute");
            IsRecklessnessKnown = Spells.Any(spell => spell.name == "Recklessness");
            IsDeathWishKnown = Spells.Any(spell => spell.name == "Death Wish");
            IsEnragedRegenerationKnown = Spells.Any(spell => spell.name == "Enraged Regeneration");
            IsInterceptKnown = Spells.Any(spell => spell.name == "Intercept");
            IsHamstringKnown = Spells.Any(spell => spell.name == "Hamstring");
            IsBattleShoutKnown = Spells.Any(spell => spell.name == "Battle Shout");
            IsBerserkerStanceKnown = Spells.Any(spell => spell.name == "Berserker Stance");
        }
    }
}
