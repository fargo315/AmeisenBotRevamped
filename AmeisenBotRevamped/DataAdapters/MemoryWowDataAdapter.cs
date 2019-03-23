using AmeisenBotRevamped.DataAdapters.DataSets;
using AmeisenBotRevamped.ObjectManager;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using AmeisenBotRevamped.OffsetLists;
using Magic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Timers;

namespace AmeisenBotRevamped.DataAdapters
{
    public class MemoryWowDataAdapter : IWowDataAdapter
    {
        #region Watchdog Timers
        // Watchdogs to update:
        // - ActiveWowObjects [default every 500ms]
        // - IsWorldLoaded [default every 200ms]

        private int ActiveWowObjectsWatchdogTime => 1000;
        private Timer ActiveWowObjectsWatchdog { get; set; }

        private int IsWorldLoadedWatchdogTime => 200;
        private Timer IsWorldLoadedWatchdog { get; set; }
        #endregion

        #region Watchdog Properties
        // Properties updated by the watchdogs

        private List<WowObject> WowObjectList { get; set; }
        public bool IsWorldLoaded { get; private set; }
        public WowGameState GameState { get; private set; }
        public bool LastIsWorldLoaded { get; private set; }
        public WowGameState LastGameState { get; private set; }
        #endregion

        #region Direct Memory Reading
        // Note that if you Access these values you will read it directly from the memory

        public ulong PlayerGuid => BlackMagic.ReadUInt64(OffsetList.StaticPlayerGuid);
        public ulong TargetGuid => BlackMagic.ReadUInt64(OffsetList.StaticTargetGuid);
        public ulong PetGuid => BlackMagic.ReadUInt64(OffsetList.StaticPetGuid);
        public int MapId => BlackMagic.ReadInt(OffsetList.StaticMapId);
        #endregion

        #region Internal Properties
        // You may not need them but who knows

        public IOffsetList OffsetList { get; private set; }
        public BlackMagic BlackMagic { get; private set; }
        #endregion

        #region External Properties
        // Interesting stuff

        public BasicInfoDataSet BasicInfoDataSet { get; private set; }
        public WowObjectManager ObjectManager { get; private set; }

        public List<ulong> PartymemberGuids => ReadPartymemberGuids();
        public ulong PartyLeaderGuid => ReadPartyLeaderGuid();
        public OnGamestateChanged OnGamestateChanged { get; set; }
        public bool ObjectUpdatesEnabled => ActiveWowObjectsWatchdog.Enabled;
        #endregion

        #region Caches
        private Dictionary<ulong, string> PlayerNameCache { get; set; }
        private Dictionary<ulong, string> UnitNameCache { get; set; }
        #endregion

        public MemoryWowDataAdapter(BlackMagic blackMagic, IOffsetList offsetList)
        {
            LastIsWorldLoaded = false;
            PlayerNameCache = new Dictionary<ulong, string>();
            UnitNameCache = new Dictionary<ulong, string>();

            WowObjectList = new List<WowObject>();
            BlackMagic = blackMagic;
            OffsetList = offsetList;

            SetupAdapter();

            ObjectManager = new WowObjectManager(this);

            SetupWatchdogs();
        }

        public void Detach()
        {
            if (ActiveWowObjectsWatchdog.Enabled) { ActiveWowObjectsWatchdog.Stop(); }
            if (IsWorldLoadedWatchdog.Enabled) { IsWorldLoadedWatchdog.Stop(); }
        }

        private void SetupAdapter()
        {
            BasicInfoDataSet = ReadBasicInfoDataSet();
        }

        private void SetupWatchdogs()
        {
            ActiveWowObjectsWatchdog = new Timer(ActiveWowObjectsWatchdogTime);
            ActiveWowObjectsWatchdog.Elapsed += CActiveWowObjectsWatchdog;

            IsWorldLoadedWatchdog = new Timer(IsWorldLoadedWatchdogTime);
            IsWorldLoadedWatchdog.Elapsed += CIsWorldLoadedWatchdog;
            IsWorldLoadedWatchdog.Start();
        }

        private BasicInfoDataSet ReadBasicInfoDataSet() => new BasicInfoDataSet()
        {
            CharacterName = BlackMagic.ReadASCIIString(OffsetList.StaticPlayerName, 12),
            RealmName = BlackMagic.ReadASCIIString(OffsetList.StaticRealmName, 12)
        };

