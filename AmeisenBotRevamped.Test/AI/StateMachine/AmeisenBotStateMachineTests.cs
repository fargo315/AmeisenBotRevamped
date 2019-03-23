﻿using AmeisenBotRevamped.AI.StateMachine.States;
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
            AmeisenBot ameisenBot = new AmeisenBot(new TestWowDataAdapter(), null, null, null);
            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(ameisenBot.WowDataAdapter, null, null);
            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);

            stateMachine.SwitchState(typeof(BotStateFollow));
            Assert.IsTrue(stateMachine.CurrentState is BotStateFollow);
        }

        [TestMethod()]
        public void AmeisenBotStateMachineTransitionIdleToFollowTest()
        {
            AmeisenBot ameisenBot = new AmeisenBot(new TestWowDataAdapter(), null, null, null);
            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(ameisenBot.WowDataAdapter, null, null);

            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);
            stateMachine.CurrentState.Execute();
            Assert.IsTrue(stateMachine.CurrentState is BotStateFollow);
            stateMachine.CurrentState.Execute();
            ((BotStateFollow)stateMachine.CurrentState).UnitToFollow = null;
            stateMachine.CurrentState.Execute();
            Assert.IsTrue(stateMachine.CurrentState is BotStateIdle);
        }

        [TestMethod()]
        public void IsMeSupposedToFollowTest()
        {
            AmeisenBot ameisenBot = new AmeisenBot(new TestWowDataAdapter(), null, null, null);
            AmeisenBotStateMachine stateMachine = new AmeisenBotStateMachine(ameisenBot.WowDataAdapter, null, null);

            WowUnit unitToFollow = stateMachine.FindUnitToFollow();
            Assert.IsTrue(stateMachine.IsMeSupposedToFollow(unitToFollow));
        }
    }
}