using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Battle;
using ANut.Core;

namespace Game.App
{
    public class AppStateMachine : IAppStateMachine
    {
        private readonly Dictionary<Type, IAppState> _states = new();
        private readonly BattleParams _battleParams;
        private bool _isTransitioning;

        private IAppState _currentState;

        public AppStateMachine(
            IEnumerable<IAppState> states,
            BattleParams battleParams)
        {
            _battleParams = battleParams;

            foreach (var state in states)
                _states[state.GetType()] = state;
        }

        public void EnterHome() => EnterAsync<HomeState>().Forget();

        public void EnterBattle(BattleMode mode)
        {
            _battleParams.SetMode(mode);
            EnterAsync<BattleState>().Forget();
        }

        public void EnterTutorialBattle() => EnterAsync<TutorialBattleState>().Forget();

        private async UniTask EnterAsync<TState>(CancellationToken ct = default) where TState : IAppState
        {
            if (_isTransitioning)
                return;

            _isTransitioning = true;

            try
            {
                if (_states.TryGetValue(typeof(TState), out var nextState) == false)
                    throw new InvalidOperationException($"State is not registered: {typeof(TState).Name}");

                if (ReferenceEquals(_currentState, nextState))
                    return;

                if (_currentState != null)
                    await _currentState.Exit(ct);

                _currentState = nextState;
                await _currentState.Enter(ct);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Log.Exception(exception);
                throw;
            }
            finally
            {
                _isTransitioning = false;
            }
        }
    }
}