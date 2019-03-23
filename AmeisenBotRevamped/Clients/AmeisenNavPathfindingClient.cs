using AmeisenBotRevamped.Clients.Structs;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace AmeisenBotRevamped.Clients
{
    public class AmeisenNavPathfindingClient : IPathfindingClient
    {
        public IPAddress Ip { get; private set; }
        public int Port { get; private set; }
        public TcpClient TcpClient { get; private set; }
        public bool IsConnected { get; private set; }

        private Timer ConnectionWatchdog { get; set; }

        public AmeisenNavPathfindingClient(string ip, int port)
        {
            Ip = IPAddress.Parse(ip);
            Port = port;
            TcpClient = new TcpClient();

            ConnectionWatchdog = new Timer(1000);
            ConnectionWatchdog.Elapsed += CConnectionWatchdog;
            ConnectionWatchdog.Start();
        }

        private void CConnectionWatchdog(object sender, ElapsedEventArgs e)
        {
            if (!TcpClient.Connected)
            {
                try
                {
                    TcpClient.Connect(Ip, Port);
                }
                catch { }
            }

            IsConnected = TcpClient.Connected;
        }

        public List<Vector3> GetPath(Vector3 start, Vector3 end, int mapId)
        {
            if (!TcpClient.Connected) return new List<Vector3>();
            StreamReader sReader = new StreamReader(TcpClient.GetStream(), Encoding.ASCII);
            StreamWriter sWriter = new StreamWriter(TcpClient.GetStream(), Encoding.ASCII);

            sWriter.WriteLine(JsonConvert.SerializeObject(new PathRequest(start, end, mapId)) + " &gt;");
            sWriter.Flush();

            string pathJson = sReader.ReadLine().Replace("&gt;", "");
            return JsonConvert.DeserializeObject<List<Vector3>>(pathJson);
        }

        public bool IsInLineOfSight(Vector3 start, Vector3 end)
        {
            return true;
        }

        public void Disconnect()
        {
            ConnectionWatchdog.Stop();
            TcpClient.Close();
        }
    }
}
