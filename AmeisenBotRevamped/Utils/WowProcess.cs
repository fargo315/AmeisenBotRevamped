using System.Diagnostics;

namespace AmeisenBotRevamped.Utils
{
    public class WowProcess
    {
        public Process Process { get; }
        public string CharacterName { get; }
        public string RealmName { get; }
        public bool IsHooked { get; }
        public bool LoginInProgress { get; set; }

        public WowProcess(Process process, string characterName, string realmName, bool isHooked)
        {
            Process = process;
            CharacterName = characterName;
            RealmName = realmName;
            IsHooked = isHooked;
            LoginInProgress = false;
        }
    }
}
