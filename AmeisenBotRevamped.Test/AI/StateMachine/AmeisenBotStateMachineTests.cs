using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.AI.CombatEngine.MovementProvider;
using AmeisenBotRevamped.AI.CombatEngine.SpellStrategies;
using AmeisenBotRevamped.AI.StateMachine.States;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmeisenBotRevamped.AI.StateMachine.Tests
{
    [TestClass()]
    public class AmeisenBotStateMachineTests
    {
        [TestMethod()]
        public void AmeisenBotStateMachineTest()
        {
            IWowDataAdapter wowDataAdapter = new TestWowDataAdapter();
            IWowActionExecutor wowActionExecutor = new TestWowActionExecutor();

            AmeisenBot ameisenBot = new AmeisenBot(null, wowDataAdapter, null, null);
            ameisenBot.Attach(wowActionExecutor, null, null, new BasicMeleeMovementProvider(), null);

            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(ameisenBot.WowDataAdapter, ameisenBot.WowActionExecutor, null, null, null);
            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);

            stateMachine.SwitchState(typeof(BotStateFollow));
            Assert.IsTrue(stateMachine.CurrentState is BotStateFollow);

            Assert.AreEqual("You suck", wowDataAdapter.LastErrorMessage);

            wowDataAdapter.StartObjectUpdates();
            wowDataAdapter.StopObjectUpdates();
        }

        [TestMethod()]
        public void AmeisenBotStateMachineTransitionIdleToFollowTest()
        {
            TestWowDataAdapter wowDataAdapter = new TestWowDataAdapter();

            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(
                wowDataAdapter,
                new TestWowActionExecutor(),
                null,
                null,
                null
            );

            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);
            stateMachine.CurrentState.Execute();
            Assert.IsTrue(stateMachine.CurrentState is BotStateFollow);
            stateMachine.CurrentState.Execute();
            ((BotStateFollow)stateMachine.CurrentState).UnitToFollow = null;
            stateMachine.CurrentState.Execute();
            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);
        }

        [TestMethod()]
        public void AmeisenBotStateMachineCombatEngineTest()
        {
            TestWowDataAdapter wowDataAdapter = new TestWowDataAdapter();

            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(
                wowDataAdapter,
                new TestWowActionExecutor(),
                null,
                new BasicMeleeMovementProvider(),
                new TestSpellStrategy()
            );
            
            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);

            wowDataAdapter.SetMeInCombat(true);
            stateMachine.CurrentState.Execute();
            Assert.IsTrue(stateMachine.CurrentState is BotStateCombat);

            stateMachine.CurrentState.Execute();
            stateMachine.CurrentState.Execute();
            stateMachine.CurrentState.Execute();

            Assert.IsTrue(stateMachine.CurrentState is BotStateCombat);

            wowDataAdapter.SetMeInCombat(false);
            stateMachine.CurrentState.Execute();
            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);
        }

        [TestMethod()]
        public void IsMeInCombatTest()
        {
            TestWowDataAdapter wowDataAdapter = new TestWowDataAdapter();

            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(
                wowDataAdapter,
                new TestWowActionExecutor(),
                null,
                new BasicMeleeMovementProvider(),
                new TestSpellStrategy()
            );

            Assert.IsFalse(stateMachine.IsMeInCombat());
            wowDataAdapter.SetMeInCombat(true);
            Assert.IsTrue(stateMachine.IsMeInCombat());
            wowDataAdapter.SetMeInCombat(false);
            Assert.IsFalse(stateMachine.IsMeInCombat());
        }

        [TestMethod()]
        public void IsPartyInCombatTest()
        {
            TestWowDataAdapter wowDataAdapter = new TestWowDataAdapter();

            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(
                wowDataAdapter,
                new TestWowActionExecutor(),
                null,
                new BasicMeleeMovementProvider(),
                new TestSpellStrategy()
            );

            Assert.IsFalse(stateMachine.IsPartyInCombat());
            wowDataAdapter.SetMeInCombat(true);
            Assert.IsFalse(stateMachine.IsPartyInCombat());
            wowDataAdapter.SetMeInCombat(false);
            Assert.IsFalse(stateMachine.IsPartyInCombat());

            Assert.IsFalse(stateMachine.IsPartyInCombat());
            wowDataAdapter.SetPartyInCombat(true);
            Assert.IsTrue(stateMachine.IsPartyInCombat());
            wowDataAdapter.SetPartyInCombat(false);
            Assert.IsFalse(stateMachine.IsPartyInCombat());
        }

        [TestMethod()]
        public void IsMeSupposedToFollowTest()
        {
            AmeisenBot ameisenBot = new AmeisenBot(null, new TestWowDataAdapter(), null, null);
            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(ameisenBot.WowDataAdapter, null, null, null, null);

            WowUnit unitToFollow = stateMachine.FindUnitToFollow();
            Assert.IsTrue(stateMachine.IsMeSupposedToFollow(unitToFollow));
        }
    }
}