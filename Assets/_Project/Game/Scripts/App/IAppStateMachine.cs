using Game.Battle;

namespace Game.App
{
    public interface IAppStateMachine
    {
        void EnterHome();

        void EnterBattle(BattleMode mode = BattleMode.FFA);

        void EnterTutorialBattle();
    }
}