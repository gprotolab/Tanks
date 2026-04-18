namespace Game.Battle
{
    public readonly struct TankSpawnedSignal
    {
        public readonly int TankId;
        public readonly string DisplayName;
        public readonly bool IsPlayer;

        public TankSpawnedSignal(int tankId, string displayName, bool isPlayer)
        {
            TankId = tankId;
            DisplayName = displayName;
            IsPlayer = isPlayer;
        }
    }
}