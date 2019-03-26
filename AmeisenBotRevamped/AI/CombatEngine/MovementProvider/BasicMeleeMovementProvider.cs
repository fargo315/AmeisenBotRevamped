using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;

namespace AmeisenBotRevamped.AI.CombatEngine.MovementProvider
{
    public class BasicMeleeMovementProvider : IMovementProvider
    {
        public WowPosition GetPositionToMoveTo(WowPosition currentPosition, WowPosition targetPosition)
        {
            return targetPosition;
        }

        public WowPosition GetPositionToMoveTo(WowPosition currentPosition, WowPosition targetPosition, List<WowUnit> unitsToStayAwayFrom)
        {
            return targetPosition;
        }
    }
}
