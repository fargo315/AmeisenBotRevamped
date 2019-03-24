using AmeisenBotRevamped.Clients.Structs;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.Clients
{
    public interface IPathfindingClient
    {
        List<Vector3> GetPath(Vector3 start, Vector3 end, int mapId);
        bool IsInLineOfSight(Vector3 start, Vector3 end, int mapId);
        void Disconnect();
    }
}
