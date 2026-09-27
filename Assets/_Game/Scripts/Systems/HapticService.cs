using System;
using UnityEngine;

namespace Shift.Game
{
    public enum HapticCue { Light, Medium, Strong, Warning }
    public interface IHapticOutput { void Pulse(HapticCue cue, Func<bool> stillValid); }
    public sealed class HapticService
    {
        private readonly GameFeelSettings settings;
        private readonly IHapticOutput output;
        private readonly Func<float> clock;
        private float nextAllowed;
        private bool paused, unfocused, interactionUsed, bigUsed, megaUsed, reaction;
        private int generation, reactionPulses;
        public event Action<HapticCue> PulseSent;
        public HapticService(GameFeelSettings settings, IHapticOutput output = null, Func<float> clock = null)
        { this.settings = settings; this.output = output ?? new PlatformHapticOutput(); this.clock = clock ?? (() => Time.realtimeSinceStartup); }
        public void Cancel() { generation++; reaction = false; }
        public void Pause(bool value) { paused = value; if (value) Cancel(); }
        public void Focus(bool value) { unfocused = !value; if (!value) Cancel(); }
        public void EndReaction() { reaction = false; }
        public void BeginReaction(bool blocked, int depth = 0)
        {
            reaction = true; reactionPulses = 0; bigUsed = megaUsed = false; interactionUsed = depth >= 4;
            Send(blocked ? HapticCue.Warning : HapticCue.Light);
        }
        public void Interaction()
        { if (!interactionUsed) { interactionUsed = true; Send(HapticCue.Medium); } }
        public void Step(int depth)
        {
            var tier = ReactionPresentation.Classify(depth);
            if (tier == ReactionTier.BigShift && !bigUsed) { bigUsed = true; Send(HapticCue.Medium); }
            if (tier == ReactionTier.MegaShift && !megaUsed) { megaUsed = true; Send(HapticCue.Strong); }
        }
        public void Outcome(AudioCue cue)
        {
            Send(cue == AudioCue.Lose ? HapticCue.Warning :
                cue == AudioCue.Win || cue == AudioCue.DailyComplete ? HapticCue.Medium : HapticCue.Strong);
            reaction = false;
        }
        public void Light() => Send(HapticCue.Light);
        public void Medium() => Send(HapticCue.Medium);
        public void Impact() => Interaction();
        public void Exit() => Interaction();
        public void Success() => Outcome(AudioCue.Win);
        public void Failure() => Outcome(AudioCue.Lose);
        private void Send(HapticCue cue)
        {
            if (!settings.hapticsEnabled || paused || unfocused || clock() < nextAllowed || (reaction && reactionPulses >= 3)) return;
            nextAllowed = clock() + Mathf.Max(.1f, settings.hapticCooldown);
            if (reaction) reactionPulses++;
            int token = generation;
            try { output.Pulse(cue, () => token == generation && settings.hapticsEnabled && !paused && !unfocused); PulseSent?.Invoke(cue); }
            catch (Exception) { /* Unsupported device/plugin: silent, never interrupt gameplay. */ }
        }
    }
    internal sealed class PlatformHapticOutput : IHapticOutput
    {
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")] private static extern void ShiftImpact(int style);
#endif
        public void Pulse(HapticCue cue, Func<bool> stillValid)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            long requested = System.Diagnostics.Stopwatch.GetTimestamp();
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                // Drop delayed queued UI work after pause/restart/settings changes.
                if (!stillValid() || (System.Diagnostics.Stopwatch.GetTimestamp() - requested) / (double)System.Diagnostics.Stopwatch.Frequency > .1) return;
                try
                {
                    using var currentPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    using var currentActivity = currentPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    using var window = currentActivity.Call<AndroidJavaObject>("getWindow");
                    using var view = window.Call<AndroidJavaObject>("getDecorView");
                    using var version = new AndroidJavaClass("android.os.Build$VERSION");
                    int api = version.GetStatic<int>("SDK_INT");
                    int effect = cue == HapticCue.Light ? 3 : cue == HapticCue.Warning ? 4 : cue == HapticCue.Medium ? 6 : api >= 30 ? 16 : 0;
                    view.Call<bool>("performHapticFeedback", effect); // No ignore-settings flags, no permission required.
                }
                catch (Exception) { }
            }));
#elif UNITY_IOS && !UNITY_EDITOR
            if (stillValid()) ShiftImpact(cue == HapticCue.Light || cue == HapticCue.Warning ? 0 : cue == HapticCue.Medium ? 1 : 2);
#endif
        }
    }
}
