﻿using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.CharacterManager.Enums;
using AmeisenBotRevamped.CharacterManager.ItemComparator;
using AmeisenBotRevamped.CharacterManager.Objects;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.Logging.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotRevamped.CharacterManager
{
    public class WowCharacterManager
    {
        public IWowDataAdapter WowDataAdapter { get; }
        public IWowActionExecutor WowActionExecutor { get; }
        public IItemComparator ItemComparator { get; }

        public Dictionary<EquipmentSlot, IItem> CurrentEquipment { get; }
        public List<IItem> InventoryItems { get; }

        public List<ArmorItem> GetArmorItems()
        {
            return InventoryItems.OfType<ArmorItem>().ToList();
        }

        public List<ConsumableItem> GetConsumableItems()
        {
            return InventoryItems.OfType<ConsumableItem>().ToList();
        }

        public List<ContainerItem> GetContainerItems()
        {
            return InventoryItems.OfType<ContainerItem>().ToList();
        }

        public List<GemItem> GetGemItems()
        {
            return InventoryItems.OfType<GemItem>().ToList();
        }

        public List<KeyItem> GetKeyItems()
        {
            return InventoryItems.OfType<KeyItem>().ToList();
        }

        public List<MiscellaneousItem> GetMiscellaneousItems()
        {
            return InventoryItems.OfType<MiscellaneousItem>().ToList();
        }

        public List<MoneyItem> GetMoneyItems()
        {
            return InventoryItems.OfType<MoneyItem>().ToList();
        }

        public List<ProjectileItem> GetProjectileItems()
        {
            return InventoryItems.OfType<ProjectileItem>().ToList();
        }

        public List<QuestItem> GetQuestItems()
        {
            return InventoryItems.OfType<QuestItem>().ToList();
        }

        public List<QuiverItem> GetQuiverItems()
        {
            return InventoryItems.OfType<QuiverItem>().ToList();
        }

        public List<ReagentItem> GetReagentItems()
        {
            return InventoryItems.OfType<ReagentItem>().ToList();
        }

        public List<RecipeItem> GetRecipeItems()
        {
            return InventoryItems.OfType<RecipeItem>().ToList();
        }

        public List<TradeGoodItem> GetTradeGoodItems()
        {
            return InventoryItems.OfType<TradeGoodItem>().ToList();
        }

        public List<WeaponItem> GetWeaponItems()
        {
            return InventoryItems.OfType<WeaponItem>().ToList();
        }

        public WowCharacterManager(IWowDataAdapter wowDataAdapter, IWowActionExecutor wowActionExecutor, IItemComparator itemComparator)
        {
            CurrentEquipment = new Dictionary<EquipmentSlot, IItem>();

            WowDataAdapter = wowDataAdapter;
            WowActionExecutor = wowActionExecutor;
            ItemComparator = itemComparator;
        }

        public void UpdateFullCharacter()
        {
            UpdateInventoryItems();
            UpdateEquipment();
        }

        public void UpdateInventoryItems()
        {
            if (WowDataAdapter.IsWorldLoaded)
            {
                string resultJson = ReadInventoryItems();

                List<RawItem> rawItems = JsonConvert.DeserializeObject<List<RawItem>>(resultJson);
                if (InventoryItems != null)
                {
                    InventoryItems.Clear();
                    foreach (RawItem rawItem in rawItems)
                    {
                        InventoryItems.Add(ItemFactory.BuildSpecificItem(rawItem));
                    }
                    AmeisenBotLogger.Instance.Log($"Total Items in Inventory: {InventoryItems.Count}", LogLevel.Verbose);
                }
            }
        }

        private string ReadInventoryItems()
        {
            string command = $"abotInventoryResult='['for b=0,4 do containerSlots=GetContainerNumSlots(b); for a=1,containerSlots do abItemLink=GetContainerItemLink(b,a)if abItemLink then abCurrentDurability,abMaxDurability=GetContainerItemDurability(b,a)abCooldownStart,abCooldownEnd=GetContainerItemCooldown(b,a)abIcon,abItemCount,abLocked,abQuality,abReadable,abLootable,abItemLink,isFiltered=GetContainerItemInfo(b,a)abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(abItemLink)abotInventoryResult=abotInventoryResult..'{{'..'\"id\": \"'..tostring(abId or 0)..'\",'..'\"count\": \"'..tostring(abItemCount or 0)..'\",'..'\"quality\": \"'..tostring(abQuality or 0)..'\",'..'\"curDurability\": \"'..tostring(abCurrentDurability or 0)..'\",'..'\"maxDurability\": \"'..tostring(abMaxDurability or 0)..'\",'..'\"cooldownStart\": \"'..tostring(abCooldownStart or 0)..'\",'..'\"cooldownEnd\": \"'..tostring(abCooldownEnd or 0)..'\",'..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"lootable\": \"'..tostring(abLootable or 0)..'\",'..'\"readable\": \"'..tostring(abReadable or 0)..'\",'..'\"link\": \"'..tostring(abItemLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equiplocation\": \"'..tostring(abEquipLoc or 0)..'\",'..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\"'..'}}'if b<4 or a<containerSlots then abotInventoryResult=abotInventoryResult..','end end end end;abotInventoryResult=abotInventoryResult..']'";
            WowActionExecutor?.LuaDoString(command);
            return WowActionExecutor?.GetLocalizedText("abotInventoryResult");
        }

        private void UpdateEquipment()
        {
            CurrentEquipment.Clear();
            foreach (EquipmentSlot equipmentSlot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (equipmentSlot == EquipmentSlot.NOT_EQUIPABLE)
                {
                    continue;
                }

                try
                {
                    string resultJson = ReadEquitmentSlot(equipmentSlot);

                    if (resultJson == "noItem")
                    {
                        continue;
                    }

                    RawItem rawItem = JsonConvert.DeserializeObject<RawItem>(resultJson);
                    CurrentEquipment.Add(equipmentSlot, ItemFactory.BuildSpecificItem(rawItem));
                    AmeisenBotLogger.Instance.Log($"Parsed EquipmentItem: {equipmentSlot.ToString()} => {rawItem.name}", LogLevel.Error);
                }
                catch (Exception ex)
                {
                    AmeisenBotLogger.Instance.Log($"Crash at updating the EquipmentSlot [{equipmentSlot.ToString()}]: \n{ex}", LogLevel.Error);
                }
            }
        }

        private string ReadEquitmentSlot(EquipmentSlot equipmentSlot)
        {
            string command = $"abotItemSlot={(int)equipmentSlot};abotItemInfoResult='noItem';abId=GetInventoryItemID('player',abotItemSlot);abCount=GetInventoryItemCount('player',abotItemSlot);abQuality=GetInventoryItemQuality('player',abotItemSlot);abCurrentDurability,abMaxDurability=GetInventoryItemDurability(abotItemSlot);abCooldownStart,abCooldownEnd=GetInventoryItemCooldown('player',abotItemSlot);abName,abLink,abRarity,abLevel,abMinLevel,abType,abSubType,abStackCount,abEquipLoc,abIcon,abSellPrice=GetItemInfo(GetInventoryItemLink('player',abotItemSlot));abotItemInfoResult='{{'..'\"id\": \"'..tostring(abId or 0)..'\",'..'\"count\": \"'..tostring(abCount or 0)..'\",'..'\"quality\": \"'..tostring(abQuality or 0)..'\",'..'\"curDurability\": \"'..tostring(abCurrentDurability or 0)..'\",'..'\"maxDurability\": \"'..tostring(abMaxDurability or 0)..'\",'..'\"cooldownStart\": \"'..tostring(abCooldownStart or 0)..'\",'..'\"cooldownEnd\": '..tostring(abCooldownEnd or 0)..','..'\"name\": \"'..tostring(abName or 0)..'\",'..'\"link\": \"'..tostring(abLink or 0)..'\",'..'\"level\": \"'..tostring(abLevel or 0)..'\",'..'\"minLevel\": \"'..tostring(abMinLevel or 0)..'\",'..'\"type\": \"'..tostring(abType or 0)..'\",'..'\"subtype\": \"'..tostring(abSubType or 0)..'\",'..'\"maxStack\": \"'..tostring(abStackCount or 0)..'\",'..'\"equiplocation\": \"'..tostring(abEquipLoc or 0)..'\",'..'\"sellprice\": \"'..tostring(abSellPrice or 0)..'\"'..'}}';";
            WowActionExecutor?.LuaDoString(command);
            return WowActionExecutor?.GetLocalizedText("abotItemInfoResult");
        }
    }
}
