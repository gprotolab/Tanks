using System;
using R3;
using VContainer.Unity;

namespace Game.Battle
{
    public class MiniLeaderboardPresenter : IStartable, IDisposable
    {
        private readonly MiniLeaderboardView _view;
        private readonly ScoreService _scoreService;
        private readonly CompositeDisposable _disposables = new();

        public MiniLeaderboardPresenter(
            MiniLeaderboardView view,
            ScoreService scoreService)
        {
            _view = view;
            _scoreService = scoreService;
        }

        public void Start()
        {
            _scoreService.ScoreChanged
                .Subscribe(_ => RefreshView())
                .AddTo(_disposables);
            RefreshView();
        }

        private void RefreshView()
        {
            var ranking = _scoreService.GetCurrentRanking();
            _view.Refresh(ranking);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}