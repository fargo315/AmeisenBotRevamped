using AmeisenBotRevamped.OffsetLists;
using Magic;
using System.Collections.Generic;
using System.Diagnostics;

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
                BlackMagic blackmagic = new BlackMagic(p.Id);
                uint pDevice = blackmagic.ReadUInt(offsetList.StaticEndSceneDevice);
                uint pEnd = blackmagic.ReadUInt(pDevice + offsetList.EndSceneOffsetDevice);
                uint pScene = blackmagic.ReadUInt(pEnd);
                uint endscene = blackmagic.ReadUInt(pScene + offsetList.EndSceneOffset);

                bool isAlreadyHooked = false;
                try
                {
                    isAlreadyHooked = blackmagic.ReadByte(endscene + 0x2) == 0xE9;
                }
                catch { }

                string name = blackmagic.ReadASCIIString(offsetList.StaticPlayerName, 12);
                if (name == "")
                {
                    name = "not logged in";
                }

                string realm = blackmagic.ReadASCIIString(offsetList.StaticRealmName, 12);
                if (realm == "")
                {
                    if (name == "not logged in")
                    {
                        realm = "";
                    }
                    else
                    {
                        realm = "not logged in";
                    }
                }

                wows.Add(new WowProcess(p, name, realm, isAlreadyHooked));
                blackmagic.Close();
            }

            return wows;
        }
    }
}
