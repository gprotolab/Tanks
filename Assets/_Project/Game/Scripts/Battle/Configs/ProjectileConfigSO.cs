using UnityEngine;


namespace Game.Battle
{
    [CreateAssetMenu(fileName = "ProjectileConfig", menuName = "Configs/Battle/ProjectileConfig")]
    public class ProjectileConfigSO : ScriptableObject
    {
        [SerializeField] private Projectile _prefab;
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private GameObject _hitEffectPrefab;

        public Projectile Prefab => _prefab;
        public float Speed => _speed;
        public float Lifetime => _lifetime;
        public GameObject HitEffectPrefab => _hitEffectPrefab;
    }
}