using AmeisenBotRevamped.ObjectManager.WowObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.AI.StateMachine.States
{
    public class BotStateIdle : BotState
    {
        AmeisenBotStateMachine StateMachine { get; set; }

        public BotStateIdle(AmeisenBotStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public override void Execute()
        {
            WowUnit unitToFollow = StateMachine.FindUnitToFollow();
            if (StateMachine.IsMeSupposedToFollow(unitToFollow))
            {
                StateMachine.SwitchState(typeof(BotStateFollow));
            }
        }

        public override void Exit()
        {

        }

        public override void Start()
        {

        }

        public override string ToString() => "Idling";
    }
}
