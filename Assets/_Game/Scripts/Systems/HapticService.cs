using UnityEngine;

namespace Shift.Game
{
    // Unity's fallback does not expose calibrated intensities; Light is intentionally a no-op.
    public sealed class HapticService
    {
        private readonly GameFeelSettings settings;
        private float nextAllowed;
        public HapticService(GameFeelSettings settings) { this.settings = settings; }
        public void Light() { }
        public void Medium() => Vibrate();
        public void Impact() => Vibrate();
        public void Exit() => Vibrate();
        public void Success() => Vibrate();
        public void Failure() => Vibrate();
        private void Vibrate()
        {
            if (!settings.hapticsEnabled || Time.realtimeSinceStartup < nextAllowed) return;
            nextAllowed = Time.realtimeSinceStartup + settings.hapticCooldown;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Handheld.Vibrate();
#endif
        }
    }
}
