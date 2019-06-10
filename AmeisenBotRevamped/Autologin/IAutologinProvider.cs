using AmeisenBotRevamped.Autologin.Structs;
using AmeisenBotRevamped.OffsetLists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.Autologin
{
    public interface IAutologinProvider
    {
        bool LoginInProgress { get; }
        string LoginInProgressCharactername { get; }

        bool DoLogin(Process process, WowAccount wowAccount, IOffsetList offsetlist, int maxTries = 4);
    }
}
