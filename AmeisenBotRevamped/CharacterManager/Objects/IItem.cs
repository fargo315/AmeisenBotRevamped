using AmeisenBotRevamped.CharacterManager.Enums;

namespace AmeisenBotRevamped.CharacterManager.Objects
{
    public interface IItem
    {
        int Id { get; }
        string Type { get; }
        string Subtype { get; }
        string Name { get; }
        string ItemLink { get; }
        EquipmentSlot EquipLocation { get; }
        ItemQuality ItemQuality { get; }
        int ItemLevel { get; }
        int RequiredLevel { get; }
        int Price { get; }
        int Count { get; }
        int MaxStack { get; }
        int Durability { get; }
        int MaxDurability { get; }
    }
}
