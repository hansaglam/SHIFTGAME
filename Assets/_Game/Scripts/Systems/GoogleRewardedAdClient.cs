using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

namespace Shift.Game
{
    public sealed class GoogleRewardedAdClient : IRewardedAdClient
    {
        public void Initialize(Action<bool> completed)
        {
            MobileAds.Initialize(status => MobileAdsEventExecutor.ExecuteInUpdate(() => completed(status != null)));
        }
        public void Load(string unitId, Action<IRewardedAdHandle> completed)
        {
            var request = new AdRequest();
            // This is not a substitute for UMP. The bootstrap gates initialization first.
            request.Extras.Add("npa", "1");
            RewardedAd.Load(unitId, request, (ad, error) => MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                if (error != null || ad == null) { ad?.Destroy(); completed(null); return; }
                completed(new Handle(ad));
            }));
        }
        private sealed class Handle : IRewardedAdHandle
        {
            private RewardedAd ad;
            public Handle(RewardedAd ad) { this.ad = ad; }
            public bool CanShow => ad != null && ad.CanShowAd();
            public void Show(Action earned, Action closed, Action failed)
            {
                ad.OnAdFullScreenContentClosed += () => MobileAdsEventExecutor.ExecuteInUpdate(closed);
                ad.OnAdFullScreenContentFailed += _ => MobileAdsEventExecutor.ExecuteInUpdate(failed);
                ad.Show(_ => MobileAdsEventExecutor.ExecuteInUpdate(earned));
            }
            public void Dispose() { var previous = ad; ad = null; previous?.Destroy(); }
        }
    }
}
