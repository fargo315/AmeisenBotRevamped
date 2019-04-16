using AmeisenBotRevamped.ActionExecutors.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using System;
using System.Collections.Generic;

namespace AmeisenBotRevamped.ActionExecutors
{
    public interface IWowActionExecutor
    {
        int ProcessId { get; }
        bool IsWorldLoaded { get; set; }

        void AntiAfk();
        void KickNpcsOutOfMammoth();
        void LootEveryThing();
        void ReleaseSpirit();
        void RepairAllItems();
        void RetrieveCorpse();
        int GetCorpseCooldown();
        void Jump();
        void FaceUnit(WowPlayer player, WowPosition positionToFace);
        void SendKey(IntPtr vKey, int minDelay = 20, int maxDelay = 40);
        void TargetGuid(ulong guid);
        void AttackUnit(WowUnit unit);
        void CastSpell(string name, bool castOnSelf = false);

        void LuaDoString(string command);
        string GetLocalizedText(string variable);
        void SendChatMessage(string message);

        void Stop();

        void InteractWithGuid(ulong guid, ClickToMoveType clickToMoveType = ClickToMoveType.Interact);
        void MoveToPosition(WowPosition targetPosition, ClickToMoveType clickToMoveType = ClickToMoveType.Move, float distance = 1.5f);

        void AcceptPartyInvite();
        void AcceptResurrect();
        void AcceptSummon();

        UnitReaction GetUnitReaction(WowUnit wowUnitA, WowUnit wowUnitB);

        void RightClickUnit(WowUnit wowUnit);

        List<string> GetAuras(string luaunitName);
        List<string> GetBuffs(string luaunitName);
        List<string> GetDebuffs(string luaunitName);

        double GetSpellCooldown(string spellName);

        void CofirmBop();
        void CofirmReadyCheck(bool isReady);
    }
}
