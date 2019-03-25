using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.AI.StateMachine.States
{
    public class BotStateDead : BotState
    {
        AmeisenBotStateMachine StateMachine { get; set; }

        public BotStateDead(AmeisenBotStateMachine stateMachine)
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
    }
}
