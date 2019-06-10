using AmeisenBotRevamped.Clients.Structs;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.Logging.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
                try
                {
                    TcpClient.Connect(Ip, Port);
                }
                catch (SocketException ex)
                {
                    // Server not running
                    AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tUnable to connect to Navmesh Server: \n{ex}", LogLevel.Error);
                }
                catch (Exception ex)
                {
                    AmeisenBotLogger.Instance.Log($"[{ProcessId.ToString("X", CultureInfo.InvariantCulture.NumberFormat)}]\tError occured while connecting to the Navmesh Server: \n{ex}", LogLevel.Error);
                }
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

            if (IsValidJson(pathJson)) {
                return JsonConvert.DeserializeObject<List<Vector3>>(pathJson.Trim());
            }
            else
            {
                return new List<Vector3>();
            }
        }

        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) ||
                (strInput.StartsWith("[") && strInput.EndsWith("]")))
            {
                try
                {
                    JToken obj = JToken.Parse(strInput);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
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
