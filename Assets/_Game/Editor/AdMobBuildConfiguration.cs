using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Shift.Game.Editor
{
    // Runs before Google's -1 preprocessors. No custom manifest or plist is introduced.
    public sealed class AdMobBuildConfiguration : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100;
        public void OnPreprocessBuild(BuildReport report)
        {
            bool ios = report.summary.platform == BuildTarget.iOS;
            if (!ios && report.summary.platform != BuildTarget.Android) return;
            var config = Resources.Load<AdConfiguration>("AdConfiguration");
            bool development = (report.summary.options & BuildOptions.Development) != 0;
            if (config == null) throw new BuildFailedException("SHIFT: AdConfiguration asset is missing.");
            string appId = ResolveAppId(config, ios ? AdPlatform.Ios : AdPlatform.Android, development);
            var settings = AssetDatabase.LoadMainAssetAtPath("Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset");
            if (settings == null) throw new BuildFailedException("SHIFT: open Assets > Google Mobile Ads > Settings to create SDK settings.");
            var serialized = new SerializedObject(settings);
            serialized.FindProperty(ios ? "adMobIOSAppId" : "adMobAndroidAppId").stringValue = appId;
            serialized.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssets();
        }
        public static string ResolveAppId(AdConfiguration config, AdPlatform platform, bool development)
        {
            if (config == null) throw new BuildFailedException("SHIFT: AdConfiguration asset is missing.");
            if (platform != AdPlatform.Android && platform != AdPlatform.Ios) throw new BuildFailedException("SHIFT: unsupported ad platform.");
            if (development) return platform == AdPlatform.Ios ? AdConfiguration.IosTestAppId : AdConfiguration.AndroidTestAppId;
            if (!config.ValidateProduction(platform, out var reason)) throw new BuildFailedException("SHIFT release ads: " + reason);
            if (!config.ConsentProviderConfigured) throw new BuildFailedException("SHIFT release ads: UMP provider configuration is required.");
            return platform == AdPlatform.Ios ? config.IosAppId : config.AndroidAppId;
        }
    }
}
