namespace AmeisenBotRevamped.OffsetLists
{
    public class Wotlk335A12340OffsetList : IOffsetList
    {
        public uint StaticPlayerName { get; } = 0xC79D18;
        public uint StaticRealmName { get; } = 0xC79B9E;
        public uint StaticAccountName { get; } = 0xB6AA40;
        public uint StaticWowBuild { get; } = 0xA30BE6;
        public uint StaticContinentName { get; } = 0xCE06D0;
        public uint StaticMapId { get; } = 0xAB63BC;
        public uint StaticZoneId { get; } = 0xAF4E48;
        public uint StaticIsWorldLoaded { get; } = 0xBEBA40;
        public uint StaticGameState { get; } = 0xB6A9E0;
        public uint StaticErrorMessage { get; } = 0xBCFB90;
        public uint StaticClientConnection { get; } = 0xC79CE0;
        public uint StaticComboPoints { get; } = 0xBD0845;
        public uint StaticTickCount { get; } = 0xB499A4;

        public uint StaticRace { get; } = 0xC79E88;
        public uint StaticClass { get; } = 0xC79E89;

        public uint StaticPlayerBase { get; } = 0xD38AE4;
        public uint StaticPlayerGuid { get; } = 0xCA1238;
        public uint StaticTargetGuid { get; } = 0xBD07B0;
        public uint StaticPetGuid { get; } = 0xC234D0;
        public uint StaticLastTargetGuid { get; } = 0xBD07B8;

        public uint StaticBattlegroundStatus { get; } = 0xBEA4D0;
        public uint StaticIsIndoors { get; } = 0xB4AA94;

        public uint StaticChatBufferStart { get; } = 0xB75A60;
        public uint StaticChatBufferCount { get; } = 0xBCEFF4;
        public uint OffsetChatBufferNext { get; } = 0x17C0;

        public uint OffsetCurrentObjectManager { get; } = 0x2ED0;
        public uint OffsetFirstObject { get; } = 0xAC;
        public uint OffsetNextObject { get; } = 0x3C;

        public uint OffsetWowObjectType { get; } = 0x14;
        public uint OffsetWowObjectGuid { get; } = 0x30;
        public uint OffsetWowObjectDescriptor { get; } = 0x8;
        public uint OffsetWowUnitPosition { get; } = 0x798;
        public uint OffsetGameObjectPosition { get; } = 0xE8;

        public uint DescriptorOffsetTargetGuid { get; } = 0x48;
        public uint DescriptorOffsetLevel { get; } = 0xD8;
        public uint DescriptorOffsetRace { get; } = 0xDC;
        public uint DescriptorOffsetClass { get; } = 0xEC;
        public uint DescriptorOffsetFactionTemplate { get; } = 0xDC;

        public uint DescriptorOffsetHealth { get; } = 0x60;
        public uint DescriptorOffsetMaxHealth { get; } = 0x80;

        public uint DescriptorOffsetManaGuid { get; } = 0x64;
        public uint DescriptorOffsetMaxManaGuid { get; } = 0x84;

        public uint DescriptorOffsetRageGuid { get; } = 0x68;
        public uint DescriptorOffsetMaxRageGuid { get; } = 0x88;

        public uint DescriptorOffsetEnergy { get; } = 0x6C;
        public uint DescriptorOffsetMaxEnergy { get; } = 0x8C;

        public uint DescriptorOffsetRuneenergyGuid { get; } = 0x70;
        public uint DescriptorOffsetMaxRuneenergyGuid { get; } = 0x80;

        public uint DescriptorOffsetExp { get; } = 0x1E3C;
        public uint DescriptorOffsetMaxExp { get; } = 0x1E40;

        public uint DescriptorOffsetUnitFlags { get; } = 0xEC;
        public uint DescriptorOffsetUnitFlagsDynamic { get; } = 0x240;

        public uint StaticNameStore { get; } = 0xC5D940;
        public uint OffsetNameBase { get; } = 0x1C;
        public uint OffsetNameMask { get; } = 0x24;
        public uint OffsetNameString { get; } = 0x20;

        public byte[] EndSceneBytes { get; } = new byte[] { 0xB8, 0x51, 0xD7, 0xCA, 0x64 };
        public uint StaticEndSceneDevice { get; } = 0xC5DF88;
        public uint EndSceneOffsetDevice { get; } = 0x397C;
        public uint EndSceneOffset { get; } = 0xA8;

        public uint StaticAutoLootPointer { get; } = 0xBD08F4;
        public uint OffsetAutolootEnabled { get; } = 0x30;

        public uint StaticClickToMovePointer { get; } = 0xBD08F4;
        public uint OffsetClickToMoveEnabled { get; } = 0x30;

        public uint StaticClickToMoveX { get; } = 0xCA11D8 + 0x8C;
        public uint StaticClickToMoveY { get; } = 0xCA11D8 + 0x90;
        public uint StaticClickToMoveZ { get; } = 0xCA11D8 + 0x94;
        public uint StaticClickToMoveDistance { get; } = 0xCA11D8 + 0xC;
        public uint StaticClickToMoveAction { get; } = 0xCA11D8 + 0x1C;
        public uint StaticClickToMoveGuid { get; } = 0xCA11D8 + 0x20;
        public uint OffsetPlayerRotation { get; } = 0x7A8;

        public uint FunctionSetTarget { get; } = 0x524BF0;
        public uint FunctionLuaDoString { get; } = 0x819210;
        public uint FunctionGetLocalizedText { get; } = 0x7225E0;
        public uint FunctionGetActivePlayerObject { get; } = 0x4038F0;
        public uint FunctionSendMovementPacket { get; } = 0x7413F0;
        public uint FunctionGetUnitReaction { get; } = 0x7251C0;

        public uint StaticRaidLeader { get; } = 0xBD1990;
        public uint StaticPartyLeader { get; } = 0xBD1968;

        public uint StaticPartyPlayer1 { get; } = 0xBD1948;
        public uint StaticPartyPlayer2 { get; } = 0xBD1950;
        public uint StaticPartyPlayer3 { get; } = 0xBD1958;
        public uint StaticPartyPlayer4 { get; } = 0xBD1960;

        public uint StaticRaidGroupStart { get; } = 0xBF8258;
        public uint RaidOffsetPlayer { get; } = 0x50;

        public uint StaticCharacterSlotSelected { get; } = 0xAC436C;

        public uint OffsetCurrentlyCastingSpellId { get; } = 0xA6C;
        public uint OffsetCurrentlyChannelingSpellId { get; } = 0xA80;
    }
}
