using AmeisenBotRevamped.ActionExecutors.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.ActionExecutors
{
    public interface IWowActionExecutor
    {
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
        void AttackTarget(ulong guid);
        void TargetGuid(ulong guid);
        void CastSpell(string name, bool castOnSelf = false);

        void LuaDoString(string command);
        string GetLocalizedText(string variable);

        void Stop();

        void InteractWithGuid(ulong guid, ClickToMoveType clickToMoveType = ClickToMoveType.Interact);
        void MoveToPosition(WowPosition targetPosition, ClickToMoveType clickToMoveType = ClickToMoveType.Move, float distance = 1.5f);
    }
}
