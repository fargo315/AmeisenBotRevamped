﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.Gui
{
    public class Settings
    {
        public string AmeisenNavmeshServerIp { get; set; }
        public int AmeisenNavmeshServerPort { get; set; }

        public string WowExePath { get; set; }
        public string BotFleetConfig { get; set; }
        public string BotPictureFolder { get; set; }

        public Dictionary<string, ActionExecutors.Structs.Rect> WowPositions { get; }

        public Settings()
        {
            AmeisenNavmeshServerIp = "127.0.0.1";
            AmeisenNavmeshServerPort = 47110;

            WowExePath = "";
            BotFleetConfig = "";
            BotPictureFolder = "";

            WowPositions = new Dictionary<string, ActionExecutors.Structs.Rect>();
        }
    }
}
