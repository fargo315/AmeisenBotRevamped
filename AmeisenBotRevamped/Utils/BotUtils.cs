using AmeisenBotRevamped.OffsetLists;
using TrashMemCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AmeisenBotRevamped.Utils
{
    public static class BotUtils
    {
        public static List<WowProcess> GetRunningWows(IOffsetList offsetList)
        {
            List<WowProcess> wows = new List<WowProcess>();
            List<Process> processList = new List<Process>(Process.GetProcessesByName("Wow"));

            foreach (Process p in processList)
            {
                TrashMem trashMem = new TrashMem(p);
                uint pDevice = trashMem.ReadUnmanaged<uint>(offsetList.StaticEndSceneDevice);
                uint pEnd = trashMem.ReadUnmanaged<uint>(pDevice + offsetList.EndSceneOffsetDevice);
                uint pScene = trashMem.ReadUnmanaged<uint>(pEnd);
                uint endscene = trashMem.ReadUnmanaged<uint>(pScene + offsetList.EndSceneOffset);

                bool isAlreadyHooked = false;
                try
                {
                    isAlreadyHooked = trashMem.ReadChar(endscene + 0x2) == 0xE9;
                }
                catch { }

                string name = trashMem.ReadString(offsetList.StaticPlayerName, Encoding.ASCII, 12);
                if (name.Length == 0)
                {
                    name = "";
                }

                string realm = trashMem.ReadString(offsetList.StaticRealmName, Encoding.ASCII, 12);
                if (realm.Length == 0)
                {
                    realm = "";
                }

                wows.Add(new WowProcess(p, name, realm, isAlreadyHooked));
                trashMem.Detach();
            }

            return wows;
        }

        public static string BigValueToString(double value)
        {
            if (value >= 100000000)
            {
                return $"{(int)value / 1000000}M";
            }
            else if (value >= 100000)
            {
                return $"{(int)value / 1000}K";
            }

            return $"{value}";
        }
    }
}
