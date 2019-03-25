using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.AI.StateMachine.States
{
    public class BotStateGhost : BotState
    {
        AmeisenBotStateMachine StateMachine { get; set; }

        public BotStateGhost(AmeisenBotStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public override void Execute()
        {

        }

        public override void Exit()
        {

        }

        public override void Start()
        {

        }

        public override string ToString() => "Ghost";
    }
}
