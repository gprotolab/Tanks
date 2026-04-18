using UnityEngine;

namespace ANut.Core.Utils
{
    public class MoveToTargetLerp : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField, Min(0f)] private float _lerpSpeed = 5f;
        [SerializeField] private bool _useLocalPosition;
        [SerializeField] private bool _setForceOnStart;

        private void Start()
        {
            if (_setForceOnStart)
            {
                if (_useLocalPosition)
                    transform.localPosition = _target.localPosition;
                else
                    transform.position = _target.position;
            }
        }

        private void Update()
        {
            if (_target == null)
            {
                return;
            }

            if (_useLocalPosition)
            {
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    _target.localPosition,
                    _lerpSpeed * Time.deltaTime);

                return;
            }

            transform.position = Vector3.Lerp(
                transform.position,
                _target.position,
                _lerpSpeed * Time.deltaTime);
        }
    }
}