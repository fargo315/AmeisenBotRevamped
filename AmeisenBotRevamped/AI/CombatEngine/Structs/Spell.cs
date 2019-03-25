using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.AI.CombatEngine.Structs
{
    public struct Spell
    {
        public int castTime;
        public int costs;
        public int maxRange;
        public int minRange;
        public string name;
        public string rank;
        public int spellbookId;
        public string spellBookName;
    }
}
