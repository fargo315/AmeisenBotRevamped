using AmeisenBotRevamped.CharacterManager.Objects;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.ObjectManager;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;

namespace AmeisenBotRevamped.CharacterManager.ItemComparator
{
    public class BasicItemLevelComparator : IItemComparator
    {
        public IWowDataAdapter WowDataAdapter { get; private set; }
        public WowObjectManager ObjectManager => WowDataAdapter.ObjectManager;

        public BasicItemLevelComparator(IWowDataAdapter wowDataAdapter)
        {
            WowDataAdapter = wowDataAdapter;
        }

        public bool CompareItems(IItem newItem, IItem currentItem)
        {
            WowPlayer player = (WowPlayer)ObjectManager.GetWowObjectByGuid(WowDataAdapter.PlayerGuid);
            if (ItemUtils.IsItemUsefulForMe(player, newItem))
            {
                if (newItem.ItemLevel > currentItem.ItemLevel)
                    return true;
            }
            return false;
        }
    }
}
