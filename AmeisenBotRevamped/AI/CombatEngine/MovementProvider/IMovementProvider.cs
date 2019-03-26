using AmeisenBotRevamped.ObjectManager.WowObjects;
using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.AI.CombatEngine.MovementProvider
{
    public interface IMovementProvider
    {
        WowPosition GetPositionToMoveTo(WowPosition currentPosition, WowPosition targetPosition);
        WowPosition GetPositionToMoveTo(WowPosition currentPosition, WowPosition targetPosition, List<WowUnit> unitsToStayAwayFrom);
    }
}
