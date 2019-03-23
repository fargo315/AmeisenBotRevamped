using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.AI.StateMachine.States
{
    public abstract class BotState
    {
        public abstract void Execute();
        public abstract void Start();
        public abstract void Exit();
    }
}
