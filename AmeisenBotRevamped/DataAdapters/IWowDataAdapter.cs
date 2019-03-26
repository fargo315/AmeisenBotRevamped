using AmeisenBotRevamped.DataAdapters.DataSets;
using AmeisenBotRevamped.ObjectManager;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using System.Collections.Generic;

namespace AmeisenBotRevamped.DataAdapters
{
    public delegate void OnGamestateChanged(bool IsWorldLoaded, WowGameState gameState);

    public interface IWowDataAdapter
    {
        ulong PlayerGuid { get; }
        ulong TargetGuid { get; }
        ulong PetGuid { get; }
        int MapId { get; }
        int ZoneId { get; }
        int ComboPoints { get; }
        int WowBuild { get; }
        string ContinentName { get; }
        string LastErrorMessage { get; }
        string AccountName { get; }

        List<ulong> PartymemberGuids { get; }
        ulong PartyleaderGuid { get; }
        WowPosition ActivePlayerPosition { get; }

        BasicInfoDataSet BasicInfoDataSet { get; }
        WowObjectManager ObjectManager { get; }

        bool IsWorldLoaded { get; }
        WowGameState GameState { get; }

        OnGamestateChanged OnGamestateChanged { get; set; }

        bool ObjectUpdatesEnabled { get; }

        void StartObjectUpdates();
        void StopObjectUpdates();

        void ClearCaches();

        WowPosition GetPosition(uint baseAddress);
    }
}
