namespace AmeisenBotRevamped.OffsetLists
{
    public interface IOffsetList
    {
        uint StaticPlayerName { get; }
        uint StaticRealmName { get; }
        uint StaticAccountName { get; }
        uint StaticWowBuild { get; }
        uint StaticContinentName { get; }
        uint StaticMapId { get; }
        uint StaticZoneId { get; }
        uint StaticIsWorldLoaded { get; }
        uint StaticGameState { get; }
        uint StaticErrorMessage { get; }
        uint StaticClientConnection { get; }
        uint StaticComboPoints { get; }
        uint StaticTickCount { get; }

        uint StaticRace { get; }
        uint StaticClass { get; }

        uint StaticPlayerBase { get; }
        uint StaticPlayerGuid { get; }
        uint StaticTargetGuid { get; }
        uint StaticPetGuid { get; }

        uint OffsetCurrentObjectManager { get; }
        uint OffsetFirstObject { get; }
        uint OffsetNextObject { get; }

        uint OffsetWowObjectType { get; }
        uint OffsetWowObjectGuid { get; }
        uint OffsetWowObjectDescriptor { get; }
        uint OffsetWowUnitPosition { get; }
        uint OffsetGameObjectPosition { get; }

        uint DescriptorOffsetTargetGuid { get; }
        uint DescriptorOffsetLevel { get; }
        uint DescriptorOffsetRace { get; }
        uint DescriptorOffsetClass { get; }

        uint DescriptorOffsetHealth { get; }
        uint DescriptorOffsetMaxHealth { get; }

        uint DescriptorOffsetManaGuid { get; }
        uint DescriptorOffsetMaxManaGuid { get; }

        uint DescriptorOffsetRageGuid { get; }
        uint DescriptorOffsetMaxRageGuid { get; }

        uint DescriptorOffsetEnergy { get; }
        uint DescriptorOffsetMaxEnergy { get; }

        uint DescriptorOffsetRuneenergyGuid { get; }
        uint DescriptorOffsetMaxRuneenergyGuid { get; }

        uint DescriptorOffsetExp{ get; }
        uint DescriptorOffsetMaxExp { get; }

        uint DescriptorOffsetUnitFlags { get; }
        uint DescriptorOffsetUnitFlagsDynamic { get; }

        uint StaticNameStore { get; }
        uint OffsetNameBase { get; }
        uint OffsetNameMask { get; }
        uint OffsetNameString { get; }

        byte[] EndSceneBytes { get; }
        uint StaticEndSceneDevice { get; }
        uint EndSceneOffsetDevice { get; }
        uint EndSceneOffset { get; }

        uint StaticAutoLootPointer { get; }
        uint OffsetAutolootEnabled { get; }

        uint StaticClickToMovePointer { get; }
        uint OffsetClickToMoveEnabled { get; }

        uint StaticClickToMoveX { get; }
        uint StaticClickToMoveY { get; }
        uint StaticClickToMoveZ { get; }
        uint StaticClickToMoveDistance { get; }
        uint StaticClickToMoveAction { get; }
        uint StaticClickToMoveGuid { get; }
        uint OffsetPlayerRotation { get; }

        uint FunctionSetTarget { get; }
        uint FunctionLuaDoString { get; }
        uint FunctionGetLocalizedText { get; }
        uint FunctionGetActivePlayerObject { get; }

        uint StaticRaidLeader { get; }
        uint StaticPartyLeader { get; }

        uint StaticPartyPlayer1 { get; }
        uint StaticPartyPlayer2 { get; }
        uint StaticPartyPlayer3 { get; }
        uint StaticPartyPlayer4 { get; }

        uint StaticRaidGroupStart { get; }
        uint RaidOffsetPlayer { get; }

        uint StaticCharacterSlotSelected { get; }

        uint OffsetCurrentlyCastingSpellId { get; }
        uint OffsetCurrentlyChannelingSpellId { get; }
    }
}
