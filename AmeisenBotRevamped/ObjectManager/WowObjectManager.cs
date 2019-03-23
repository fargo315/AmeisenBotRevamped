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

        private IWowDataAdapter WowDataAdapter { get; set; }

        private List<WowObject> wowObjects;
        public List<WowObject> WowObjects {
            get => wowObjects;
            set { lock (QueryLock) { wowObjects = value; } }
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
                wowObject = items.Count() > 0 ? items.First() : null;
            }

            if (wowObject == null) return null;

            switch (wowObject.Type)
            {
                case WowObjectType.Unit: return (WowUnit)wowObject;
                case WowObjectType.Player: return (WowPlayer)wowObject;
                default: return wowObject;
            }
        }
    }
}
