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
using System.Diagnostics;
using System.Timers;

namespace AmeisenBotRevamped.DataAdapters
{
    public class MemoryWowDataAdapter : IWowDataAdapter
    {
        #region Watchdog Timers
        // Watchdogs to update:
        // - ActiveWowObjects [default every 500ms]
        // - IsWorldLoaded [default every 200ms]

        private int ActiveWowObjectsWatchdogTime => 500;
        private Timer ActiveWowObjectsWatchdog { get; set; }

        private int IsWorldLoadedWatchdogTime => 100;
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

        public ulong PlayerGuid => ReadUInt64(OffsetList.StaticPlayerGuid);
        public ulong TargetGuid => ReadUInt64(OffsetList.StaticTargetGuid);
        public ulong PetGuid => ReadUInt64(OffsetList.StaticPetGuid);
        public int WowBuild => ReadInt(OffsetList.StaticWowBuild);
        public string ContinentName => ReadString(OffsetList.StaticContinentName, 16);
        public int MapId => ReadInt(OffsetList.StaticMapId);
        public int ZoneId => ReadInt(OffsetList.StaticZoneId);
        public int ComboPoints => ReadInt(OffsetList.StaticComboPoints);
        public string LastErrorMessage => ReadString(OffsetList.StaticErrorMessage, 64);
        public string AccountName => ReadString(OffsetList.StaticAccountName, 16);

        public WowPosition ActivePlayerPosition => (WowPosition)ReadObject(ReadUInt(OffsetList.StaticPlayerBase) + OffsetList.OffsetWowUnitPosition, typeof(WowPosition));
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
        public ulong PartyleaderGuid => ReadPartyLeaderGuid();
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

            EnableAutoloot();
            EnableClickToMove();
        }

        private void EnableClickToMove()
        {
            uint ctmPointer = ReadUInt(OffsetList.StaticClickToMovePointer);
            bool ctmEnabled = ReadInt(ctmPointer + OffsetList.OffsetClickToMoveEnabled) == 1;

            if (!ctmEnabled) WriteInt(ctmPointer + OffsetList.OffsetClickToMoveEnabled, 1);
        }

        private void EnableAutoloot()
        {
            uint autolootPointer = ReadUInt(OffsetList.StaticAutoLootPointer);
            bool autolootEnabled = ReadInt(autolootPointer + OffsetList.OffsetAutolootEnabled) == 1;

            if (!autolootEnabled) WriteInt(autolootPointer + OffsetList.OffsetAutolootEnabled, 1);
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
            CharacterName = ReadString(OffsetList.StaticPlayerName, 12),
            RealmName = ReadString(OffsetList.StaticRealmName, 12)
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
            uint clientConnection = ReadUInt(OffsetList.StaticClientConnection);
            uint currentObjectManager = ReadUInt(clientConnection + OffsetList.OffsetCurrentObjectManager);

            uint activeObject = ReadUInt(currentObjectManager + OffsetList.OffsetFirstObject);
            uint objectType = ReadUInt(activeObject + OffsetList.OffsetWowObjectType);

            while (IsWorldLoaded && (objectType <= 7 && objectType > 0))
            {
                WowObjectType wowObjectType = (WowObjectType)objectType;
                switch (wowObjectType)
                {
                    case WowObjectType.Unit: WowObjectList.Add(ReadWowUnit(activeObject, wowObjectType)); break;
                    case WowObjectType.Player: WowObjectList.Add(ReadWowPlayer(activeObject, wowObjectType)); break;

                    default: WowObjectList.Add(ReadWowObject(activeObject, wowObjectType)); break;
                }

                activeObject = ReadUInt(activeObject + OffsetList.OffsetNextObject);
                objectType = ReadUInt(activeObject + OffsetList.OffsetWowObjectType);
            }
        }

