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

        }

        public void AcceptResurrect()
        {

        }

        public void AcceptSummon()
        {

        }

        public void AntiAfk()
        {

        }

        public void AttackTarget()
        {

        }

        public void CastSpell(string name, bool castOnSelf = false)
        {

        }

        public void FaceUnit(WowPlayer player, WowPosition positionToFace)
        {

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

        }

        public void Jump()
        {

        }

        public void KickNpcsOutOfMammoth()
        {

        }

        public void LootEveryThing()
        {

        }

        public void LuaDoString(string command)
        {

        }

        public void MoveToPosition(WowPosition targetPosition, ClickToMoveType clickToMoveType = ClickToMoveType.Move, float distance = 1.5F)
        {

        }

        public void ReleaseSpirit()
        {

        }

        public void RepairAllItems()
        {

        }

        public void RetrieveCorpse()
        {

        }

        public void RightClickUnit(WowUnit wowUnit)
        {

        }

        public void SendChatMessage(string message)
        {

        }

        public void SendKey(IntPtr vKey, int minDelay = 20, int maxDelay = 40)
        {

        }

        public void Stop()
        {

        }

        public void TargetGuid(ulong guid)
        {

        }
    }
}
