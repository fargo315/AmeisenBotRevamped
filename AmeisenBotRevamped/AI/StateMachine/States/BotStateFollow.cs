using AmeisenBotRevamped.Clients.Structs;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using AmeisenBotRevamped.Utils;
using System.Collections.Generic;

namespace AmeisenBotRevamped.AI.StateMachine.States
{
    public class BotStateFollow : BotState
    {
        public WowUnit UnitToFollow { get; set; }

        private AmeisenBotStateMachine StateMachine { get; set; }
        private Queue<WowPosition> CurrentPath { get; set; }
        private WowPosition LastPosition { get; set; }

        public BotStateFollow(AmeisenBotStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public override void Execute()
        {
            WowUnit player = ((WowUnit)StateMachine.ObjectManager.GetWowObjectByGuid(StateMachine.WowDataAdapter.PlayerGuid));

            if (UnitToFollow == null || UnitToFollow == player || !StateMachine.IsUnitInFollowRange(UnitToFollow))
            {
                StateMachine.SwitchState(typeof(BotStateIdle));
                return;
            }
            
            if (CurrentPath.Count == 0)
            {
                UpdatePath();
                if (CurrentPath.Count == 0)
                {
                    return;
                }
            }
            else
            {
                StateMachine.WowActionExecutor?.MoveToPosition(CurrentPath.Peek());

                WowPosition myPosition = player.Position;
                if (BotMath.GetDistance(myPosition, CurrentPath.Peek()) < 2.5)
                {
                    CurrentPath.Dequeue();
                }

                double distanceTraveled = BotMath.GetDistance(myPosition, LastPosition);
                if (distanceTraveled > 0 && distanceTraveled < 0.2)
                {
                    StateMachine.WowActionExecutor?.Jump();
                }

                LastPosition = myPosition;
            }
        }

        private void UpdatePath()
        {
            WowPosition myPosition = ((WowUnit)StateMachine.ObjectManager.GetWowObjectByGuid(StateMachine.WowDataAdapter.PlayerGuid)).Position;
            Vector3 myPos = new Vector3(myPosition.x, myPosition.y, myPosition.z);
            Vector3 targetPos = new Vector3(myPosition.x, myPosition.y, myPosition.z);

            List<Vector3> waypoints = new List<Vector3> { targetPos };

            if (StateMachine.PathfindingClient != null)
            {
                waypoints = StateMachine.PathfindingClient.GetPath(myPos, targetPos, StateMachine.WowDataAdapter.MapId);
            }

            foreach (Vector3 pos in waypoints)
            {
                CurrentPath.Enqueue(new WowPosition(pos));
            }
        }

        public override void Exit()
        {
            UnitToFollow = null;
            CurrentPath = null;
        }

        public override void Start()
        {
            UnitToFollow = StateMachine.FindUnitToFollow();
            CurrentPath = new Queue<WowPosition>();
        }

        public override string ToString() => "Following";
    }
}
