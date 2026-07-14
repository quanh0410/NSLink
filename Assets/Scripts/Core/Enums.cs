namespace PolarBond.Core
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum EntityType
    {
        Player,
        Magnet,
        Wall,
        TargetTile
    }

    public enum MagneticPolarity
    {
        None,
        North, // Red
        South  // Blue
    }

    public enum TargetType
    {
        Universal,
        NorthTarget,
        SouthTarget
    }

    public enum SpecialTileType
    {
        None,
        PolarityReverse
    }

    public static class DirectionExtensions
    {
        public static UnityEngine.Vector2Int ToVector2Int(this Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return UnityEngine.Vector2Int.up;
                case Direction.Down: return UnityEngine.Vector2Int.down;
                case Direction.Left: return UnityEngine.Vector2Int.left;
                case Direction.Right: return UnityEngine.Vector2Int.right;
                default: return UnityEngine.Vector2Int.zero;
            }
        }

        public static Direction GetOpposite(this Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return Direction.Down;
                case Direction.Down: return Direction.Up;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                default: return dir;
            }
        }
    }
}
