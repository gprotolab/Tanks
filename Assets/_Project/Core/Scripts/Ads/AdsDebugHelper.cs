using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace ANut.Core.Ads
{
    public class AdsDebugHelper : MonoBehaviour
    {
        private IAdsService _ads;

        [Inject]
        private void Construct(IAdsService ads)
        {
            _ads = ads;
        }

        [ContextMenu("Show Rewarded")]
        private void ShowRewarded()
            => _ads.ShowRewardedAsync("debug_context_menu", this.destroyCancellationToken).Forget();

        [ContextMenu("Show Interstitial")]
        private void ShowInterstitial()
            => _ads.ShowInterstitialAsync("debug_context_menu", this.destroyCancellationToken).Forget();
    }
}