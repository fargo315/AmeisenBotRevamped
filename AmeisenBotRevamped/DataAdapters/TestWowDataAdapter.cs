using AmeisenBotRevamped.DataAdapters.DataSets;
using AmeisenBotRevamped.ObjectManager;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AmeisenBotRevamped.DataAdapters
{
    public class TestWowDataAdapter : IWowDataAdapter
    {
        public ulong PlayerGuid => 0x1;
        public ulong TargetGuid => 0x2;
        public ulong PetGuid => 0x10;
        public int MapId => 0;

        public BasicInfoDataSet BasicInfoDataSet => new BasicInfoDataSet() { CharacterName = "TCharacter", RealmName = "TRealm" };

        public WowObjectManager ObjectManager { get; private set; }

        public bool IsWorldLoaded { get; set; }

        public WowGameState GameState => WowGameState.World;

        public List<ulong> PartymemberGuids => new List<ulong>() { 0x2, 0x10 };

        public ulong PartyleaderGuid => 0x2;

        public OnGamestateChanged OnGamestateChanged { get; set; }

        public bool ObjectUpdatesEnabled => false;

        public int ZoneId => 1000;

        public int ComboPoints => 4;

        public int WowBuild => 12340;

        public string ContinentName => "Trash";

        public string LastErrorMessage => "You suck";

        public WowPosition ActivePlayerPosition => new WowPosition();

        public string AccountName => "Schisch";

        public TestWowDataAdapter()
        {
            IsWorldLoaded = true;

            ObjectManager = new WowObjectManager(this)
            {
                WowObjects = GetFakeObjects(false, false)
        };
        }

        public void SetMeInCombat(bool v)
        {
            ObjectManager.WowObjects = GetFakeObjects(v, false);
        }

        private List<WowObject> GetFakeObjects(bool isMeInCombat, bool isPartyInCombat)
        {
            return new List<WowObject>
                {
                    new WowPlayer(){
                        BaseAddress = 0x0,
                        DescriptorAddress = 0x0,
                        Guid = 0x1,
                        TargetGuid = 0x2,
                        Name = "TCharacter",
                        Level = 80,
                        Health = 1000,
                        MaxHealth = 1000,
                        Race = WowRace.Human,
                        Class = WowClass.Mage,
                        Energy = 800,
                        MaxEnergy = 800,
                        Type = WowObjectType.Player,
                        Position = new WowPosition {
                            x = 0,
                            y = 0,
                            z = 0,
                            r = 0,
                        },
                        UnitFlags = isMeInCombat ? new BitVector32((int)UnitFlags.Combat) : new BitVector32(),
                        UnitFlagsDynamic = new BitVector32()
                    },
                    new WowPlayer(){
                        BaseAddress = 0x0,
                        DescriptorAddress = 0x0,
                        Guid = 0x2,
                        TargetGuid = 0x1,
                        Name = "TCharacter2",
                        Level = 80,
                        Health = 1200,
                        MaxHealth = 1200,
                        Race = WowRace.Human,
                        Class = WowClass.Warrior,
                        Energy = 100,
                        MaxEnergy = 100,
                        Type = WowObjectType.Player,
                        Position = new WowPosition {
                            x = 4,
                            y = 4,
                            z = 0,
                            r = 0,
                        },
                        UnitFlags = isPartyInCombat ? new BitVector32((int)UnitFlags.Combat) : new BitVector32(),
                        UnitFlagsDynamic = new BitVector32()
                    },
                    new WowUnit(){
                        BaseAddress = 0x0,
                        DescriptorAddress = 0x0,
                        Guid = 0x10,
                        TargetGuid = 0x1,
                        Name = "TPet",
                        Level = 80,
                        Health = 600,
                        MaxHealth = 600,
                        Energy = 100,
                        MaxEnergy = 100,
                        Type = WowObjectType.Unit,
                        Position = new WowPosition {
                            x = 2,
                            y = 2,
                            z = 0,
                            r = 0,
                        },
                        UnitFlags = new BitVector32(),
                        UnitFlagsDynamic = new BitVector32()
                    },
                    new WowObject(){
                        BaseAddress = 0x0,
                        DescriptorAddress = 0x0,
                        Guid = 0x50,
                        Type = WowObjectType.Gameobject
                    },
                };
        }

        public void SetPartyInCombat(bool v)
        {
            ObjectManager.WowObjects = GetFakeObjects(false, v);
        }

        public void SetIsWorldLoaded(bool v)
        {
            IsWorldLoaded = v;
            OnGamestateChanged?.Invoke(IsWorldLoaded, GameState);
        }

        public void StartObjectUpdates()
        {

        }

        public void StopObjectUpdates()
        {

        }

        public WowPosition GetPosition(uint baseAddress)
        {
            return new WowPosition();
        }

        public WowObject ReadWowObject(uint activeObject, WowObjectType wowObjectType = WowObjectType.None)
        {
            return null;
        }

        public WowUnit ReadWowUnit(uint activeObject, WowObjectType wowObjectType = WowObjectType.Unit)
        {
            return null;
        }

        public WowPlayer ReadWowPlayer(uint activeObject, WowObjectType wowObjectType = WowObjectType.Player)
        {
            return null;
        }
    }
}