        private void CActiveWowObjectsWatchdog(object sender, ElapsedEventArgs e)
        {
            ReadAllWoWObjects();
            ObjectManager.WowObjects = WowObjectList;
        }

        public void ReadAllWoWObjects()
        {
            if (!IsWorldLoaded) return;

            WowObjectList = new List<WowObject>();
            uint clientConnection = BlackMagic.ReadUInt(OffsetList.StaticClientConnection);
            uint currentObjectManager = BlackMagic.ReadUInt(clientConnection + OffsetList.OffsetCurrentObjectManager);

            uint activeObject = BlackMagic.ReadUInt(currentObjectManager + OffsetList.OffsetFirstObject);
            uint objectType = BlackMagic.ReadUInt(activeObject + OffsetList.OffsetWowObjectType);

            while (IsWorldLoaded && (objectType <= 7 && objectType > 0))
            {
                WowObjectType wowObjectType = (WowObjectType)objectType;
                switch (wowObjectType)
                {
                    case WowObjectType.Unit: WowObjectList.Add(ReadWowUnit(activeObject, wowObjectType)); break;
                    case WowObjectType.Player: WowObjectList.Add(ReadWowPlayer(activeObject, wowObjectType)); break;

                    default: WowObjectList.Add(ReadWowObject(activeObject, wowObjectType)); break;
                }

                activeObject = BlackMagic.ReadUInt(activeObject + OffsetList.OffsetNextObject);
                objectType = BlackMagic.ReadUInt(activeObject + OffsetList.OffsetWowObjectType);
            }
        }

        private WowObject ReadWowObject(uint activeObject, WowObjectType wowObjectType) => new WowObject()
        {
            BaseAddress = activeObject,
            DescriptorAddress = BlackMagic.ReadUInt(activeObject + OffsetList.OffsetWowObjectDescriptor),
            Guid = BlackMagic.ReadUInt64(activeObject + OffsetList.OffsetWowObjectGuid),
            Type = wowObjectType
        };

        private WowUnit ReadWowUnit(uint activeObject, WowObjectType wowObjectType)
        {
            WowObject wowObject = ReadWowObject(activeObject, wowObjectType);
            return new WowUnit()
            {
                BaseAddress = activeObject,
                DescriptorAddress = wowObject.DescriptorAddress,
                Guid = wowObject.Guid,
                Type = wowObjectType,
                Name = ReadUnitName(activeObject, wowObject.Guid),
                TargetGuid = BlackMagic.ReadUInt64(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetTargetGuid),
                Position = (WowPosition)BlackMagic.ReadObject(activeObject + OffsetList.OffsetWowUnitPosition, typeof(WowPosition)),
                UnitFlags = (BitVector32)BlackMagic.ReadObject(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetUnitFlags, typeof(BitVector32)),
                UnitFlagsDynamic = (BitVector32)BlackMagic.ReadObject(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetUnitFlagsDynamic, typeof(BitVector32)),
                Health = BlackMagic.ReadInt(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetHealth),
                MaxHealth = BlackMagic.ReadInt(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetMaxHealth),
                Energy = BlackMagic.ReadInt(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetEnergy),
                MaxEnergy = BlackMagic.ReadInt(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetMaxEnergy),
                Level = BlackMagic.ReadInt(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetLevel)
            };
        }

        private WowPlayer ReadWowPlayer(uint activeObject, WowObjectType wowObjectType)
        {
            WowUnit wowUnit = ReadWowUnit(activeObject, wowObjectType);
            return new WowPlayer()
            {
                BaseAddress = activeObject,
                DescriptorAddress = wowUnit.DescriptorAddress,
                Guid = wowUnit.Guid,
                Type = wowObjectType,
                Name = ReadPlayerName(wowUnit.Guid),
                TargetGuid = wowUnit.TargetGuid,
                Position = wowUnit.Position,
                UnitFlags = wowUnit.UnitFlags,
                UnitFlagsDynamic = wowUnit.UnitFlagsDynamic,
                Health = wowUnit.Health,
                MaxHealth = wowUnit.MaxHealth,
                Energy = wowUnit.Energy,
                MaxEnergy = wowUnit.MaxEnergy,
                Level = wowUnit.Level,
                Race = (WowRace)BlackMagic.ReadByte(wowUnit.DescriptorAddress + OffsetList.DescriptorOffsetRace),
                Class = (WowClass)BlackMagic.ReadByte(OffsetList.StaticClass)
            };
        }

