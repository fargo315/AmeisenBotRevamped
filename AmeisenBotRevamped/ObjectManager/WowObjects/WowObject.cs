using AmeisenBotRevamped.ObjectManager.WowObjects.Enums;

namespace AmeisenBotRevamped.ObjectManager.WowObjects
{
    public class WowObject
    {
        public uint BaseAddress { get; set; }
        public uint DescriptorAddress { get; set; }
        public ulong Guid { get; set; }
        public WowObjectType Type { get; set; }
    }
}
