using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Battle
{
    public class BattleJoystickView : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private RectTransform _knob;

        [Tooltip("Maximum knob offset radius in pixels (canvas space)")] [SerializeField]
        private float _maxRadius = 80f;

        public Vector3 Direction { get; private set; }

        public bool IsPressed { get; private set; }

        private RectTransform _rect;
        private Vector2 _anchorPosition;

        private void Awake()
        {
            _rect = transform as RectTransform;

            if (_root != null)
                _root.gameObject.SetActive(false);
        }

        public void Enable() => gameObject.SetActive(true);

        public void Disable()
        {
            Reset();
            gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rect, eventData.position, eventData.pressEventCamera, out _anchorPosition);

            if (_root != null)
            {
                _root.anchoredPosition = _anchorPosition;
                _root.gameObject.SetActive(true);
            }

            if (_knob != null)
                _knob.anchoredPosition = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rect, eventData.position, eventData.pressEventCamera, out var currentPos);

            var delta = currentPos - _anchorPosition;
            float magnitude = delta.magnitude;

            Vector2 dir2D;
            if (magnitude > _maxRadius)
            {
                // Clamp knob to the joystick radius while keeping full direction.
                dir2D = delta / magnitude;
                if (_knob != null) _knob.anchoredPosition = dir2D * _maxRadius;
            }
            else
            {
                // Inside radius: direction scales from 0 to 1.
                dir2D = delta / _maxRadius;
                if (_knob != null) _knob.anchoredPosition = delta;
            }

            // Map canvas XY input to world XZ movement.
            Direction = new Vector3(dir2D.x, 0f, dir2D.y);
        }

        public void OnPointerUp(PointerEventData eventData) => Reset();

        private void Reset()
        {
            IsPressed = false;
            Direction = Vector3.zero;

            if (_knob != null)
                _knob.anchoredPosition = Vector2.zero;

            if (_root != null)
                _root.gameObject.SetActive(false);
        }
    }
}