        private WowObject ReadWowObject(uint activeObject, WowObjectType wowObjectType) => new WowObject()
        {
            BaseAddress = activeObject,
            DescriptorAddress = ReadUInt(activeObject + OffsetList.OffsetWowObjectDescriptor),
            Guid = ReadUInt64(activeObject + OffsetList.OffsetWowObjectGuid),
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
                TargetGuid = ReadUInt64(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetTargetGuid),
                Position = (WowPosition)ReadObject(activeObject + OffsetList.OffsetWowUnitPosition, typeof(WowPosition)),
                UnitFlags = (BitVector32)ReadObject(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetUnitFlags, typeof(BitVector32)),
                UnitFlagsDynamic = (BitVector32)ReadObject(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetUnitFlagsDynamic, typeof(BitVector32)),
                Health = ReadInt(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetHealth),
                MaxHealth = ReadInt(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetMaxHealth),
                Energy = ReadInt(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetEnergy),
                MaxEnergy = ReadInt(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetMaxEnergy),
                Level = ReadInt(wowObject.DescriptorAddress + OffsetList.DescriptorOffsetLevel),
                CurrentlyCastingSpellId = ReadInt(activeObject + OffsetList.OffsetCurrentlyCastingSpellId),
                CurrentlyChannelingSpellId = ReadInt(activeObject + OffsetList.OffsetCurrentlyChannelingSpellId)
            };
        }

        private WowPlayer ReadWowPlayer(uint activeObject, WowObjectType wowObjectType)
        {
            WowUnit wowUnit = ReadWowUnit(activeObject, wowObjectType);
            WowPlayer player = new WowPlayer()
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
                CurrentlyCastingSpellId = wowUnit.CurrentlyCastingSpellId,
                CurrentlyChannelingSpellId = wowUnit.CurrentlyChannelingSpellId
            };

            if (wowUnit.Guid == PlayerGuid)
            {
                player.Exp = ReadInt(wowUnit.DescriptorAddress + OffsetList.DescriptorOffsetExp);
                player.MaxExp = ReadInt(wowUnit.DescriptorAddress + OffsetList.DescriptorOffsetMaxExp);
                player.Race = (WowRace)ReadByte(OffsetList.StaticRace);
                player.Class = (WowClass)ReadByte(OffsetList.StaticClass);
            }

            return player;
        }

        private string ReadPlayerName(ulong guid)
        {
            if (PlayerNameCache.ContainsKey(guid))
            {
                return PlayerNameCache[guid];
            }

            uint playerMask, playerBase, shortGUID, testGUID, offset, current;

            playerMask = ReadUInt(OffsetList.StaticNameStore + OffsetList.OffsetNameMask);
            playerBase = ReadUInt(OffsetList.StaticNameStore + OffsetList.OffsetNameBase);

            shortGUID = (uint)guid & 0xfffffff;
            offset = 12 * (playerMask & shortGUID);

            current = ReadUInt(playerBase + offset + 8);
            offset = ReadUInt(playerBase + offset);

            if ((current & 0x1) == 0x1) { return ""; }
            testGUID = ReadUInt(current);

            while (testGUID != shortGUID)
            {
                current = ReadUInt(current + offset + 4);
                if ((current & 0x1) == 0x1) { return ""; }
                testGUID = ReadUInt(current);
            }

            string name = ReadString(current + OffsetList.OffsetNameString, 12);

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
                uint objName = ReadUInt(activeObject + 0x964);
                objName = ReadUInt(objName + 0x05C);
                string name = ReadString(objName, 24);

                UnitNameCache.Add(guid, name);
                return name;
            }
            catch { return "unknown"; }
        }

        public List<ulong> ReadPartymemberGuids()
        {
            List<ulong> partymemberGuids = new List<ulong>
            {
                ReadUInt64(OffsetList.StaticPartyPlayer1),
                ReadUInt64(OffsetList.StaticPartyPlayer2),
                ReadUInt64(OffsetList.StaticPartyPlayer3),
                ReadUInt64(OffsetList.StaticPartyPlayer4)
            };

            // try to add raidmembers
            for (uint p = 0; p < 40; p++)
            {
                try
                {
                    uint address = OffsetList.StaticRaidGroupStart + (p * OffsetList.RaidOffsetPlayer);
                    ulong guid = ReadUInt64(address);
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
                partyleaderGuid = ReadUInt64(OffsetList.StaticRaidLeader);
            }
            catch
            {
                partyleaderGuid = ReadUInt64(OffsetList.StaticPartyLeader);
            }

            return partyleaderGuid;
        }

        private void CIsWorldLoadedWatchdog(object sender, ElapsedEventArgs e)
        {
            try
            {
                IsWorldLoaded = ReadUInt(OffsetList.StaticIsWorldLoaded) == 1;
            }
            catch
            {
                CheckForGameCrashed();
                return;
            }

            if (!IsWorldLoaded
                && Enum.TryParse(ReadString(OffsetList.StaticGameState, 12), true, out WowGameState gameState))
            {
                GameState = gameState;
            }
            else
            {
                GameState = WowGameState.World;
            }

            if (IsWorldLoaded != LastIsWorldLoaded || GameState != LastGameState)
            {
                OnGamestateChanged?.Invoke(IsWorldLoaded, GameState);
                BasicInfoDataSet = ReadBasicInfoDataSet();
            }

            LastIsWorldLoaded = IsWorldLoaded;
            LastGameState = GameState;
        }

        private void CheckForGameCrashed()
        {
            if (GameState == WowGameState.Crashed) return;

            try
            {
                Process.GetProcessById(BlackMagic.ProcessId);
            }
            catch
            {
                IsWorldLoadedWatchdog.Stop();
                GameState = WowGameState.Crashed;
                OnGamestateChanged?.Invoke(false, WowGameState.Crashed);
            }
        }

        public void StartObjectUpdates() => ActiveWowObjectsWatchdog.Start();
        public void StopObjectUpdates() => ActiveWowObjectsWatchdog.Stop();

        public void ClearCaches()
        {
            PlayerNameCache = new Dictionary<ulong, string>();
            UnitNameCache = new Dictionary<ulong, string>();

            EnableAutoloot();
            EnableClickToMove();
        }

        private ulong ReadUInt64(uint offset)
        {
            try
            {
                return BlackMagic.ReadUInt64(offset);
            }
            catch { CheckForGameCrashed(); return 0; }
        }

        private int ReadInt(uint offset)
        {
            try
            {
                return BlackMagic.ReadInt(offset);
            }
            catch { CheckForGameCrashed(); return 0; }
        }

        private void WriteInt(uint offset, int value)
        {
            try
            {
                BlackMagic.WriteInt(offset, value);
            }
            catch { CheckForGameCrashed(); }
        }

        private uint ReadUInt(uint offset)
        {
            try
            {
                return BlackMagic.ReadUInt(offset);
            }
            catch { CheckForGameCrashed(); return 0; }
        }

        private byte ReadByte(uint offset)
        {
            try
            {
                return BlackMagic.ReadByte(offset);
            }
            catch { CheckForGameCrashed(); return 0; }
        }

        private string ReadString(uint offset, int lenght)
        {
            try
            {
                return BlackMagic.ReadASCIIString(offset, lenght);
            }
            catch { CheckForGameCrashed(); return ""; }
        }

        private object ReadObject(uint offset, Type type)
        {
            try
            {
                return BlackMagic.ReadObject(offset, type);
            }
            catch { CheckForGameCrashed(); return null; }
        }

        public WowPosition GetPosition(uint baseAddress)
        {
            return (WowPosition)ReadObject(baseAddress + OffsetList.OffsetWowUnitPosition, typeof(WowPosition));
        }
    }
}
