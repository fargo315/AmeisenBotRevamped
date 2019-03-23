using AmeisenBotRevamped.ObjectManager.WowObjects.Structs;
using System;

namespace AmeisenBotRevamped.Utils
{
    public static class BotMath
    {
        public static float GetFacingAngle(WowPosition position, WowPosition targetPosition)
        {
            float angle = (float)Math.Atan2(targetPosition.y - position.y, targetPosition.x - position.x);

            if (angle < 0.0f)
            {
                angle = angle + (float)Math.PI * 2.0f;
            }
            else if (angle > (float)Math.PI * 2)
            {
                angle = angle - (float)Math.PI * 2.0f;
            }

            return angle;
        }

        public static bool IsFacing(WowPosition position, WowPosition targetPosition, double minRotation = 0.7, double maxRotation = 1.3)
        {
            float f = GetFacingAngle(position, targetPosition);
            return (f >= (position.r * minRotation)) && (f <= (position.r * maxRotation)) ? true : false;
        }

        public static double GetDistance(WowPosition a, WowPosition b)
             => Math.Sqrt((a.x - b.x) * (a.x - b.x) +
                          (a.y - b.y) * (a.y - b.y) +
                          (a.z - b.z) * (a.z - b.z));
    }
}
