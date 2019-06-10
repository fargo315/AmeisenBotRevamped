using AmeisenBotRevamped.Autologin.Structs;
using AmeisenBotRevamped.Gui.BotManager.Enums;
using AmeisenBotRevamped.Utils;
using System.Diagnostics;
using System.Threading;

namespace AmeisenBotRevamped.Gui.BotManager.Objects
{
    public class ManagedAmeisenBot
    {
        public BotStartState StartState { get; }
        public Thread Thread { get; }
        public AmeisenBot AmeisenBot { get; }
        public WowAccount WowAccount { get; }
        public WowProcess WowProcess { get; private set; }

        public ManagedAmeisenBot(WowProcess wowProcess, WowAccount wowAccount, AmeisenBot ameisenBot)
        {
            WowProcess = wowProcess;
            WowAccount = wowAccount;
            AmeisenBot = ameisenBot;
            StartState = BotStartState.None;
        }

        public bool SetupWowProcess(WowProcess wowProcess)
        {
            WowProcess = wowProcess;
            return true;
        }
    }
}
