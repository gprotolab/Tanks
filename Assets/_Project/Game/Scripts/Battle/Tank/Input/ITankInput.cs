using UnityEngine;

namespace Game.Battle
{
    public interface ITankInput
    {
        Vector3 MoveDirection { get; }

        bool IsMoving { get; }
    }
}