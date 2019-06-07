using AmeisenBotRevamped.ActionExecutors.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using System;
using System.Collections.Generic;

namespace AmeisenBotRevamped.ActionExecutors
{
    public class TestWowActionExecutor : IWowActionExecutor
    {
        public int ProcessId => 0x1337;

        public bool IsWorldLoaded { get; set; }

        public void AcceptPartyInvite()
        {
            // Empty for tests
        }

        public void AcceptResurrect()
        {
            // Empty for tests
        }

        public void AcceptSummon()
        {
            // Empty for tests
        }

        public void AntiAfk()
        {
            // Empty for tests
        }

        public void AttackUnit(WowUnit unit)
        {
            // Empty for tests
        }

        public void CastSpell(string name, bool castOnSelf = false)
        {
            // Empty for tests
        }

        public void CofirmBop()
        {
            // Empty for tests
        }

        public void CofirmReadyCheck(bool isReady)
        {
            // Empty for tests
        }

        public void FaceUnit(WowPlayer player, WowPosition positionToFace)
        {
            // Empty for tests
        }

        public List<string> GetAuras(string luaunitName)
        {
            return new List<string> { "buff1", "buff2", "buff3", "debuff1", "debuff2", "debuff3" };
        }

        public List<string> GetBuffs(string luaunitName)
        {
            return new List<string> { "buff1", "buff2", "buff3" };
        }

        public int GetCorpseCooldown()
        {
            return 1000;
        }

        public List<string> GetDebuffs(string luaunitName)
        {
            return new List<string> { "debuff1", "debuff2", "debuff3" };
        }

        public string GetLocalizedText(string variable)
        {
            return "";
        }

        public double GetSpellCooldown(string spellName)
        {
            return 0;
        }

        public UnitReaction GetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB)
        {
            return UnitReaction.Hostile;
        }

        public void InteractWithGuid(ulong guid, ClickToMoveType clickToMoveType = ClickToMoveType.Interact)
        {
            // Empty for tests
        }

        public void Jump()
        {
            // Empty for tests
        }

        public void KickNpcsOutOfMammoth()
        {
            // Empty for tests
        }

        public void LootEveryThing()
        {
            // Empty for tests
        }

        public void LuaDoString(string command)
        {
            // Empty for tests
        }

        public void MoveToPosition(WowPosition targetPosition, ClickToMoveType clickToMoveType = ClickToMoveType.Move, float distance = 1.5F)
        {
            // Empty for tests
        }

        public void ReleaseSpirit()
        {
            // Empty for tests
        }

        public void RepairAllItems()
        {
            // Empty for tests
        }

        public void RetrieveCorpse()
        {
            // Empty for tests
        }

        public void RightClickUnit(WowUnit wowUnit)
        {
            // Empty for tests
        }

        public void SendChatMessage(string message)
        {
            // Empty for tests
        }

        public void SendKey(IntPtr vKey, int minDelay = 20, int maxDelay = 40)
        {
            // Empty for tests
        }

        public void Stop()
        {
            // Empty for tests
        }

        public void TargetGuid(ulong guid)
        {
            // Empty for tests
        }
    }
}
