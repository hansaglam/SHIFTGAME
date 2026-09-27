using UnityEngine;
using UnityEngine.Serialization;
using System.Text.RegularExpressions;

namespace Shift.Game
{
    public enum AdPlatform { Unsupported, Android, Ios }
    [CreateAssetMenu(menuName = "SHIFT/Ad Configuration")]
    public sealed class AdConfiguration : ScriptableObject
    {
        public bool EnableAds = true;
        public bool UseTestAdsInDevelopment = true;
        public bool EnableEditorPreview;
        public string AndroidAppId = "";
        public string AndroidRewardedAdUnitId = "";
        public string IosAppId = "";
        public string IosRewardedAdUnitId = "";
        // Build/config guard only; the runtime must ALSO pass the live UMP gate.
        [FormerlySerializedAs("ProductionConsentReady")]
        public bool ConsentProviderConfigured;
        // Retained for source compatibility. Setting this never supplies consent.
        public bool ProductionConsentReady { get => ConsentProviderConfigured; set => ConsentProviderConfigured = value; }
        public bool TagForUnderAgeOfConsent;
        public bool EnableConsentDebug;
        public ConsentTestGeography ConsentDebugGeography;
        public static bool IsAppIdValid(string value) => value != null && Regex.IsMatch(value, @"\Aca-app-pub-[0-9]{16}~[0-9]{10}\z");
        public static bool IsRewardedIdValid(string value) => value != null && Regex.IsMatch(value, @"\Aca-app-pub-[0-9]{16}/[0-9]{10}\z");
        public bool ValidateProduction(AdPlatform platform, out string reason)
        {
            reason = "unsupported_platform";
            if (platform != AdPlatform.Android && platform != AdPlatform.Ios) return false;
            string app = platform == AdPlatform.Android ? AndroidAppId : IosAppId;
            string unit = platform == AdPlatform.Android ? AndroidRewardedAdUnitId : IosRewardedAdUnitId;
            if (string.IsNullOrWhiteSpace(app) || string.IsNullOrWhiteSpace(unit)) { reason = "missing_production_ids"; return false; }
            if (!IsAppIdValid(app)) { reason = "invalid_app_id"; return false; }
            if (!IsRewardedIdValid(unit)) { reason = "invalid_rewarded_id"; return false; }
            if (app.Substring(0, 27) != unit.Substring(0, 27)) { reason = "publisher_mismatch"; return false; }
            if (app == AndroidTestAppId || app == IosTestAppId || unit == AndroidTestRewardedId || unit == IosTestRewardedId)
            { reason = "test_ids_in_release"; return false; }
            reason = "production"; return true;
        }
        public const string AndroidTestAppId = "ca-app-pub-3940256099942544~3347511713";
        public const string IosTestAppId = "ca-app-pub-3940256099942544~1458002511";
        public const string AndroidTestRewardedId = "ca-app-pub-3940256099942544/5224354917";
        public const string IosTestRewardedId = "ca-app-pub-3940256099942544/1712485313";
        public bool TryResolve(AdPlatform platform, bool editor, bool development, out string unit, out string reason)
        {
            unit = ""; reason = "disabled";
            if (!EnableAds) return false;
            if (editor || development)
            {
                // Development builds can disable requests, but can never select production ads.
                if (!editor && !UseTestAdsInDevelopment) return false;
                unit = platform == AdPlatform.Ios ? IosTestRewardedId : AndroidTestRewardedId;
                reason = "test"; return true;
            }
            if (!ValidateProduction(platform, out reason)) return false;
            unit = platform == AdPlatform.Android ? AndroidRewardedAdUnitId : IosRewardedAdUnitId;
            if (!ConsentProviderConfigured) { unit = ""; reason = "consent_not_configured"; return false; }
            reason = "production"; return true;
        }
    }
}
