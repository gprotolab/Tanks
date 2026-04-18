using UnityEngine;
using UnityEngine.Pool;

namespace Game.Idle
{
    public class CoinAnimationSpawner : MonoBehaviour
    {
        [Header("Setup")] [SerializeField] private RectTransform _spawnArea;
        [SerializeField] private CoinAnimationItemView _prefab;
        [SerializeField] private int _poolSize = 15;

        [Header("Spawn Settings")] [SerializeField]
        private Vector2 _spawnPadding = new Vector2(50f, 50f);

        private IObjectPool<CoinAnimationItemView> _pool;

        private void Start()
        {
            _pool = new ObjectPool<CoinAnimationItemView>(
                createFunc: () => Instantiate(_prefab, _spawnArea),
                actionOnGet: item => item.gameObject.SetActive(true),
                actionOnRelease: item => item.gameObject.SetActive(false),
                actionOnDestroy: item => Destroy(item.gameObject),
                collectionCheck: true,
                defaultCapacity: _poolSize);
        }

        public void SpawnCoin(long amount)
        {
            CoinAnimationItemView item = _pool.Get();
            item.Play(GetRandomPosition(), amount, () => _pool.Release(item));
        }

        // === Private ===

        private Vector2 GetRandomPosition()
        {
            Rect rect = _spawnArea.rect;

            float minX = rect.xMin + _spawnPadding.x;
            float maxX = rect.xMax - _spawnPadding.x;
            float minY = rect.yMin + _spawnPadding.y;
            float maxY = rect.yMax - _spawnPadding.y;

            return new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY));
        }
    }
}