using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.AI.CombatEngine
{
    public interface ICombatEngine
    {
        void Execute();
        void Start();
        void Exit();
    }
}
