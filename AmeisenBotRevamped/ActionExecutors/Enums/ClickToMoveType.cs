using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotRevamped.ActionExecutors.Enums
{
    public enum ClickToMoveType
    {
        FaceTarget = 0x1,
        FaceDestination = 0x2,
        Stop = 0x3,
        Move = 0x4,
        Interact = 0x5,
        Loot = 0x6,
        InteractObject = 0x7,
        FaceOther = 0x8,
        Skin = 0x9,
        AttackPos = 0xA,
        AttackGuid = 0xB,
        Attack = 0x10,
        WalkAndRotate = 0x13
    }
}
