using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;

namespace AmeisenBotRevamped.Clients.Structs
{
    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(WowPosition pos) : this()
        {
            X = pos.x;
            Y = pos.y;
            Z = pos.z;
        }

        public Vector3(float x, float y, float z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
