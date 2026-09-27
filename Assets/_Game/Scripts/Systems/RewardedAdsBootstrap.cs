using System;
using UnityEngine;

namespace Shift.Game
{
    // Consent is resolved before ANY SDK initialization, not just before Load.
    public interface IAdConsentGate { void Prepare(Action<bool> canRequestAds); }

    public sealed class RewardedAdsBootstrap : MonoBehaviour
    {
        private static RewardedAdsBootstrap instance;
        private ConsentedRewardedAds service;
        public static IRewardedAdService Shared
        {
            get
            {
                if (instance == null)
                {
                    var root = new GameObject("SHIFT Rewarded Ads");
                    instance = root.AddComponent<RewardedAdsBootstrap>();
                    DontDestroyOnLoad(root);
                    instance.Configure();
                }
                return (IRewardedAdService)instance.service ?? new NullRewardedAdService();
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { instance = null; }
        private void Configure()
        {
            var config = Resources.Load<AdConfiguration>("AdConfiguration");
            if (config == null) { Debug.LogWarning("SHIFT ads unavailable: AdConfiguration is missing."); return; }
#if UNITY_EDITOR
            // Automated tests and normal editor play stay completely offline. Opt in manually
            // to Google's editor placeholder only through the dedicated preview setting.
            if (!config.EnableEditorPreview || Application.isBatchMode) return;
#endif
            var platform = Application.platform == RuntimePlatform.IPhonePlayer ? AdPlatform.Ios :
                Application.platform == RuntimePlatform.Android ? AdPlatform.Android : AdPlatform.Unsupported;
            if (!config.TryResolve(platform, Application.isEditor, Debug.isDebugBuild, out var unit, out var reason))
            {
                if (reason != "disabled" && reason != "unsupported_platform")
                    Debug.LogWarning("SHIFT ads unavailable: " + reason + ". Configure ads before release.");
                return;
            }
            var gate = new GoogleUmpConsentGate(new GoogleUmpConsentClient(config), () => Time.realtimeSinceStartupAsDouble);
            service = new ConsentedRewardedAds(gate, new GoogleRewardedAdClient(), unit, () => Time.realtimeSinceStartupAsDouble);
            service.Start(); // Test ads also go through UMP. Never assume permission.
        }
        private void Update() => service?.Tick();
        private void OnApplicationPause(bool paused) => service?.Pause(paused);
        private void OnDestroy()
        {
            service?.Dispose();
            if (instance == this) instance = null;
        }
    }
}
