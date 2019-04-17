using AmeisenBotRevamped.Autologin.Structs;
using System.Threading;

namespace AmeisenBotRevamped.Gui.BotManager.Objects
{
    public class ManagedAmeisenBot
    {
        public Thread Thread { get; }
        public AmeisenBot AmeisenBot { get; }
        public WowAccount WowAccount { get; }

        public ManagedAmeisenBot(Thread thread, AmeisenBot ameisenBot, WowAccount wowAccount)
        {
            Thread = thread;
            AmeisenBot = ameisenBot;
            WowAccount = wowAccount;
        }
    }
}
