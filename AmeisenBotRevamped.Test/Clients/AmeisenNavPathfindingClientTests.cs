using Microsoft.VisualStudio.TestTools.UnitTesting;
using AmeisenBotRevamped.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.Clients.Tests
{
    [TestClass()]
    public class AmeisenNavPathfindingClientTests
    {
        [TestMethod()]
        public void AmeisenNavPathfindingClientTest()
        {
            IPathfindingClient pathfindingClient = new AmeisenNavPathfindingClient("127.0.0.1", 47110, 0x1337);
            pathfindingClient.GetPath(new Structs.Vector3(0,0,0), new Structs.Vector3(0,0,0), 0);
            pathfindingClient.IsInLineOfSight(new Structs.Vector3(0, 0, 0), new Structs.Vector3(0, 0, 0), 0);
        }
    }
}