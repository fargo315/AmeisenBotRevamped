using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.CharacterManager.Enums;
using AmeisenBotRevamped.CharacterManager.ItemComparator;
using AmeisenBotRevamped.CharacterManager.Objects;
using AmeisenBotRevamped.DataAdapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.CharacterManager
{
    public class CharacterManager
    {
        public IWowDataAdapter WowDataAdapter { get;private set; }
        public IWowActionExecutor WowActionExecutor { get; private set; }
        public IItemComparator ItemComparator { get; private set; }

        public Dictionary<EquipmentSlot, IItem> CurrentEquipment { get; private set; }

        public CharacterManager(IWowDataAdapter wowDataAdapter, IWowActionExecutor wowActionExecutor, IItemComparator itemComparator)
        {
            CurrentEquipment = new Dictionary<EquipmentSlot, IItem>();

            WowDataAdapter = wowDataAdapter;
            WowActionExecutor = wowActionExecutor;
            ItemComparator = itemComparator;
        }

        public void ScanCharacter()
        {
            ScanEquipment();
        }

        public void ScanEquipment()
        {

        }
    }
}
