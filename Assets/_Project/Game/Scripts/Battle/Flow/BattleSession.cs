namespace Game.Battle
{
    // Runtime data for the current battle session.
    public class BattleSession
    {
        public BattleMode Mode { get; internal set; }

        public ArenaData Arena { get; internal set; }
        public float RemainingTime { get; internal set; }
        public bool IsPlayerDead { get; internal set; }
        public Tank PlayerTank { get; internal set; }
    }
}