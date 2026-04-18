using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;

namespace ANut.Core.Ads
{
    public interface IRawAdsProvider
    {
        UniTask InitializeAsync(CancellationToken ct);

        ReadOnlyReactiveProperty<bool> IsRewardedReady { get; }
        ReadOnlyReactiveProperty<bool> IsInterstitialReady { get; }

        UniTask<bool> ShowRewardedAsync(string placement, CancellationToken ct);

        UniTask ShowInterstitialAsync(string placement, CancellationToken ct);
    }
}