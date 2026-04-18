using DG.Tweening;
using Game.Equipment;
using R3;
using UnityEngine;

namespace Game.Merge
{
    public class MergeTankView : MonoBehaviour
    {
        [Header("Tank Model")] [SerializeField]
        private Transform _tankRoot;

        [SerializeField] private Transform _turretMount;
        [SerializeField] private Transform _chassisMount;

        [Header("Rotation")] [SerializeField] private float _rotationSpeed = 20f;

        private TankPartSkinCatalogSO _catalog;
        private MergeModel _gridModel;
        private GameObject _currentTurretModel;
        private GameObject _currentChassisModel;
        private Tweener _rotationTween;

        public void Initialize(MergeModel gridModel, TankPartSkinCatalogSO catalog)
        {
            _gridModel = gridModel;
            _catalog = catalog;

            _gridModel.OnTankSlotChanged
                .Subscribe(t => UpdateSlotModel(t.type, t.part))
                .AddTo(this);

            UpdateSlotModel(TankPartType.Turret, _gridModel.EquippedTurret);
            UpdateSlotModel(TankPartType.Chassis, _gridModel.EquippedChassis);

            StartRotation();
        }

        private void StartRotation()
        {
            _rotationTween?.Kill();
            _rotationTween = _tankRoot
                .DORotate(new Vector3(0, 360, 0), 360f / _rotationSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }

        private void UpdateSlotModel(TankPartType type, TankPartData part)
        {
            var mount = type == TankPartType.Turret ? _turretMount : _chassisMount;

            var existing = type == TankPartType.Turret ? _currentTurretModel : _currentChassisModel;
            if (existing != null)
            {
                existing.transform.DOKill();
                Destroy(existing);
            }

            if (part == null || _catalog == null)
            {
                SetSlotModelRef(type, null);
                return;
            }

            var prefab = _catalog.GetPrefab(part.Type, part.Level);
            if (prefab == null)
            {
                SetSlotModelRef(type, null);
                return;
            }

            var newModel = Instantiate(prefab, mount);
            newModel.transform.localPosition = Vector3.zero;
            newModel.transform.localRotation = Quaternion.identity;

            // Bounce animation on equip
            newModel.transform.localScale = Vector3.one * 0.5f;
            newModel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            SetSlotModelRef(type, newModel);
        }

        private void SetSlotModelRef(TankPartType type, GameObject model)
        {
            if (type == TankPartType.Turret)
                _currentTurretModel = model;
            else
                _currentChassisModel = model;
        }

        private void OnDestroy()
        {
            _rotationTween?.Kill();
        }
    }
}