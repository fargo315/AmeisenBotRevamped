using System.Runtime.InteropServices;

namespace AmeisenBotRevamped.ActionExecutors.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public override bool Equals(object obj)
        {
            return obj is Rect rect
                && rect.Left == Left
                && rect.Top == Top
                && rect.Right == Right
                && rect.Bottom == Bottom;
        }

        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rect left, Rect right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return unchecked((Left.GetHashCode() * 23 * 23 * 23)
                    + (Top.GetHashCode() * 23 * 23)
                    + (Right.GetHashCode() * 23)
                    + Bottom.GetHashCode());
        }
    }
}
