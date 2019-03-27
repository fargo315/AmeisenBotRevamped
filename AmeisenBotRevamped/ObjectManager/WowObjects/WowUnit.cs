using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using System.Collections.Specialized;

namespace AmeisenBotRevamped.ObjectManager.WowObjects
{
    public class WowUnit : WowObject
    {
        public string Name { get; set; }

        public ulong TargetGuid { get; set; }

        public int Level { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }

        public int Energy { get; set; }
        public int MaxEnergy { get; set; }

        public WowPosition Position { get; set; }

        public int FactionTemplate { get; set; }

        public BitVector32 UnitFlags { get; set; }
        public BitVector32 UnitFlagsDynamic { get; set; }

        public int CurrentlyCastingSpellId { get; set; }
        public int CurrentlyChannelingSpellId { get; set; }

        public bool IsInCombat => UnitFlags[(int)Enums.UnitFlags.Combat];
        public bool IsSitting => UnitFlags[(int)Enums.UnitFlags.Sitting];
        public bool IsTotem => UnitFlags[(int)Enums.UnitFlags.Totem];
        public bool IsNotAttackable => UnitFlags[(int)Enums.UnitFlags.NotAttackable];
        public bool IsLooting => UnitFlags[(int)Enums.UnitFlags.Looting];
        public bool IsPetInCombat => UnitFlags[(int)Enums.UnitFlags.PetInCombat];
        public bool IsPvpFlagged => UnitFlags[(int)Enums.UnitFlags.PvpFlagged];
        public bool IsSilenced => UnitFlags[(int)Enums.UnitFlags.Silenced];
        public bool IsInFlightmasterFlight => UnitFlags[(int)Enums.UnitFlags.FlightmasterFlight];
        public bool IsDisarmed => UnitFlags[(int)Enums.UnitFlags.Disarmed];
        public bool IsConfused => UnitFlags[(int)Enums.UnitFlags.Confused];
        public bool IsFleeing => UnitFlags[(int)Enums.UnitFlags.Fleeing];
        public bool IsSkinnable => UnitFlags[(int)Enums.UnitFlags.Skinnable];
        public bool IsMounted => UnitFlags[(int)Enums.UnitFlags.Mounted];
        public bool IsDazed => UnitFlags[(int)Enums.UnitFlags.Dazed];

        public bool IsLootable => UnitFlagsDynamic[(int)Enums.UnitFlagsDynamic.Lootable];
        public bool IsTrackedUnit => UnitFlagsDynamic[(int)Enums.UnitFlagsDynamic.TrackUnit];
        public bool IsTaggedByOther => UnitFlagsDynamic[(int)Enums.UnitFlagsDynamic.TaggedByOther];
        public bool IsTaggedByMe => UnitFlagsDynamic[(int)Enums.UnitFlagsDynamic.TaggedByMe];
        public bool IsSpecialInfo => UnitFlagsDynamic[(int)Enums.UnitFlagsDynamic.SpecialInfo];
        public bool IsDead => UnitFlagsDynamic[(int)Enums.UnitFlagsDynamic.Dead];
        public bool IsReferAFriendLinked => UnitFlagsDynamic[(int)Enums.UnitFlagsDynamic.ReferAFriendLinked];
        public bool IsTappedByThreat => UnitFlagsDynamic[(int)Enums.UnitFlagsDynamic.TappedByThreat];
    }
}
