using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.Autologin.Structs
{
    public struct WowAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int CharacterSlot { get; set; }
        public string CharacterName { get; set; }
    }
}
