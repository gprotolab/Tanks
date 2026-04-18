using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Battle
{
    public class ReloadBarView : MonoBehaviour
    {
        [SerializeField] private Image _smoothFillImage;
        [SerializeField] private Image _stepFillImage;
        [SerializeField] private RectTransform _dividersContainer;
        [SerializeField] private GameObject _dividerPrefab;

        [SerializeField] private TankWeapon _weapon;

        public void Init()
        {
            int capacity = _weapon.AmmoCapacity;

            _smoothFillImage.fillAmount = 1f;
            _stepFillImage.fillAmount = 1f;

            SpawnDividers(capacity);

            _weapon.NormalizedAmmo
                .Subscribe(n => _smoothFillImage.fillAmount = n)
                .AddTo(this);

            _weapon.CurrentAmmoInt
                .Subscribe(ammo => _stepFillImage.fillAmount = (float) ammo / capacity)
                .AddTo(this);
        }

        private void SpawnDividers(int capacity)
        {
            if (_dividerPrefab == null || capacity <= 1) return;

            for (int i = 1; i < capacity; i++)
            {
                var divider = Instantiate(_dividerPrefab, _dividersContainer);
                var rt = divider.GetComponent<RectTransform>();

                float normalizedPos = (float) i / capacity;
                rt.anchorMin = new Vector2(normalizedPos, 0f);
                rt.anchorMax = new Vector2(normalizedPos, 1f);
                rt.anchoredPosition = Vector2.zero;

                divider.SetActive(true);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_weapon == null) _weapon = GetComponentInParent<TankWeapon>();
        }
#endif
    }
}