namespace AmeisenBotRevamped.OffsetLists
{
    public class Wotlk335a12340OffsetList : IOffsetList
    {
        public uint StaticPlayerName => 0xC79D18;
        public uint StaticRealmName => 0xC79B9E;
        public uint StaticAccountName => 0xB6AA40;
        public uint StaticWowBuild => 0xA30BE6;
        public uint StaticContinentName => 0xCE06D0;
        public uint StaticMapId => 0xAB63BC;
        public uint StaticZoneId => 0xAF4E48;
        public uint StaticIsWorldLoaded => 0xBEBA40;
        public uint StaticGameState => 0xB6A9E0;
        public uint StaticErrorMessage => 0xBCFB90;
        public uint StaticClientConnection => 0xC79CE0;
        public uint StaticComboPoints => 0xBD0845;
        public uint StaticTickCount => 0xB499A4;

        public uint StaticRace => 0xC79E88;
        public uint StaticClass => 0xC79E89;

        public uint StaticPlayerBase => 0xD38AE4;
        public uint StaticPlayerGuid => 0xCA1238;
        public uint StaticTargetGuid => 0xBD07B0;
        public uint StaticPetGuid => 0xC234D0;

        public uint OffsetCurrentObjectManager => 0x2ED0;
        public uint OffsetFirstObject => 0xAC;
        public uint OffsetNextObject => 0x3C;

        public uint OffsetWowObjectType => 0x14;
        public uint OffsetWowObjectGuid => 0x30;
        public uint OffsetWowObjectDescriptor => 0x8;
        public uint OffsetWowUnitPosition => 0x798;
        public uint OffsetGameObjectPosition => 0xE8;

        public uint DescriptorOffsetTargetGuid => 0x48;
        public uint DescriptorOffsetLevel => 0xD8;
        public uint DescriptorOffsetRace => 0xDC;
        public uint DescriptorOffsetClass => 0xEC;

        public uint DescriptorOffsetHealth => 0x60;
        public uint DescriptorOffsetMaxHealth => 0x80;

        public uint DescriptorOffsetManaGuid => 0x64;
        public uint DescriptorOffsetMaxManaGuid => 0x84;

        public uint DescriptorOffsetRageGuid => 0x68;
        public uint DescriptorOffsetMaxRageGuid => 0x88;

        public uint DescriptorOffsetEnergy => 0x6C;
        public uint DescriptorOffsetMaxEnergy => 0x8C;

        public uint DescriptorOffsetRuneenergyGuid => 0x70;
        public uint DescriptorOffsetMaxRuneenergyGuid => 0x80;

        public uint DescriptorOffsetExp => 0x1E3C;
        public uint DescriptorOffsetMaxExp => 0x1E40;

        public uint DescriptorOffsetUnitFlags => 0xEC;
        public uint DescriptorOffsetUnitFlagsDynamic => 0x240;

        public uint StaticNameStore => 0xC5D940;
        public uint OffsetNameBase => 0x1C;
        public uint OffsetNameMask => 0x24;
        public uint OffsetNameString => 0x20;

        public byte[] EndSceneBytes => new byte[] { 0xB8, 0x51, 0xD7, 0xCA, 0x64 };
        public uint StaticEndSceneDevice => 0xC5DF88;
        public uint EndSceneOffsetDevice => 0x397C;
        public uint EndSceneOffset => 0xA8;

        public uint StaticAutoLootPointer => 0xBD08F4;
        public uint OffsetAutolootEnabled => 0x30;

        public uint StaticClickToMovePointer => 0xBD08F4;
        public uint OffsetClickToMoveEnabled => 0x30;

        public uint StaticClickToMoveX => 0xCA11D8 + 0x8C;
        public uint StaticClickToMoveY => 0xCA11D8 + 0x90;
        public uint StaticClickToMoveZ => 0xCA11D8 + 0x94;
        public uint StaticClickToMoveDistance => 0xCA11D8 + 0xC;
        public uint StaticClickToMoveAction => 0xCA11D8 + 0x1C;
        public uint StaticClickToMoveGuid => 0xCA11D8 + 0x20;
        public uint OffsetPlayerRotation => 0x7A8;

        public uint FunctionSetTarget => 0x524BF0;
        public uint FunctionLuaDoString => 0x819210;
        public uint FunctionGetLocalizedText => 0x7225E0;
        public uint FunctionGetActivePlayerObject => 0x4038F0;

        public uint StaticRaidLeader => 0xBD1990;
        public uint StaticPartyLeader => 0xBD1968;

        public uint StaticPartyPlayer1 => 0xBD1948;
        public uint StaticPartyPlayer2 => 0xBD1950;
        public uint StaticPartyPlayer3 => 0xBD1958;
        public uint StaticPartyPlayer4 => 0xBD1960;

        public uint StaticRaidGroupStart => 0xBF8258;
        public uint RaidOffsetPlayer => 0x50;

        public uint StaticCharacterSlotSelected => 0x6C436C;

        public uint OffsetCurrentlyCastingSpellId => 0xA6C;
        public uint OffsetCurrentlyChannelingSpellId => 0xA80;
    }
}
