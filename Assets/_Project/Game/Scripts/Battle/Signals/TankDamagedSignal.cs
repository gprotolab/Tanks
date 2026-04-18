using UnityEngine;


namespace Game.Battle
{
    public readonly struct TankDamagedSignal
    {
        public readonly int AttackerId;
        public readonly int TargetId;
        public readonly float Damage;
        public readonly Vector3 HitPoint;
        public readonly bool IsTargetPlayer;

        public TankDamagedSignal(int attackerId, int targetId, float damage, Vector3 hitPoint, bool isTargetPlayer)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            Damage = damage;
            HitPoint = hitPoint;
            IsTargetPlayer = isTargetPlayer;
        }
    }
}