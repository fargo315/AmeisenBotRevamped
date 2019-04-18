using AmeisenBotRevamped.ActionExecutors.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.Utils
{
    public sealed class SharedCacheManager
    {
        private static readonly object padlock = new object();

        private static SharedCacheManager instance;

        public static SharedCacheManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new SharedCacheManager();
                    return instance;
                }
            }
        }

        public Dictionary<ulong, string> PlayerNameCache { get; }
        public Dictionary<ulong, string> UnitNameCache { get; }
        public Dictionary<(int, int), UnitReaction> ReactionCache { get; }

        private SharedCacheManager()
        {
            PlayerNameCache = new Dictionary<ulong, string>();
            UnitNameCache = new Dictionary<ulong, string>();
            ReactionCache = new Dictionary<(int, int), UnitReaction>();
        }
    }
}
