using UnityEngine;

namespace Game.Common
{
    public static class Layers
    {
        public static readonly int Tank = LayerMask.NameToLayer("Tank");
        public static readonly int Wall = LayerMask.NameToLayer("Wall");
        public static readonly int Floor = LayerMask.NameToLayer("Floor");

        public static readonly LayerMask TankMask = LayerMask.GetMask("Tank");
        public static readonly LayerMask WallMask = LayerMask.GetMask("Wall");
    }
}