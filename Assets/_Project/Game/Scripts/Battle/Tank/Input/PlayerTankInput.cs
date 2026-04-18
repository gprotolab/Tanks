using UnityEngine;

namespace Game.Battle
{
    public class PlayerTankInput : ITankInput
    {
        private readonly BattleJoystickView _joystickView;

        public PlayerTankInput(BattleJoystickView joystickView)
        {
            _joystickView = joystickView;
        }

        public Vector3 MoveDirection => _joystickView.Direction;
        public bool IsMoving => _joystickView.IsPressed;
    }
}