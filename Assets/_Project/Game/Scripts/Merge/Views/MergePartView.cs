using UnityEngine;
using DG.Tweening;
using Game.Equipment;

namespace Game.Merge
{
    public class MergePartView : MonoBehaviour
    {
        [SerializeField] private Transform _modelPivot;
        [SerializeField] private PartLevelLabel _levelLabel;

        private GameObject _modelInstance;

        public TankPartData Part { get; private set; }

        public void Setup(TankPartData part, TankPartSkinCatalogSO catalog, bool animate = false)
        {
            Part = part;
            ClearModel();

            var prefab = catalog.GetPrefab(part.Type, part.Level);
            if (prefab == null) return;

            _modelInstance = Instantiate(prefab, _modelPivot);
            _modelInstance.transform.localPosition = Vector3.zero;
            _modelInstance.transform.localRotation = Quaternion.identity;

            if (_levelLabel != null)
                _levelLabel.SetLevel(part.Level);

            if (animate)
                AnimateAppear();
        }

        public void ClearModel()
        {
            if (_modelInstance != null)
            {
                Destroy(_modelInstance);
                _modelInstance = null;
            }

            if (_levelLabel != null)
                _levelLabel.SetLevel(0);
        }

        public void AnimateAppear()
        {
            _modelPivot.localScale = Vector3.zero;
            _modelPivot.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBounce);
        }

        public void KillAnimations()
        {
            if (_modelPivot != null) _modelPivot.DOKill();
            transform.DOKill();
        }

        private void OnDestroy()
        {
            KillAnimations();
        }
    }
}