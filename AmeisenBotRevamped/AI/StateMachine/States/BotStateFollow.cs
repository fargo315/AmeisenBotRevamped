using AmeisenBotRevamped.Clients.Structs;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using AmeisenBotRevamped.Utils;
using System;
using System.Collections.Generic;

namespace AmeisenBotRevamped.AI.StateMachine.States
{
    public class BotStateFollow : BotState
    {
        public WowUnit UnitToFollow { get; set; }

        private AmeisenBotStateMachine StateMachine { get; set; }
        private Queue<WowPosition> CurrentPath { get; set; }

        public WowPosition ActiveTargetPosition { get; private set; }
        private WowPosition LastPosition { get; set; }

        private float XOffset { get; set; }
        private float YOffset { get; set; }

        public BotStateFollow(AmeisenBotStateMachine stateMachine)
        {
            StateMachine = stateMachine;
            XOffset = 0;
            YOffset = 0;
        }

        public override void Execute()
        {
            if (StateMachine.IsMeInCombat || StateMachine.IsPartyInCombat())
            {
                StateMachine.SwitchState(typeof(BotStateCombat));
                return;
            }

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

            ActiveTargetPosition = CurrentPath.Peek();
            StateMachine.WowActionExecutor?.MoveToPosition(new WowPosition()
            {
                x = ActiveTargetPosition.x, //+ XOffset,
                y = ActiveTargetPosition.y, //+ YOffset,
                z = ActiveTargetPosition.z
            });

            // get position directly from memory
            WowPosition myPosition = StateMachine.WowDataAdapter.ActivePlayerPosition;
            if (BotMath.GetDistance(myPosition, CurrentPath.Peek()) < 6)
            {
                CurrentPath.Dequeue();
            }

            double distanceTraveled = BotMath.GetDistance(myPosition, LastPosition);
            if (distanceTraveled > 0 && distanceTraveled < 0.1)
            {
                float newX = (float)Math.Cos(myPosition.r + (Math.PI / 2)) + 4 + myPosition.x;
                float newY = (float)Math.Sin(myPosition.r + (Math.PI / 2)) + 4 + myPosition.y;
                StateMachine.WowActionExecutor?.MoveToPosition(new WowPosition() { x = newX, y = newY, z = myPosition.z });
            }
            else if (distanceTraveled > 0 && distanceTraveled < 0.2)
            {
                StateMachine.WowActionExecutor?.Jump();
            }

            LastPosition = myPosition;
        }

        private void UpdatePath()
        {
            WowPosition myPosition = StateMachine.WowDataAdapter.ActivePlayerPosition;
            Vector3 myPosAsVector = new Vector3(myPosition.x, myPosition.y, myPosition.z);
            Vector3 targetPosAsVector = new Vector3(UnitToFollow.Position.x, UnitToFollow.Position.y, UnitToFollow.Position.z);

            List<Vector3> waypoints = new List<Vector3> { targetPosAsVector };

            if (StateMachine.PathfindingClient != null)
            {
                waypoints = StateMachine.PathfindingClient.GetPath(myPosAsVector, targetPosAsVector, StateMachine.WowDataAdapter.MapId);
            }

            foreach (Vector3 pos in waypoints)
            {
                WowPosition wpos = new WowPosition(pos);
                if ((pos.X != myPosAsVector.X && pos.Y != myPosAsVector.Y && pos.Z != myPosAsVector.Z) && !CurrentPath.Contains(wpos))
                    CurrentPath.Enqueue(wpos);
            }

            if ((targetPosAsVector.X != myPosAsVector.X && targetPosAsVector.Y != myPosAsVector.Y && targetPosAsVector.Z != myPosAsVector.Z))
                CurrentPath.Enqueue(new WowPosition(targetPosAsVector));
        }

        public override void Exit()
        {
            UnitToFollow = null;
            CurrentPath = null;
        }

        public override void Start()
        {
            Random rnd = new Random();
            if (XOffset == 0) XOffset = (float)rnd.NextDouble() * 2;
            if (YOffset == 0) YOffset = (float)rnd.NextDouble() * 2;

            UnitToFollow = StateMachine.FindUnitToFollow();
            CurrentPath = new Queue<WowPosition>();
        }

        public override string ToString() => "Following";
    }
}
