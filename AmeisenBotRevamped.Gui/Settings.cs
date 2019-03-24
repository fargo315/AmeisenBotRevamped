using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.Gui
{
    public class Settings
    {
        public string WowExecutableFilePath { get; set; }
        public string BotListFilePath { get; set; }

        public string AmeisenNavmeshServerIp { get; set; }
        public int AmeisenNavmeshServerPort { get; set; }

        public Settings()
        {
            WowExecutableFilePath = "";
            BotListFilePath = "";

            AmeisenNavmeshServerIp = "127.0.0.1";
            AmeisenNavmeshServerPort = 47110;
        }
    }
}
