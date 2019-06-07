using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotRevamped.ObjectManager
{
    public class WowObjectManager
    {
        // This lock prevents changes to the Objects List while a query is running
        private readonly object QueryLock = new object();

        private IWowDataAdapter WowDataAdapter { get; }

        private List<WowObject> wowObjects;

        public List<WowObject> WowObjects {
            get => wowObjects;
            set { lock (QueryLock) { wowObjects = value; } }
        }

        public List<WowUnit> GetWowUnits()
        {
            return wowObjects != null ? wowObjects.OfType<WowUnit>().ToList() : new List<WowUnit>();
        }

        public List<WowPlayer> GetWowPlayers()
        {
            return wowObjects != null ? wowObjects.OfType<WowPlayer>().ToList() : new List<WowPlayer>();
        }

        public WowObjectManager(IWowDataAdapter wowDataAdapter)
        {
            WowDataAdapter = wowDataAdapter;
        }

        public WowObject GetWowObjectByGuid(ulong guid)
        {
            if (WowObjects == null) return null;

            WowObject wowObject;
            lock (QueryLock)
            {
                IEnumerable<WowObject> items = WowObjects.Where(obj => obj.Guid == guid);
                wowObject = items.FirstOrDefault();
            }

            if (wowObject == null) return null;

            switch (wowObject.Type)
            {
                case WowObjectType.Unit: return (WowUnit)wowObject;
                case WowObjectType.Player: return (WowPlayer)wowObject;
                default: return wowObject;
            }
        }

        public T UpdateObject<T>(T objectToUpdate) where T : WowObject
        {
            switch (objectToUpdate.Type)
            {
                case WowObjectType.Unit:
                    return (T)(object)WowDataAdapter.ReadWowUnit(objectToUpdate.BaseAddress);

                case WowObjectType.Player:
                    return (T)(object)WowDataAdapter.ReadWowPlayer(objectToUpdate.BaseAddress);

                default:
                    return (T)(object)WowDataAdapter.ReadWowObject(objectToUpdate.BaseAddress);
            }
        }
    }
}
