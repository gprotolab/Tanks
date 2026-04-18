namespace Game.Core.FSM
{
    public interface IState
    {
        void OnEnter();
        void OnTick(float dt);
        void OnExit();
    }
}