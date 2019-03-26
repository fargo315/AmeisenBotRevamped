using AmeisenBotRevamped.AI.StateMachine.States;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.Logging;
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
            AmeisenBot ameisenBot = new AmeisenBot(null, new TestWowDataAdapter(), null, null);
            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(ameisenBot.WowDataAdapter, null, null);
            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);

            stateMachine.SwitchState(typeof(BotStateFollow));
            Assert.IsTrue(stateMachine.CurrentState is BotStateFollow);
        }

        [TestMethod()]
        public void AmeisenBotStateMachineTransitionIdleToFollowTest()
        {
            TestWowDataAdapter testWowDataAdapter = new TestWowDataAdapter();
            AmeisenBot ameisenBot = new AmeisenBot(null, testWowDataAdapter, null, null);
            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(ameisenBot.WowDataAdapter, null, null);

            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);
            stateMachine.CurrentState.Execute();
            Assert.IsTrue(stateMachine.CurrentState is BotStateFollow);
            stateMachine.CurrentState.Execute();
            ((BotStateFollow)stateMachine.CurrentState).UnitToFollow = null;
            stateMachine.CurrentState.Execute();
            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);
            testWowDataAdapter.SetMeInCombat(true);
            stateMachine.CurrentState.Execute();
            Assert.IsTrue(stateMachine.CurrentState is BotStateCombat);
            stateMachine.CurrentState.Execute();
            testWowDataAdapter.SetMeInCombat(false);
            stateMachine.CurrentState.Execute();
            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);
        }

        [TestMethod()]
        public void IsMeSupposedToFollowTest()
        {
            AmeisenBot ameisenBot = new AmeisenBot(null, new TestWowDataAdapter(), null, null);
            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(ameisenBot.WowDataAdapter, null, null);

            WowUnit unitToFollow = stateMachine.FindUnitToFollow();
            Assert.IsTrue(stateMachine.IsMeSupposedToFollow(unitToFollow));
        }
    }
}