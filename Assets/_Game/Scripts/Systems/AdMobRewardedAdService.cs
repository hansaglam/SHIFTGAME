using System;

namespace Shift.Game
{
    // The SDK boundary is deliberately small: tests never need a Google network request.
    public interface IRewardedAdHandle : IDisposable
    {
        bool CanShow { get; }
        void Show(Action earned, Action closed, Action failed);
    }
    public interface IRewardedAdClient
    {
        void Initialize(Action<bool> completed);
        void Load(string unitId, Action<IRewardedAdHandle> completed);
    }

    // All methods and callbacks run on Unity's main thread; the SDK adapter marshals events.
    public sealed class AdMobRewardedAdService : IRewardedAdService, IDisposable
    {
        private readonly IRewardedAdClient client;
        private readonly string unitId;
        private readonly Func<double> now;
        private readonly Func<bool> consent;
        private bool consentAllowed = true;
        public bool IsShowing => showing;
        private bool Permitted => consentAllowed && consent();
        private IRewardedAdHandle ad;
        private Action<bool> completion;
        private bool started, initialized, loading, showing, disposed, paused, reported;
        private int generation, failures;
        private double deadline, loadedAt, nextLoad = double.PositiveInfinity;
        public string Status { get; private set; } = "not_started";
        public bool IsRewardedAdAvailable
        {
            get
            {
                if (disposed || !Permitted || paused || showing || ad == null || now() - loadedAt >= 3300) return false;
                try { return ad.CanShow; } catch { return false; }
            }
        }
        public AdMobRewardedAdService(IRewardedAdClient client, string unitId, Func<double> now, Func<bool> consent = null)
        { this.client = client; this.unitId = unitId; this.now = now; this.consent = consent ?? (() => true); }

        // Called only after the bootstrap's consent gate permits SDK initialization.
        public void Start()
        {
            if (started || disposed || !Permitted) return;
            started = true;
            if (string.IsNullOrWhiteSpace(unitId)) { Status = "missing_config"; return; }
            Status = "initializing"; deadline = now() + 30;
            int token = ++generation;
            try
            {
                client.Initialize(ok =>
                {
                    if (disposed || token != generation || initialized) return;
                    ++generation;
                    initialized = ok; Status = ok ? "initialized" : "initialization_failed";
                    if (ok) nextLoad = now();
                });
            }
            catch { ++generation; Status = "initialization_failed"; }
        }
        public void Tick()
        {
            if (disposed || paused || !Permitted) return;
            if (Status == "initializing" && now() >= deadline)
            { ++generation; Status = "initialization_timeout"; }
            if (loading && now() >= deadline)
            { ++generation; loading = false; LoadFailed(); }
            if (showing) return; // A pause/resume never implies an earned reward or dismissal.
            if (ad != null && now() - loadedAt >= 3300)
            { Release(); nextLoad = now(); }
            if (initialized && !loading && ad == null && now() >= nextLoad) Load();
        }
        private void Load()
        {
            nextLoad = double.PositiveInfinity; loading = true; Status = "loading";
            deadline = now() + 30; int token = ++generation;
            try
            {
                client.Load(unitId, result =>
                {
                    if (disposed || token != generation || !loading)
                    { if (!ReferenceEquals(ad, result)) Destroy(result); return; }
                    loading = false;
                    if (!Permitted) { Destroy(result); return; }
                    if (result == null) { LoadFailed(); return; }
                    ad = result; loadedAt = now(); failures = 0; Status = "ready";
                });
            }
            catch { ++generation; loading = false; LoadFailed(); }
        }
        private void LoadFailed()
        {
            Status = "load_failed";
            // Three delayed retries, then stay unavailable for this app session.
            failures++;
            nextLoad = failures <= 3 ? now() + 15 * Math.Pow(2, failures - 1) : double.PositiveInfinity;
        }
        public void ShowRewardedAd(RewardReason reason, Action<bool> onComplete)
        {
            if ((reason != RewardReason.Hint && reason != RewardReason.Undo) || !IsRewardedAdAvailable)
            { onComplete?.Invoke(false); return; }
            showing = true; reported = false; completion = onComplete; Status = "showing";
            int token = ++generation;
            try
            {
                ad.Show(() =>
                {
                    if (disposed || token != generation || !showing || !Permitted) return;
                    Status = "reward_earned"; Report(true);
                }, () => EndShow(token, "closed"), () => EndShow(token, "show_failed"));
            }
            catch { EndShow(token, "show_failed"); }
        }
        private void EndShow(int token, string status)
        {
            if (disposed || token != generation || !showing) return;
            ++generation; showing = false; Status = status;
            Release(); nextLoad = now() + 1;
            Report(false); // No-op after confirmed reward; close alone is always failure.
        }
        private void Report(bool success)
        {
            if (reported) return;
            reported = true; var callback = completion; completion = null;
            callback?.Invoke(success);
        }
        private static void Destroy(IRewardedAdHandle handle) { try { handle?.Dispose(); } catch { } }
        private void Release() { var previous = ad; ad = null; Destroy(previous); }
        public void Pause(bool value) { paused = value; }
        public void SetConsentAllowed(bool allowed)
        {
            if (disposed || consentAllowed == allowed) return;
            consentAllowed = allowed;
            if (!allowed)
            {
                // Do not cancel the one in-flight initialization; its completion may be
                // retained, but no load is allowed until the gate permits it again.
                if (initialized) ++generation;
                loading = showing = false; Release(); Report(false);
                nextLoad = double.PositiveInfinity;
            }
            else if (initialized) { failures = 0; nextLoad = now(); }
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; ++generation; showing = loading = false;
            Release(); Status = "disposed"; Report(false);
        }
    }
}
