using DG.Tweening;
using R3;
using UnityEngine;
using ANut.Core;

namespace Game.Battle
{
    public class TankDamageFlash : MonoBehaviour
    {
        [SerializeField] private Color _flashColor = new Color(1f, 0.25f, 0.05f);
        [SerializeField, Range(0.5f, 8f)] private float _flashPeak = 3f;
        [SerializeField, Range(0.05f, 1f)] private float _fadeDuration = 0.35f;

        [SerializeField] private TankHealth _health;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private Material[] _instanceMaterials;
        private Tweener _tween;
        private float _currentIntensity;

        public void Init(Transform searchRoot)
        {
            var targets = searchRoot.GetComponentsInChildren<FlashTarget>(includeInactive: true);

            if (targets.Length == 0)
            {
                Log.Warning(
                    "[TankDamageFlash] No FlashTarget markers found under '{0}'. Add FlashTarget to mesh GameObjects in the skin prefab.",
                    searchRoot.name);
                return;
            }

            var materials = new System.Collections.Generic.List<Material>(targets.Length);

            for (int i = 0; i < targets.Length; i++)
            {
                var renderer = targets[i].Renderer;
                if (renderer == null)
                {
                    Log.Warning("[TankDamageFlash] FlashTarget on '{0}' has no Renderer assigned — skipped.",
                        targets[i].gameObject.name);
                    continue;
                }

                // Create instance to avoid modifying the shared material asset
                var instance = new Material(renderer.sharedMaterial);
                instance.EnableKeyword("_EMISSION");
                renderer.sharedMaterial = instance;
                materials.Add(instance);
            }

            _instanceMaterials = materials.ToArray();

            _health.CurrentHp
                .Pairwise() // (previous, current) pair
                .Where(pair => pair.Current < pair.Previous) // damage only, not healing/respawn
                .Subscribe(_ => PlayFlash())
                .AddTo(this);
        }

        private void PlayFlash()
        {
            // Kill previous tween so rapid hits restart cleanly at peak intensity
            _tween?.Kill();

            _currentIntensity = _flashPeak;
            SetEmission(_currentIntensity);

            _tween = DOTween
                .To(getter: () => _currentIntensity,
                    setter: v =>
                    {
                        _currentIntensity = v;
                        SetEmission(v);
                    },
                    endValue: 0f,
                    duration: _fadeDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject); // auto-kill if GameObject is destroyed mid-tween
        }

        private void SetEmission(float intensity)
        {
            if (_instanceMaterials == null) return;

            var color = _flashColor * intensity;
            foreach (var mat in _instanceMaterials)
                mat?.SetColor(EmissionColorId, color);
        }

        private void OnDestroy()
        {
            _tween?.Kill();

            if (_instanceMaterials == null) return;
            foreach (var mat in _instanceMaterials)
                if (mat != null)
                    Destroy(mat);
        }
    }
}