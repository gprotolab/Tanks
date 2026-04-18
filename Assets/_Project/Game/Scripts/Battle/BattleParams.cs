namespace Game.Battle
{
    // Parameters for the upcoming battle.
    public sealed class BattleParams
    {
        private BattleMode _mode = BattleMode.FFA;

        public BattleMode Mode => _mode;

        public void SetMode(BattleMode mode) => _mode = mode;
    }
}