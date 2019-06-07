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
        AmeisenBotStateMachine StateMachine { get; }

        public BotStateIdle(AmeisenBotStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public override void Execute()
        {
            if(StateMachine.IsMeInCombat() || StateMachine.IsPartyInCombat())
            {
                StateMachine.SwitchState(typeof(BotStateCombat));
                return;
            }

            if (StateMachine.IsMeSupposedToFollow(StateMachine.FindUnitToFollow()))
            {
                StateMachine.SwitchState(typeof(BotStateFollow));
            }
        }

        public override void Exit()
        {
            // TODO
        }

        public override void Start()
        {
            // TODO
        }

        public override string ToString() => "Idling";
    }
}
