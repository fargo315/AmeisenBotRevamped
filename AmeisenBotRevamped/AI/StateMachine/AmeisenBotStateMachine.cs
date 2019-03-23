using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.AI.StateMachine.States;
using AmeisenBotRevamped.Clients;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.ObjectManager;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using AmeisenBotRevamped.Utils;
using System;
using System.Collections.Generic;
using System.Timers;

namespace AmeisenBotRevamped.AI.StateMachine
{
    public class AmeisenBotStateMachine
    {
        public double UnitFollowThreshold => 5.0;
        public double UnitFollowThresholdMax => 50.0;
        public double UnitFollowThresholdLeaderMax => 120.0;

        public BotState CurrentState { get; private set; }

        private Timer StateMachineTimer { get; set; }
        private Dictionary<Type, BotState> BotStates { get; set; }

        internal IWowActionExecutor WowActionExecutor { get; set; }
        internal IWowDataAdapter WowDataAdapter { get; set; }
        internal WowObjectManager ObjectManager => WowDataAdapter.ObjectManager;
        internal IPathfindingClient PathfindingClient { get; set; }

        public AmeisenBotStateMachine(IWowDataAdapter dataAdapter, IWowActionExecutor wowActionExecutor, IPathfindingClient pathfindingClient, int stateUpdateInterval = 250)
        {
            BotStates = new Dictionary<Type, BotState> {
                {typeof(BotStateIdle), new BotStateIdle(this) },
                {typeof(BotStateFollow), new BotStateFollow(this) }
            };

            PathfindingClient = pathfindingClient;
            WowActionExecutor = wowActionExecutor;
            WowDataAdapter = dataAdapter;

            CurrentState = BotStates[typeof(BotStateIdle)];
            CurrentState.Start();

            StateMachineTimer = new Timer(stateUpdateInterval);
            StateMachineTimer.Elapsed += CStateMachineUpdate;
        }

        ~AmeisenBotStateMachine()
        {
            CurrentState.Exit();
            StateMachineTimer.Stop();
            StateMachineTimer.Dispose();
        }

        public void Start() => StateMachineTimer.Start();
        public void Stop() => StateMachineTimer.Stop();
        public bool Enabled => StateMachineTimer.Enabled;

        private void CStateMachineUpdate(object sender, ElapsedEventArgs e)
        {
            CurrentState.Execute();
            WowActionExecutor.AntiAfk();
        }

        public void SwitchState(Type newType)
        {
            CurrentState.Exit();
            CurrentState = BotStates[newType];
            CurrentState.Start();
        }

        public bool IsMeSupposedToFollow(WowUnit unitToFollow)
        {
            if (unitToFollow == null) return false;
            return IsUnitInFollowRange(unitToFollow);
        }

        public bool IsUnitInFollowRange(WowUnit unitToFollow)
        {
            WowUnit wowPlayer = (WowUnit)ObjectManager.GetWowObjectByGuid(WowDataAdapter.PlayerGuid);
            WowPosition myPosition = wowPlayer.Position;

            if (wowPlayer == null || unitToFollow == null) return false;

            double distance = BotMath.GetDistance(myPosition, unitToFollow.Position);

            if (unitToFollow.Guid == WowDataAdapter.PartyLeaderGuid)
            {
                return distance >= UnitFollowThreshold
                    && distance <= UnitFollowThresholdLeaderMax;
            }
            else
            {
                return distance >= UnitFollowThreshold
                    && distance <= UnitFollowThresholdMax;
            }
        }

        public WowUnit FindUnitToFollow()
        {
            foreach (ulong guid in WowDataAdapter.PartymemberGuids)
            {
                WowUnit wowUnit = (WowUnit)ObjectManager.GetWowObjectByGuid(guid);

                if (wowUnit != null && IsUnitInFollowRange(wowUnit)) return wowUnit;
            }
            return null;
        }
    }
}
