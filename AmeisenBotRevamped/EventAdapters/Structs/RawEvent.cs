using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.EventAdapters.Structs
{
    public struct RawEvent
    {
        public string @event;
        public List<string> args;
        public long time;
    }
}
