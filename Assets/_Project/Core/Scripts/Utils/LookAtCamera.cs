using UnityEngine;

namespace ANut.Core.Utils
{
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private bool _updateEveryFrame = false;

        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void OnEnable()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            UpdateRotation();
        }

        private void LateUpdate()
        {
            if (_updateEveryFrame)
            {
                UpdateRotation();
            }
        }

        private void UpdateRotation()
        {
            if (_camera != null)
            {
                transform.rotation = _camera.transform.rotation;
            }
        }
    }
}