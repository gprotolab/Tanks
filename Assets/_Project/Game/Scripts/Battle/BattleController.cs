using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer.Unity;
using ANut.Core;

namespace Game.Battle
{
    public class BattleController : IAsyncStartable, IDisposable
    {
        private readonly IBattleFlow _flow;
        private readonly BattleSession _session;
        private readonly ArenaLoader _arenaLoader;

        public BattleController(
            IBattleFlow flow,
            BattleSession session,
            ArenaLoader arenaLoader,
            BattleParams battleParams
        )
        {
            session.Mode = battleParams.Mode;

            _flow = flow;
            _session = session;
            _arenaLoader = arenaLoader;
        }

        public async UniTask StartAsync(CancellationToken ct)
        {
            Log.Info("[BattleController] Battle started. Mode={0}", _session.Mode);

            try
            {
                await _flow.RunAsync(ct);
            }
            catch (OperationCanceledException)
            {
                // Silent cancellation when scope is disposed.
                Log.Info("[BattleController] Battle cancelled (scope disposed).");
            }
        }

        public void Dispose()
        {
            if (_session != null)
                _arenaLoader.Unload(_session.Arena);
        }
    }
}