using AmeisenBotRevamped.Clients.Structs;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.Logging.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace AmeisenBotRevamped.Clients
{
    public class AmeisenNavPathfindingClient : IPathfindingClient
    {
        public IPAddress Ip { get; }
        public int Port { get; }
        public TcpClient TcpClient { get; }
        public bool IsConnected { get; private set; }

        private Timer ConnectionWatchdog { get; }
        private int ProcessId { get; }

        public AmeisenNavPathfindingClient(string ip, int port, int processId)
        {
            Ip = IPAddress.Parse(ip);
            Port = port;
            ProcessId = processId;
            TcpClient = new TcpClient();

            ConnectionWatchdog = new Timer(1000);
            ConnectionWatchdog.Elapsed += CConnectionWatchdog;
            ConnectionWatchdog.Start();
        }

        private void CConnectionWatchdog(object sender, ElapsedEventArgs e)
        {
            if (!TcpClient.Connected)
            {
                AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tConnecting to NavmeshServer {Ip}:{Port}", LogLevel.Verbose);
                TcpClient.Connect(Ip, Port);
            }

            if (TcpClient?.Client != null)
            {
                IsConnected = TcpClient.Connected;
            }
        }

        public List<Vector3> GetPath(Vector3 start, Vector3 end, int mapId)
        {
            if (!TcpClient.Connected)
            {
                return new List<Vector3>();
            }

            StreamReader sReader = new StreamReader(TcpClient.GetStream(), Encoding.ASCII);
            StreamWriter sWriter = new StreamWriter(TcpClient.GetStream(), Encoding.ASCII);

            string pathRequest = JsonConvert.SerializeObject(new PathRequest(start, end, mapId));
            AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tSending PathRequest to server: {pathRequest}", LogLevel.Verbose);

            sWriter.WriteLine(pathRequest + " &gt;");
            sWriter.Flush();

            string pathJson = sReader.ReadLine().Replace("&gt;", "");
            AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tServer returned: {pathJson}", LogLevel.Verbose);

            return JsonConvert.DeserializeObject<List<Vector3>>(pathJson);
        }

        public bool IsInLineOfSight(Vector3 start, Vector3 end, int mapId)
        {
            return GetPath(start, end, mapId).Count == 1;
        }

        public void Disconnect()
        {
            ConnectionWatchdog.Stop();
            TcpClient.Close();
        }
    }
}
