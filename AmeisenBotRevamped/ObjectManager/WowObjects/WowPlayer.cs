using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;

namespace AmeisenBotRevamped.ObjectManager.WowObjects
{
    public class WowPlayer : WowUnit
    {
        public WowClass Class { get; set; }
        public WowRace Race { get; set; }
    }
}