        private string ReadPlayerName(ulong guid)
        {
            if (PlayerNameCache.ContainsKey(guid))
            {
                return PlayerNameCache[guid];
            }

            uint playerMask, playerBase, shortGUID, testGUID, offset, current;

            playerMask = BlackMagic.ReadUInt(OffsetList.StaticNameStore + OffsetList.OffsetNameMask);
            playerBase = BlackMagic.ReadUInt(OffsetList.StaticNameStore + OffsetList.OffsetNameBase);

            shortGUID = (uint)guid & 0xfffffff;
            offset = 12 * (playerMask & shortGUID);

            current = BlackMagic.ReadUInt(playerBase + offset + 8);
            offset = BlackMagic.ReadUInt(playerBase + offset);

            if ((current & 0x1) == 0x1) { return ""; }
            testGUID = BlackMagic.ReadUInt(current);

            while (testGUID != shortGUID)
            {
                current = BlackMagic.ReadUInt(current + offset + 4);
                if ((current & 0x1) == 0x1) { return ""; }
                testGUID = BlackMagic.ReadUInt(current);
            }

            string name = BlackMagic.ReadASCIIString(current + OffsetList.OffsetNameString, 12);

            if (name != "")
                PlayerNameCache.Add(guid, name);

            return name;
        }

        private string ReadUnitName(uint activeObject, ulong guid)
        {
            if (UnitNameCache.ContainsKey(guid))
            {
                return UnitNameCache[guid];
            }

            try
            {
                uint objName = BlackMagic.ReadUInt(activeObject + 0x964);
                objName = BlackMagic.ReadUInt(objName + 0x05C);
                string name = BlackMagic.ReadASCIIString(objName, 24);

                UnitNameCache.Add(guid, name);
                return name;
            }
            catch { return "unknown"; }
        }

        public List<ulong> ReadPartymemberGuids()
        {
            List<ulong> partymemberGuids = new List<ulong>
            {
                BlackMagic.ReadUInt64(OffsetList.StaticPartyPlayer1),
                BlackMagic.ReadUInt64(OffsetList.StaticPartyPlayer2),
                BlackMagic.ReadUInt64(OffsetList.StaticPartyPlayer3),
                BlackMagic.ReadUInt64(OffsetList.StaticPartyPlayer4)
            };

            // try to add raidmembers
            for (uint p = 0; p < 40; p++)
            {
                try
                {
                    uint address = OffsetList.StaticRaidGroupStart + (p * OffsetList.RaidOffsetPlayer);
                    ulong guid = BlackMagic.ReadUInt64(address);
                    if (!partymemberGuids.Contains(guid))
                    {
                        partymemberGuids.Add(guid);
                    }
                }
                catch { }
            }

            return partymemberGuids;
        }

        public ulong ReadPartyLeaderGuid()
        {
            ulong partyleaderGuid = 0;

            try
            {
                partyleaderGuid = BlackMagic.ReadUInt64(OffsetList.StaticRaidLeader);
            }
            catch
            {
                partyleaderGuid = BlackMagic.ReadUInt64(OffsetList.StaticPartyLeader);
            }

            return partyleaderGuid;
        }

        private void CIsWorldLoadedWatchdog(object sender, ElapsedEventArgs e)
        {
            IsWorldLoaded = BlackMagic.ReadUInt(OffsetList.StaticIsWorldLoaded) == 1;

            if (!IsWorldLoaded)
            {
                if (Enum.TryParse(BlackMagic.ReadASCIIString(OffsetList.StaticGameState, 12), true, out WowGameState gameState))
                {
                    GameState = gameState;
                }
            }
            else
            {
                GameState = WowGameState.World;
            }

            if (IsWorldLoaded != LastIsWorldLoaded || GameState != LastGameState)
            {
                OnGamestateChanged?.Invoke(IsWorldLoaded, GameState);
            }

            LastIsWorldLoaded = IsWorldLoaded;
            LastGameState = GameState;
        }

        public void StartObjectUpdates() => ActiveWowObjectsWatchdog.Start();
        public void StopObjectUpdates() => ActiveWowObjectsWatchdog.Stop();
    }
}
