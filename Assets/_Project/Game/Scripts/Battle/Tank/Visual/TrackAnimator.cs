using System.Collections.Generic;
using UnityEngine;

namespace Game.Battle
{
    public class TrackAnimator : MonoBehaviour
    {
        private static readonly int BASE_MAP = Shader.PropertyToID("_BaseMap");

        [SerializeField] private List<Renderer> _trackRenderers = new();
        [SerializeField] private Vector2 _scrollAxis = Vector2.up;
        [SerializeField, Range(0.01f, 10f)] private float _scrollSpeed = 1f;
        [SerializeField] private int _materialIndex;

        private Rigidbody _rb;
        private Material[] _instanceMaterials;
        private float _offset;

        private void Start()
        {
            _rb = GetComponentInParent<Rigidbody>();

            if (_rb == null)
            {
                Destroy(this);
                return;
            }

            _instanceMaterials = new Material[_trackRenderers.Count];

            for (int i = 0; i < _trackRenderers.Count; i++)
            {
                var r = _trackRenderers[i];
                if (r == null) continue;

                var mats = r.sharedMaterials;
                var instance = new Material(mats[_materialIndex]);
                mats[_materialIndex] = instance;
                r.sharedMaterials = mats;
                _instanceMaterials[i] = instance;
            }
        }

        private void Update()
        {
            if (_rb == null || _instanceMaterials == null) return;

            float speed = _rb.linearVelocity.magnitude;
            if (speed < 0.01f) return;

            _offset += speed * _scrollSpeed * Time.deltaTime;
            if (_offset >= 1f) _offset -= 1f;

            var uv = _scrollAxis.normalized * _offset;
            for (int i = 0; i < _instanceMaterials.Length; i++)
                _instanceMaterials[i]?.SetTextureOffset(BASE_MAP, uv);
        }

        private void OnDestroy()
        {
            if (_instanceMaterials == null) return;
            foreach (var m in _instanceMaterials)
                if (m != null)
                    Destroy(m);
        }
    }
}