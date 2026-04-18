using System;
using System.Collections.Generic;
using MessagePipe;
using R3;
using ANut.Core;

namespace Game.Battle
{
    public class ScoreService : IDisposable
    {
        private readonly Dictionary<int, TankStatsEntry> _stats = new();
        private readonly Subject<Unit> _scoreChanged = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly List<ScoreEntry> _rankingBuffer = new();
        private ScoreEntry[] _ranking = Array.Empty<ScoreEntry>();

        public Observable<Unit> ScoreChanged => _scoreChanged;

        public ScoreService(
            ISubscriber<TankSpawnedSignal> tankSpawned,
            ISubscriber<TankDamagedSignal> tankDamaged,
            ISubscriber<TankDiedSignal> tankDied)
        {
            tankSpawned.Subscribe(s => RegisterTank(s.TankId, s.DisplayName, s.IsPlayer)).AddTo(_disposables);
            tankDamaged.Subscribe(s => AddDamage(s.AttackerId, s.Damage)).AddTo(_disposables);
            tankDied.Subscribe(s => RecordKill(s.KillerId, s.VictimId)).AddTo(_disposables);
        }

        public ScoreEntry[] GetCurrentRanking() => _ranking;

        public int GetPlayerPlace()
        {
            foreach (var entry in _ranking)
            {
                if (entry.IsPlayer)
                    return entry.Place;
            }

            return _ranking.Length;
        }

        private void RebuildRanking()
        {
            _rankingBuffer.Clear();
            foreach (var (id, entry) in _stats)
            {
                _rankingBuffer.Add(new ScoreEntry
                {
                    TankId = id,
                    Name = entry.Name,
                    IsPlayer = entry.IsPlayer,
                    Kills = entry.Kills,
                    Deaths = entry.Deaths,
                    TotalDamage = entry.TotalDamageDealt
                });
            }

            _rankingBuffer.Sort((a, b) =>
            {
                int killDiff = b.Kills.CompareTo(a.Kills);
                if (killDiff != 0) return killDiff;

                int damageDiff = b.TotalDamage.CompareTo(a.TotalDamage);
                if (damageDiff != 0) return damageDiff;

                // Player wins all ties — deterministic, not a hack:
                // at equal stats the human player is ranked first.
                int playerDiff = b.IsPlayer.CompareTo(a.IsPlayer);
                if (playerDiff != 0) return playerDiff;

                // Final stable tie-breaker: registration order (lower id = earlier spawn).
                return a.TankId.CompareTo(b.TankId);
            });

            if (_ranking.Length != _rankingBuffer.Count)
                _ranking = new ScoreEntry[_rankingBuffer.Count];

            for (int i = 0; i < _rankingBuffer.Count; i++)
            {
                var entry = _rankingBuffer[i];
                entry.Place = i + 1;
                _ranking[i] = entry;
            }
        }

        public void Dispose()
        {
            _scoreChanged.Dispose();
            _disposables.Dispose();
        }

        private void RegisterTank(int id, string name, bool isPlayer)
        {
            if (_stats.ContainsKey(id))
            {
                Log.Warning("[ScoreService] Tank Id={0} is already registered.", id);
                return;
            }

            _stats[id] = new TankStatsEntry {Name = name, IsPlayer = isPlayer};
            RebuildRanking();
            _scoreChanged.OnNext(Unit.Default);
        }

        private void AddDamage(int attackerId, float damage)
        {
            if (!_stats.TryGetValue(attackerId, out var entry)) return;
            entry.TotalDamageDealt += damage;
            RebuildRanking();
            _scoreChanged.OnNext(Unit.Default);
        }

        private void RecordKill(int killerId, int victimId)
        {
            if (_stats.TryGetValue(killerId, out var killer))
                killer.Kills++;

            if (_stats.TryGetValue(victimId, out var victim))
                victim.Deaths++;

            RebuildRanking();
            _scoreChanged.OnNext(Unit.Default);
        }

        private class TankStatsEntry
        {
            public string Name;
            public bool IsPlayer;
            public int Kills;
            public int Deaths;
            public float TotalDamageDealt;
        }
    }

    [Serializable]
    public struct ScoreEntry
    {
        public int Place;
        public int TankId;
        public string Name;
        public bool IsPlayer;
        public int Kills;
        public int Deaths;
        public float TotalDamage;
    }
}