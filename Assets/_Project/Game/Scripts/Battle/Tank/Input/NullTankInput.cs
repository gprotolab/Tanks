using UnityEngine;

namespace Game.Battle
{
    public class NullTankInput : ITankInput
    {
        public Vector3 MoveDirection => Vector3.zero;
        public bool IsMoving => false;
    }
}