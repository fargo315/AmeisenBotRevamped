using AmeisenBotRevamped.DataAdapters.DataSets;
using AmeisenBotRevamped.ObjectManager;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using System.Collections.Generic;

namespace AmeisenBotRevamped.DataAdapters
{
    public delegate void OnGamestateChanged(bool IsWorldLoaded, WowGameState gameState);

    public interface IWowDataAdapter
    {
        ulong PlayerGuid { get; }
        ulong TargetGuid { get; }
        ulong PetGuid { get; }
        List<ulong> PartymemberGuids { get; }
        ulong PartyLeaderGuid { get; }

        int MapId { get; }

        BasicInfoDataSet BasicInfoDataSet { get; }
        WowObjectManager ObjectManager { get; }

        bool IsWorldLoaded { get; }
        WowGameState GameState { get; }

        OnGamestateChanged OnGamestateChanged { get; set; }

        bool ObjectUpdatesEnabled { get; }

        void StartObjectUpdates();
        void StopObjectUpdates();

        void ClearCaches();
    }
}
