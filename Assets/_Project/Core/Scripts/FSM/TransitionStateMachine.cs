using System;
using System.Collections.Generic;

namespace Game.Core.FSM
{
    public class TransitionStateMachine
    {
        private IState _current;
        private readonly Dictionary<IState, List<(IState next, Func<bool> condition)>> _transitions = new();

        public IState Current => _current;

        public void AddTransition(IState from, IState to, Func<bool> condition)
        {
            if (!_transitions.ContainsKey(from))
                _transitions[from] = new List<(IState, Func<bool>)>();

            _transitions[from].Add((to, condition));
        }

        public void Init(IState initialState) => Transition(initialState);

        public void ForceState(IState state) => Transition(state);

        public void Tick(float dt)
        {
            _current?.OnTick(dt);

            if (_current == null || !_transitions.TryGetValue(_current, out var transitions))
                return;

            foreach (var (next, condition) in transitions)
            {
                if (!condition()) continue;
                Transition(next);
                break;
            }
        }

        private void Transition(IState next)
        {
            _current?.OnExit();
            _current = next;
            _current?.OnEnter();
        }
    }
}