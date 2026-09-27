using System;

namespace Shift.Game
{
    // One session keeps SDK initialization intact across privacy changes; loaded ads do not survive them.
    public sealed class ConsentedRewardedAds : IRewardedAdService, IPrivacyChoices, IDisposable
    {
        private readonly GoogleUmpConsentGate gate;
        private readonly AdMobRewardedAdService ads;
        private bool started, disposed;
        public ConsentedRewardedAds(GoogleUmpConsentGate gate, IRewardedAdClient client, string unit, Func<double> now)
        {
            this.gate = gate;
            ads = new AdMobRewardedAdService(client, unit, now, () => gate.CanRequestAds);
            ads.SetConsentAllowed(false); gate.Changed += Refresh;
        }
        public bool IsRewardedAdAvailable => !disposed && gate.CanRequestAds && ads.IsRewardedAdAvailable;
        public bool IsPrivacyOptionsRequired => !disposed && gate.IsPrivacyOptionsRequired;
        public bool IsBusy => gate.IsBusy || ads.IsShowing;
        public void Start() { if (started || disposed) return; started = true; gate.Prepare(_ => Refresh()); }
        private void Refresh()
        {
            if (disposed) return;
            ads.SetConsentAllowed(gate.CanRequestAds);
            if (gate.CanRequestAds) ads.Start();
        }
        public void Tick() { if (disposed) return; gate.Tick(); Refresh(); ads.Tick(); }
        public void Pause(bool paused) { ads.Pause(paused); if (!paused) Refresh(); }
        public void ShowRewardedAd(RewardReason reason, Action<bool> completed)
        {
            Refresh();
            if (!IsRewardedAdAvailable) { completed?.Invoke(false); return; }
            ads.ShowRewardedAd(reason, completed);
        }
        public void ShowPrivacyOptions(Action<bool> completed)
        {
            if (disposed || IsBusy) { completed?.Invoke(false); return; }
            gate.ShowPrivacyOptions(completed);
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; gate.Changed -= Refresh; ads.Dispose(); gate.Dispose();
        }
    }
}
