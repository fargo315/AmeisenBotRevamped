using AmeisenBotRevamped.CharacterManager.Objects;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.ObjectManager;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;

namespace AmeisenBotRevamped.CharacterManager.ItemComparator
{
    public class BasicItemLevelComparator : IItemComparator
    {
        public IWowDataAdapter WowDataAdapter { get; }
        public WowObjectManager ObjectManager => WowDataAdapter.ObjectManager;

        public BasicItemLevelComparator(IWowDataAdapter wowDataAdapter)
        {
            WowDataAdapter = wowDataAdapter;
        }

        public bool CompareItems(IItem newItem, IItem currentItem)
        {
            WowPlayer player = (WowPlayer)ObjectManager.GetWowObjectByGuid(WowDataAdapter.PlayerGuid);
            return ItemUtils.IsItemUsefulForMe(player, newItem) 
                && newItem.ItemLevel > currentItem.ItemLevel;
        }
    }
}
