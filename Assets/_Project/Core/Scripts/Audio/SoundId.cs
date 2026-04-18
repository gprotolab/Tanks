namespace ANut.Core.Audio
{
    public enum SoundId
    {
        None = 0,

        // UI
        UI_Click = 1,
        UI_Upgrade = 2,

        // Battle 
        Battle_Shoot = 100,
        Battle_Explosion = 150,
        Battle_CountdownTick = 200,

        // Merge
        Merge_TankEquip = 300,
        Merge_Success = 301,
        Merge_Sell = 302,

        // Idle 
        Idle_CoinCollect = 400,
    }
}