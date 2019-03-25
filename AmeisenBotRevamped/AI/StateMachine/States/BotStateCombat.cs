using AmeisenBotRevamped.AI.CombatEngine;

namespace AmeisenBotRevamped.AI.StateMachine.States
{
    public class BotStateCombat : BotState
    {
        private AmeisenBotStateMachine StateMachine { get; set; }
        private ICombatEngine CombatEngine { get; set; }

        public BotStateCombat(AmeisenBotStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public override void Execute()
        {
            if (!StateMachine.IsMeInCombat && !StateMachine.IsPartyInCombat())
            {
                StateMachine.SwitchState(typeof(BotStateIdle));
                return;
            }

            CombatEngine.Execute();
        }

        public override void Exit()
        {
            CombatEngine.Exit();
        }

        public override void Start()
        {
            CombatEngine.Start();
            CombatEngine = new BasicCombatEngine(StateMachine.WowDataAdapter, StateMachine.WowActionExecutor);
        }
    }
}
