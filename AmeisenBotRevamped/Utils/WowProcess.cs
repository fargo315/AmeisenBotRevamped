using System.Diagnostics;

namespace AmeisenBotRevamped.Utils
{
    public class WowProcess
    {
        public Process Process { get; private set; }
        public string CharacterName { get; private set; }
        public string RealmName { get; private set; }
        public bool IsHooked { get; private set; }

        public WowProcess(Process process, string characterName, string realmName, bool isHooked)
        {
            Process = process;
            CharacterName = characterName;
            RealmName = realmName;
            IsHooked = isHooked;
        }
    }
}
