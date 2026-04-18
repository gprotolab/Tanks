using UnityEngine;

namespace Game.Battle
{
    public class TurretVisualRoot : MonoBehaviour
    {
        [SerializeField] private Transform _recoilTarget;
        [SerializeField] private Transform _firePoint;

        public Transform RecoilTarget => _recoilTarget;

        public Transform FirePoint => _firePoint;
    }
}