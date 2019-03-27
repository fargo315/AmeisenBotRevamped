using AmeisenBotRevamped.AI.CombatEngine;
using AmeisenBotRevamped.AI.CombatEngine.MovementProvider;
using AmeisenBotRevamped.AI.CombatEngine.SpellStrategies;

namespace AmeisenBotRevamped.AI.StateMachine.States
{
    public class BotStateCombat : BotState
    {
        private AmeisenBotStateMachine StateMachine { get; set; }
        private ICombatEngine CombatEngine { get; set; }

        private IMovementProvider MovementProvider { get; set; }
        private ISpellStrategy SpellStrategy { get; set; }

        public BotStateCombat(AmeisenBotStateMachine stateMachine)
        {
            StateMachine = stateMachine;
            MovementProvider = StateMachine.MovementProvider;
            SpellStrategy = StateMachine.SpellStrategy;
        }

        public override void Execute()
        {
            if (!StateMachine.IsMeInCombat() && !StateMachine.IsPartyInCombat())
            {
                StateMachine.SwitchState(typeof(BotStateIdle));
                return;
            }

            CombatEngine?.Execute();
        }

        public override void Exit()
        {
            CombatEngine?.Exit();
        }

        public override void Start()
        {
            CombatEngine = new BasicCombatEngine(StateMachine.WowDataAdapter, StateMachine.WowActionExecutor, MovementProvider, SpellStrategy);
            CombatEngine?.Start();
        }

        public override string ToString() => "Combat";
    }
}
