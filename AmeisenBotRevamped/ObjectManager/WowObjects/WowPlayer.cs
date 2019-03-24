using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;

namespace AmeisenBotRevamped.ObjectManager.WowObjects
{
    public class WowPlayer : WowUnit
    {
        public int Exp { get; set; }
        public int MaxExp { get; set; }

        public WowClass Class { get; set; }
        public WowRace Race { get; set; }
    }
}
