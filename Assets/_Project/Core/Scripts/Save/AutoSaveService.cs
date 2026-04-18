using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace ANut.Core.Save
{
    public class AutoSaveService : IStartable, IDisposable
    {
        private const int AUTO_SAVE_INTERVAL_SECONDS = 20;

        private readonly ISaveService _saveService;
        private CancellationTokenSource _cts;

        public AutoSaveService(ISaveService saveService)
        {
            _saveService = saveService;
        }

        void IStartable.Start()
        {
            _cts = new CancellationTokenSource();
            PeriodicAutoSaveAsync(_cts.Token).Forget();
            Application.quitting += OnApplicationQuit;
            Application.focusChanged += OnApplicationFocusChanged;

            Log.Info("[AutoSaveService] Started. Interval: {0}s", AUTO_SAVE_INTERVAL_SECONDS);
        }

        private void OnApplicationFocusChanged(bool hasFocus)
        {
            if (!hasFocus)
            {
                _saveService.Save();
            }
        }

        public void Dispose()
        {
            Application.quitting -= OnApplicationQuit;
            Application.focusChanged -= OnApplicationFocusChanged;
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private async UniTaskVoid PeriodicAutoSaveAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(AUTO_SAVE_INTERVAL_SECONDS), cancellationToken: ct);
                await _saveService.SaveAsync(ct);
            }
        }

        private void OnApplicationQuit()
        {
            _saveService.Save();
        }
    }
}