using AmeisenBotRevamped.ActionExecutors;
using AmeisenBotRevamped.AI.CombatEngine.MovementProvider;
using AmeisenBotRevamped.AI.CombatEngine.SpellStrategies;
using AmeisenBotRevamped.AI.StateMachine.States;
using AmeisenBotRevamped.Clients;
using AmeisenBotRevamped.DataAdapters;
using AmeisenBotRevamped.Logging;
using AmeisenBotRevamped.ObjectManager;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using AmeisenBotRevamped.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Timers;

namespace AmeisenBotRevamped.AI.StateMachine
{
    public class AmeisenBotStateMachine
    {
        public double UnitFollowThreshold => 5.0;
        public double UnitFollowThresholdMax => 50.0;
        public double UnitFollowThresholdLeaderMax => 120.0;

        public BotState CurrentState { get; private set; }
        private Dictionary<Type, BotState> BotStates { get; }

        internal IWowActionExecutor WowActionExecutor { get; set; }
        internal IWowDataAdapter WowDataAdapter { get; set; }
        internal WowObjectManager ObjectManager => WowDataAdapter.ObjectManager;
        internal IPathfindingClient PathfindingClient { get; set; }

        public IMovementProvider MovementProvider { get; set; }
        public ISpellStrategy SpellStrategy { get; set; }

        private Timer StateMachineTimer { get; }
        public void Start() => StateMachineTimer.Start();
        public void Stop() => StateMachineTimer.Stop();
        public bool Enabled => StateMachineTimer.Enabled;

        public AmeisenBotStateMachine(IWowDataAdapter dataAdapter, IWowActionExecutor wowActionExecutor, IPathfindingClient pathfindingClient, IMovementProvider movementProvider, ISpellStrategy spellStrategy, int stateUpdateInterval = 100)
        {
            MovementProvider = movementProvider;
            SpellStrategy = spellStrategy;

            BotStates = new Dictionary<Type, BotState> {
                {typeof(BotStateIdle), new BotStateIdle(this) },
                {typeof(BotStateFollow), new BotStateFollow(this) },
                {typeof(BotStateCombat), new BotStateCombat(this) },
                {typeof(BotStateDead), new BotStateDead(this) },
                {typeof(BotStateGhost), new BotStateGhost(this) }
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

        private void CStateMachineUpdate(object sender, ElapsedEventArgs e)
        {
            try
            {
                CurrentState.Execute();
                WowActionExecutor.AntiAfk();
            }
            catch (Exception ex)
            {
                AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X" , CultureInfo.InvariantCulture.NumberFormat)}]\tCrash at StateMachine: \n{ex}");
            }
        }

        public void SwitchState(Type newType)
        {
            AmeisenBotLogger.Instance.Log($"[{WowActionExecutor?.ProcessId.ToString("X" , CultureInfo.InvariantCulture.NumberFormat)}]\tSwitching state from \"{CurrentState}\" => \"{BotStates[newType]}\"");
            CurrentState.Exit();
            CurrentState = BotStates[newType];
            CurrentState.Start();
        }

        public bool IsMeSupposedToFollow(WowUnit unitToFollow)
        {
            if (unitToFollow == null)
            {
                return false;
            }

            return IsUnitInFollowRange(unitToFollow);
        }

        public bool IsUnitInFollowRange(WowUnit unitToFollow)
        {
            WowUnit wowPlayer = (WowUnit)ObjectManager.GetWowObjectByGuid(WowDataAdapter.PlayerGuid);

            if (wowPlayer != null && unitToFollow != null)
            {
                WowPosition myPosition = wowPlayer.Position;
                return BotMath.GetDistance(myPosition, unitToFollow.Position) >= UnitFollowThreshold;
            }
            else
            {
                return false;
            }
        }

        public bool IsUnitInFollowMaxRange(WowUnit unitToFollow)
        {
            WowUnit wowPlayer = (WowUnit)ObjectManager.GetWowObjectByGuid(WowDataAdapter.PlayerGuid);

            if (wowPlayer != null && unitToFollow != null)
            {
                WowPosition myPosition = wowPlayer.Position;
                if (unitToFollow.Guid == WowDataAdapter.PartyleaderGuid)
                {
                    return BotMath.GetDistance(myPosition, unitToFollow.Position) <= UnitFollowThresholdLeaderMax;
                }
                else
                {
                    return BotMath.GetDistance(myPosition, unitToFollow.Position) <= UnitFollowThresholdMax;
                }
            }
            else
            {
                return false;
            }
        }

        public WowUnit FindUnitToFollow()
        {
            WowUnit wowUnit = (WowUnit)ObjectManager.GetWowObjectByGuid(WowDataAdapter.PartyleaderGuid);
            if (wowUnit != null && (IsUnitInFollowMaxRange(wowUnit) || IsUnitInFollowRange(wowUnit)))
            {
                return wowUnit;
            }

            foreach (ulong guid in WowDataAdapter.PartymemberGuids)
            {
                wowUnit = (WowUnit)ObjectManager.GetWowObjectByGuid(guid);
                if (wowUnit != null && IsUnitInFollowMaxRange(wowUnit) && IsUnitInFollowRange(wowUnit))
                {
                    return wowUnit;
                }
            }
            return null;
        }

        public bool IsMeInCombat()
        {
            WowPlayer player = ((WowPlayer)ObjectManager.GetWowObjectByGuid(WowDataAdapter.PlayerGuid));
            if (player != null)
                return player.IsInCombat;
            return false;
        }

        public bool IsPartyInCombat()
        {
            foreach (WowUnit unit in ObjectManager.GetWowUnits())
            {
                if (WowDataAdapter.PartymemberGuids.Contains(unit.Guid) && unit.IsInCombat)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
