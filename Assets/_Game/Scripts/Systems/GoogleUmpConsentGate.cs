using System;

namespace Shift.Game
{
    public enum ConsentTestGeography { Disabled, Eea, Other }
    public interface IPrivacyChoices
    {
        bool IsPrivacyOptionsRequired { get; }
        bool IsBusy { get; }
        void ShowPrivacyOptions(Action<bool> completed);
    }
    public interface IUmpConsentClient
    {
        bool CanRequestAds { get; }
        bool IsPrivacyOptionsRequired { get; }
        void Update(Action<bool> completed);
        void LoadAndShowIfRequired(Action<bool> completed);
        void ShowPrivacyOptions(Action<bool> completed);
    }
    // No cached consent, raw SDK error, TCF string or identifiers enter game state.
    public sealed class GoogleUmpConsentGate : IAdConsentGate, IPrivacyChoices, IDisposable
    {
        private readonly IUmpConsentClient client;
        private readonly Func<double> now;
        private bool disposed, resolved, busy;
        private int generation;
        private double updateDeadline;
        private Action<bool> completion;
        public event Action Changed;
        public string Status { get; private set; } = "unresolved";
        public bool IsBusy => busy;
        public bool CanRequestAds
        {
            get { try { return !disposed && resolved && !busy && client.CanRequestAds; } catch { return false; } }
        }
        public bool IsPrivacyOptionsRequired
        {
            get { try { return !disposed && client.IsPrivacyOptionsRequired; } catch { return false; } }
        }
        public GoogleUmpConsentGate(IUmpConsentClient client, Func<double> now) { this.client = client; this.now = now; }
        public void Prepare(Action<bool> completed)
        {
            if (disposed || busy) { completed?.Invoke(false); return; }
            busy = true; resolved = false; completion = completed; Status = "updating";
            int token = ++generation; updateDeadline = now() + 30; Changed?.Invoke();
            try
            {
                client.Update(ok =>
                {
                    if (!Current(token) || Status != "updating") return;
                    if (!ok) { Finish(token, false, "update_failed"); return; }
                    Status = "form"; Changed?.Invoke();
                    try { client.LoadAndShowIfRequired(success => Finish(token, success, success ? "resolved" : "form_failed")); }
                    catch { Finish(token, false, "form_failed"); }
                });
            }
            catch { Finish(token, false, "update_failed"); }
        }
        public void ShowPrivacyOptions(Action<bool> completed)
        {
            if (disposed || busy || !IsPrivacyOptionsRequired) { completed?.Invoke(false); return; }
            busy = true; resolved = false; completion = completed; Status = "privacy_options";
            int token = ++generation;
            Changed?.Invoke(); // Invalidate cached ads BEFORE presenting changed choices.
            try { client.ShowPrivacyOptions(ok => Finish(token, ok, ok ? "resolved" : "privacy_failed")); }
            catch { Finish(token, false, "privacy_failed"); }
        }
        private bool Current(int token) => !disposed && busy && token == generation;
        private void Finish(int token, bool success, string status)
        {
            if (!Current(token)) return;
            ++generation; busy = false; resolved = success; Status = status;
            var done = completion; completion = null;
            Changed?.Invoke(); done?.Invoke(CanRequestAds);
        }
        public void Tick()
        {
            if (busy && Status == "updating" && now() >= updateDeadline) Finish(generation, false, "update_timeout");
            // Never time out a native form while a user is reading it. Gameplay is not waiting.
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; resolved = busy = false; ++generation;
            var done = completion; completion = null; Changed?.Invoke(); done?.Invoke(false);
        }
    }
}
