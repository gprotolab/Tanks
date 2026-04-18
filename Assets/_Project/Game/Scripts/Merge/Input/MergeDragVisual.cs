using UnityEngine;
using DG.Tweening;

namespace Game.Merge
{
    public class MergeDragVisual : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private Vector3 _liftOffset = new(0f, 1.5f, 0f);
        [SerializeField] private float _dragScale = 1.2f;
        [SerializeField] private float _liftDuration = 0.15f;
        [SerializeField] private float _returnDuration = 0.25f;
        [SerializeField] private float _mergeFlightDuration = 0.2f;
        [SerializeField] private Camera _gridCamera;

        private MergePartView _draggedPart;
        private Vector3 _originalWorldPos;
        private float _dragPlaneY;
        private bool _isActive;

        public void StartDrag(MergePartView partView, Vector3 originalWorldPos)
        {
            if (_isActive)
            {
                EndDrag();

                if (_draggedPart != null)
                    _draggedPart.KillAnimations();
            }

            _draggedPart = partView;
            _originalWorldPos = originalWorldPos;
            _dragPlaneY = originalWorldPos.y;
            _isActive = true;

            partView.KillAnimations();
            partView.transform.DOMove(originalWorldPos + _liftOffset, _liftDuration)
                .SetEase(Ease.OutCubic);
            partView.transform.DOScale(Vector3.one * _dragScale, _liftDuration)
                .SetEase(Ease.OutBack);
        }

        public void UpdatePosition(Vector2 screenPos)
        {
            if (!_isActive || _draggedPart == null || _gridCamera == null) return;

            var ray = _gridCamera.ScreenPointToRay(screenPos);
            var plane = new Plane(Vector3.up, new Vector3(0, _dragPlaneY, 0));

            if (plane.Raycast(ray, out float distance))
            {
                var worldPos = ray.GetPoint(distance) + _liftOffset;
                _draggedPart.transform.position = worldPos;
            }
        }

        public void ReturnToOrigin(System.Action onComplete = null)
        {
            if (!_isActive || _draggedPart == null)
            {
                onComplete?.Invoke();
                return;
            }

            var part = _draggedPart;
            var seq = DOTween.Sequence();
            seq.Append(part.transform.DOMove(_originalWorldPos, _returnDuration).SetEase(Ease.InOutCubic));
            seq.Join(part.transform.DOScale(Vector3.one, _returnDuration).SetEase(Ease.InOutCubic));
            seq.OnComplete(() =>
            {
                EndDrag();
                onComplete?.Invoke();
            });
        }

        public void FlyToTarget(Vector3 targetWorldPos, bool shrink, System.Action onComplete = null)
        {
            if (!_isActive || _draggedPart == null)
            {
                onComplete?.Invoke();
                return;
            }

            var part = _draggedPart;
            float targetScale = shrink ? 0.5f : 1f;

            var seq = DOTween.Sequence();
            seq.Append(part.transform.DOMove(targetWorldPos, _mergeFlightDuration).SetEase(Ease.InCubic));
            seq.Join(part.transform.DOScale(Vector3.one * targetScale, _mergeFlightDuration).SetEase(Ease.InCubic));
            seq.OnComplete(() =>
            {
                EndDrag();
                onComplete?.Invoke();
            });
        }

        public void EndDrag()
        {
            _isActive = false;
            _draggedPart = null;
        }
    }
}