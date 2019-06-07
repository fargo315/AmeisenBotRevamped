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

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return (int)((X * 23 * 23)
                + (Y * 23)
                + Z);
        }

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !(left == right);
        }
    }
}
