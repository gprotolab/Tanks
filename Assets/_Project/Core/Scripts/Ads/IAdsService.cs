using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;

namespace ANut.Core.Ads
{
    public interface IAdsService
    {
        ReadOnlyReactiveProperty<bool> IsRewardedReady { get; }
        ReadOnlyReactiveProperty<bool> IsInterstitialReady { get; }

        UniTask<bool> ShowRewardedAsync(string placement, CancellationToken ct);

        UniTask ShowInterstitialAsync(string placement, CancellationToken ct);
    }
}