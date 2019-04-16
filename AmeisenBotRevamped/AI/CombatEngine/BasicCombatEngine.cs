﻿using AmeisenBotRevamped.ActionExecutors;
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
using System.Linq;
using System.Threading;

namespace AmeisenBotRevamped.AI.CombatEngine
{
    public class BasicCombatEngine : ICombatEngine
    {
        public WowUnit ActiveTarget { get; private set; }
        public List<WowUnit> AvaiableTargets => WowDataAdapter.ObjectManager.WowUnits.Where(unit => unit.IsInCombat).ToList();

        private IWowDataAdapter WowDataAdapter { get; set; }
        private IWowActionExecutor WowActionExecutor { get; set; }

        private IMovementProvider MovementProvider { get; set; }
        private ISpellStrategy SpellStrategy { get; set; }

        private List<Spell> AvaiableSpells { get; set; }

        public BasicCombatEngine(IWowDataAdapter wowDataAdapter, IWowActionExecutor wowActionExecutor, IMovementProvider movementProvider, ISpellStrategy spellStrategy)
        {
            WowDataAdapter = wowDataAdapter;
            WowActionExecutor = wowActionExecutor;
            MovementProvider = movementProvider;
            SpellStrategy = spellStrategy;
        }

        public void Execute()
        {
            if (!IsUnitValid(ActiveTarget))
            {
                ActiveTarget = SelectNewTarget();
                AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X")}]\tNew ActiveTarget is: {ActiveTarget?.Name}");
                return;
            }

            WowPosition positionToMoveTo = MovementProvider?.GetPositionToMoveTo(WowDataAdapter.ActivePlayerPosition, WowDataAdapter.GetPosition(ActiveTarget.BaseAddress)) ?? new WowPosition();
            WowActionExecutor.MoveToPosition(positionToMoveTo);


            WowUnit player = (WowUnit)WowDataAdapter.ObjectManager.GetWowObjectByGuid(WowDataAdapter.PlayerGuid);

            if (player.TargetGuid != ActiveTarget.Guid)
            {
                WowActionExecutor.TargetGuid(ActiveTarget.Guid);
            }

            //WowActionExecutor.SendChatMessage("/startattack");
            WowActionExecutor.AttackUnit(ActiveTarget);

            Spell spellToCast = SpellStrategy?.GetSpellToCast(player, ActiveTarget);

            /*if (CastSpell(spellToCast))
            {
                AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X")}]\tCast successful [{spellToCast?.name}]");
            }
            else
            {
                AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X")}]\tCast not successful [{spellToCast?.name}]");
            }*/
        }

        public void Start()
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X")}]\tStarting Combat Engine");
            ActiveTarget = null;
            AvaiableSpells = ReadAvaiableSpells();
        }

        public void Exit()
        {

        }

        private WowUnit SelectNewTarget()
        {
            List<WowUnit> aliveTargets = AvaiableTargets.Where(unit => unit.Health > 0 && unit.MaxHealth > 0).ToList();
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

            if (activeTarget.Health > 0
                || !activeTarget.IsDead)
            {
                return true;
            }

            return false;
        }

        private List<Spell> ReadAvaiableSpells()
        {
            WowActionExecutor?.LuaDoString("abotSpellResult='['tabCount=GetNumSpellTabs()for a=1,tabCount do tabName,tabTexture,tabOffset,numEntries=GetSpellTabInfo(a)for b=tabOffset+1,tabOffset+numEntries do abSpellName,abSpellRank=GetSpellName(b,\"BOOKTYPE_SPELL\")if abSpellName then abName,abRank,_,abCosts,_,_,abCastTime,abMinRange,abMaxRange=GetSpellInfo(abSpellName,abSpellRank)abotSpellResult=abotSpellResult..'{'..'\"spellbookName\": \"'..tostring(tabName or 0)..'\",'..'\"spellbookId\": \"'..tostring(a or 0)..'\",'..'\"name\": \"'..tostring(abSpellName or 0)..'\",'..'\"rank\": \"'..tostring(abRank or 0)..'\",'..'\"castTime\": \"'..tostring(abCastTime or 0)..'\",'..'\"minRange\": \"'..tostring(abMinRange or 0)..'\",'..'\"maxRange\": \"'..tostring(abMaxRange or 0)..'\",'..'\"costs\": \"'..tostring(abCosts or 0)..'\"'..'}'if a<tabCount or b<tabOffset+numEntries then abotSpellResult=abotSpellResult..','end end end end;abotSpellResult=abotSpellResult..']'");
            string result = WowActionExecutor?.GetLocalizedText("abotSpellResult");

            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X")}]\tAvailable spells: {result}", LogLevel.Verbose);

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

            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X")}]\tCasting spell \"{spell}\" [onSelf = {onSelf}]", LogLevel.Verbose);
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
                AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X")}]\tCast interrupted [casttime = {casttime}, spell.castTime = {spell.castTime}, IsConfused = {player.IsConfused}, IsDazed = {player.IsDazed}, IsDisarmed = {player.IsDisarmed}, IsFleeing = {player.IsFleeing}, IsSilenced = {player.IsSilenced}]", LogLevel.Verbose);
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
            if (spellMatches.Count() == 0)
            {
                return false;
            }

            return CastSpell(spellMatches.First());
        }

        private bool IsSpellKnown(string spellName) => AvaiableSpells.Where(spell => spell.name == spellName).Count() > 0;
    }
}
