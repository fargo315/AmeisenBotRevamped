using AmeisenBotRevamped.Clients.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.ObjectManager.WowObjects.Structs
{
    public struct WowPosition
    {
        public float x;
        public float y;
        public float z;
        public float r;

        public WowPosition(Vector3 pos) : this()
        {
            x = pos.X;
            y = pos.Y;
            z = pos.Z;
            r = 0f;
        }
    }
}
