using UnityEngine;

namespace Game.Battle
{
    public readonly struct TankDiedSignal
    {
        public readonly int VictimId;
        public readonly int KillerId;
        public readonly Vector3 DeathPosition;

        public TankDiedSignal(int victimId, int killerId, Vector3 deathPosition)
        {
            VictimId = victimId;
            KillerId = killerId;
            DeathPosition = deathPosition;
        }
    }
}