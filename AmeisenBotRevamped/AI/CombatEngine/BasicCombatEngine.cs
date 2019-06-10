using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.ActionExecutors.Enums;
using AmeisenBotRevamped.AI.CombatEngine.MovementProvider;
using AmeisenBotRevamped.AI.CombatEngine.Objects;
using AmeisenBotRevamped.AI.CombatEngine.SpellStrategies;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.Logging.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace AmeisenBotRevamped.AI.CombatEngine
{
    public class BasicCombatEngine : ICombatEngine
    {
        public WowUnit ActiveTarget { get; private set; }

        private IWowDataAdapter WowDataAdapter { get; }
        private IWowActionExecutor WowActionExecutor { get; }

        private IMovementProvider MovementProvider { get; }
        private ISpellStrategy SpellStrategy { get; }

        private List<Spell> AvaiableSpells { get; set; }

        public BasicCombatEngine(IWowDataAdapter wowDataAdapter, IWowActionExecutor wowActionExecutor, IMovementProvider movementProvider, ISpellStrategy spellStrategy)
        {
            WowDataAdapter = wowDataAdapter;
            WowActionExecutor = wowActionExecutor;
            MovementProvider = movementProvider;
            SpellStrategy = spellStrategy;
        }

        public List<WowUnit> GetAvaiableTargets()
        {
            return WowDataAdapter.ObjectManager.GetWowUnits().Where(unit => unit.IsInCombat).ToList();
        }

        public void Execute()
        {
            if (!IsUnitValid(ActiveTarget))
            {
                ActiveTarget = SelectNewTarget();
                AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X" , CultureInfo.InvariantCulture.NumberFormat)}]\tNew ActiveTarget is: {ActiveTarget?.Name}");
                return;
            }

            WowPosition positionToMoveTo = MovementProvider?.GetPositionToMoveTo(WowDataAdapter.ActivePlayerPosition, WowDataAdapter.GetPosition(ActiveTarget.BaseAddress)) ?? new WowPosition();
            WowActionExecutor.MoveToPosition(positionToMoveTo);

            WowUnit player = (WowUnit)WowDataAdapter.ObjectManager.GetWowObjectByGuid(WowDataAdapter.PlayerGuid);

            if (ActiveTarget?.Guid != 0 
                && player.TargetGuid != ActiveTarget.Guid)
            {
                WowActionExecutor.TargetGuid(ActiveTarget.Guid);
            }
            
            WowActionExecutor.AttackUnit(ActiveTarget);
            /*SpellStrategy?.GetSpellToCast(player, ActiveTarget);*/
        }

        public void Start()
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X" , CultureInfo.InvariantCulture.NumberFormat)}]\tStarting Combat Engine");
            ActiveTarget = null;
            AvaiableSpells = ReadAvaiableSpells();
        }

        public void Exit()
        {
            // TODO
        }

        private WowUnit SelectNewTarget()
        {
            List<WowUnit> aliveTargets = GetAvaiableTargets().Where(unit => unit.Health > 0 && unit.MaxHealth > 0).ToList();
            if (aliveTargets.Count == 0)
            {
                return null;
            }

            WowUnit player = (WowUnit)WowDataAdapter.ObjectManager.GetWowObjectByGuid(WowDataAdapter.PlayerGuid);
            // get the one with the lowest HealthPercentage
            foreach (WowUnit unit in aliveTargets.OrderByDescending(unit => (unit.Health / unit.MaxHealth) * 100))
            {
                if (WowActionExecutor?.GetUnitReaction(unit, player) != UnitReaction.Friendly)
                {
                    return unit;
                }
            }

            return null;
        }

        private bool IsUnitValid(WowUnit activeTarget)
        {
            if (activeTarget == null)
            {
                return false;
            }

            return activeTarget.Health > 0
                || !activeTarget.IsDead;
        }

        private List<Spell> ReadAvaiableSpells()
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tReading Spellbok...", LogLevel.Verbose);
            WowActionExecutor?.LuaDoString("abotSpellResult='['tabCount=GetNumSpellTabs()for a=1,tabCount do tabName,tabTexture,tabOffset,numEntries=GetSpellTabInfo(a)for b=tabOffset+1,tabOffset+numEntries do abSpellName,abSpellRank=GetSpellName(b,\"BOOKTYPE_SPELL\")if abSpellName then abName,abRank,_,abCosts,_,_,abCastTime,abMinRange,abMaxRange=GetSpellInfo(abSpellName,abSpellRank)abotSpellResult=abotSpellResult..'{'..'\"spellbookName\": \"'..tostring(tabName or 0)..'\",'..'\"spellbookId\": \"'..tostring(a or 0)..'\",'..'\"name\": \"'..tostring(abSpellName or 0)..'\",'..'\"rank\": \"'..tostring(abRank or 0)..'\",'..'\"castTime\": \"'..tostring(abCastTime or 0)..'\",'..'\"minRange\": \"'..tostring(abMinRange or 0)..'\",'..'\"maxRange\": \"'..tostring(abMaxRange or 0)..'\",'..'\"costs\": \"'..tostring(abCosts or 0)..'\"'..'}'if a<tabCount or b<tabOffset+numEntries then abotSpellResult=abotSpellResult..','end end end end;abotSpellResult=abotSpellResult..']'");
            string result = WowActionExecutor?.GetLocalizedText("abotSpellResult");

            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X" , CultureInfo.InvariantCulture.NumberFormat)}]\tAvailable spells: {result}", LogLevel.Verbose);

            List<Spell> spells;
            try
            {
                spells = JsonConvert.DeserializeObject<List<Spell>>(result);
            }
            catch { spells = new List<Spell>(); }

            return spells;
        }

        private bool CastSpell(Spell spell, bool onSelf = false)
        {
            if (spell == null)
            {
                return false;
            }

            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X" , CultureInfo.InvariantCulture.NumberFormat)}]\tCasting spell \"{spell}\" [onSelf = {onSelf}]", LogLevel.Verbose);
            WowUnit player = ((WowUnit)WowDataAdapter.ObjectManager.GetWowObjectByGuid(WowDataAdapter.PlayerGuid));
            WowActionExecutor?.CastSpell(spell.name, onSelf);

            int casttime = 0;
            while (IsMeCasting())
            {
                Thread.Sleep(100);
                casttime += 100;
            }

            if (player.IsConfused
                || player.IsDazed
                || player.IsDisarmed
                || player.IsFleeing
                || player.IsSilenced
                || (casttime < spell.castTime - 100))
            {
                AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X" , CultureInfo.InvariantCulture.NumberFormat)}]\tCast interrupted [casttime = {casttime}, spell.castTime = {spell.castTime}, IsConfused = {player.IsConfused}, IsDazed = {player.IsDazed}, IsDisarmed = {player.IsDisarmed}, IsFleeing = {player.IsFleeing}, IsSilenced = {player.IsSilenced}]", LogLevel.Verbose);
                //we got interrupted, TODO: need to handle casttime buffs
                return false;
            }

            return true;
        }

        private bool IsMeCasting()
        {
            WowUnit player = (WowUnit)WowDataAdapter.ObjectManager.GetWowObjectByGuid(WowDataAdapter.PlayerGuid);
            if (player == null)
            {
                return false;
            }
            return player.CurrentlyCastingSpellId != 0 || player.CurrentlyChannelingSpellId != 0;
        }

        private bool CastSpellByName(string spellName)
        {
            var spellMatches = AvaiableSpells.Where(spell => spell.name == spellName).OrderByDescending(spell => spell.rank);
            if (!spellMatches.Any())
            {
                return false;
            }

            return CastSpell(spellMatches.First());
        }

        private bool IsSpellKnown(string spellName) => AvaiableSpells.Any(spell => spell.name == spellName);
    }
}
