using AmeisenBotRevamped.CharacterManager.Objects;

namespace AmeisenBotRevamped.CharacterManager.ItemComparator
{
    public interface IItemComparator
    {
        /// <summary>
        /// Compare two IItems
        /// </summary>
        /// <returns>returns true if the first item is better</returns>
        bool CompareItems(IItem newItem, IItem currentItem);
    }
}
