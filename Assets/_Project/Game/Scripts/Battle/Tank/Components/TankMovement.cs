using UnityEngine;

namespace Game.Battle
{
    public class TankMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rb;
        private float _moveSpeed;
        private float _rotationSpeed;
        private ITankInput _input;


        public bool IsMoving => _input != null && _input.IsMoving;

        public void Init(float moveSpeed, float rotationSpeed)
        {
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;
        }

        public void SetInput(ITankInput input) => _input = input;

        private void FixedUpdate()
        {
            if (_input == null || _rb == null) return;

            var dir = _input.MoveDirection;

            if (dir.sqrMagnitude < 0.01f)
            {
                _rb.linearVelocity = Vector3.zero;
                return;
            }

            var targetRotation = Quaternion.LookRotation(dir, Vector3.up);
            _rb.rotation = Quaternion.RotateTowards(
                _rb.rotation,
                targetRotation,
                _rotationSpeed * Time.fixedDeltaTime);

            _rb.linearVelocity = transform.forward * (_moveSpeed * dir.magnitude);
        }
    }
}