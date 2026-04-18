using DG.Tweening;
using TMPro;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Battle
{
    public class HealthBarView : MonoBehaviour
    {
        private static readonly Color ColorPlayer = new Color(0.2f, 0.9f, 0.2f);
        private static readonly Color ColorEnemy = new Color(0.9f, 0.2f, 0.2f);

        private const float AnimDuration = 0.15f;

        [SerializeField] private Image _fillImage;
        [SerializeField] private TMP_Text _hpLabel;

        [SerializeField] private TankHealth _health;

        private Tweener _activeTween;

        public void Init(bool isPlayer)
        {
            _fillImage.color = isPlayer ? ColorPlayer : ColorEnemy;

            // Sync immediately so UI is correct before first reactive update.
            if (_health.MaxHp > 0f)
                _fillImage.fillAmount = _health.CurrentHpValue / _health.MaxHp;

            SetHpLabel(_health.CurrentHpValue);

            _health.CurrentHp
                .Subscribe(hp => OnHealthChanged(hp, _health.MaxHp))
                .AddTo(this);
        }

        private void OnHealthChanged(float currentHp, float maxHp)
        {
            if (maxHp <= 0f) return;

            float targetFill = currentHp / maxHp;

            SetHpLabel(currentHp);

            _activeTween?.Kill();
            _activeTween = DOTween
                .To(() => _fillImage.fillAmount, value => _fillImage.fillAmount = value, targetFill, AnimDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);
        }

        private void SetHpLabel(float currentHp)
        {
            if (_hpLabel == null) return;
            _hpLabel.text = Mathf.CeilToInt(currentHp).ToString();
        }

        private void OnDestroy()
        {
            _activeTween?.Kill();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_health == null) _health = GetComponentInParent<TankHealth>();
        }
#endif
    }
}