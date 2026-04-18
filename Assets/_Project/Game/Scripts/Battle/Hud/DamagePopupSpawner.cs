using System;
using DG.Tweening;
using MessagePipe;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace Game.Battle
{
    public class DamagePopupSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _popupPrefab;
        [SerializeField] private int _warmUpCount = 10;

        private static readonly Color ColorPlayerHit = Color.yellow;
        private static readonly Color ColorEnemyHit = Color.red;

        private ISubscriber<TankDamagedSignal> _subscriber;

        private IObjectPool<TextMeshPro> _pool;

        [Inject]
        public void Construct(ISubscriber<TankDamagedSignal> subscriber)
        {
            _subscriber = subscriber;
        }

        private void Awake()
        {
            _pool = new ObjectPool<TextMeshPro>(
                createFunc: () => Instantiate(_popupPrefab, transform).GetComponent<TextMeshPro>(),
                actionOnGet: item => item.gameObject.SetActive(true),
                actionOnRelease: item => item.gameObject.SetActive(false),
                actionOnDestroy: item => Destroy(item.gameObject),
                collectionCheck: true,
                defaultCapacity: _warmUpCount);
        }

        private void Start()
        {
            _subscriber.Subscribe(OnTankDamaged).AddTo(this);
        }

        private void OnDestroy()
        {
            (_pool as System.IDisposable)?.Dispose();
        }

        private void OnTankDamaged(TankDamagedSignal signal)
        {
            var popup = _pool.Get();

            var offset = new Vector3(
                UnityEngine.Random.Range(-0.3f, 0.3f),
                1f,
                UnityEngine.Random.Range(-0.3f, 0.3f));

            popup.transform.position = signal.HitPoint + offset;
            popup.transform.localScale = Vector3.zero;

            popup.text = Mathf.RoundToInt(signal.Damage).ToString();
            popup.color = signal.IsTargetPlayer ? ColorPlayerHit : ColorEnemyHit;

            PlayAnimation(popup);
        }

        private void PlayAnimation(TextMeshPro popup)
        {
            var seq = DOTween.Sequence();

            Color startColor = popup.color;
            Color transparentColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            // Quick pop-in to make the number readable immediately.
            seq.Append(popup.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));

            // Then move up to keep battlefield visibility.
            seq.Append(popup.transform.DOMoveY(popup.transform.position.y + 1f, 0.6f).SetEase(Ease.OutQuad));

            // Fade out during the last part of the upward move.
            seq.Join(
                DOTween
                    .To(() => popup.color, value => popup.color = value, transparentColor, 0.3f)
                    .SetDelay(0.3f));

            seq.OnComplete(() => _pool.Release(popup));
        }
    }
}