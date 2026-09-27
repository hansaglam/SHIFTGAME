using System;
using System.Collections.Generic;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;

namespace Shift.Game
{
    public sealed class GoogleUmpConsentClient : IUmpConsentClient
    {
        private readonly ConsentRequestParameters parameters;
        public bool CanRequestAds => ConsentInformation.CanRequestAds();
        public bool IsPrivacyOptionsRequired => ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
        public GoogleUmpConsentClient(AdConfiguration config)
        {
            parameters = new ConsentRequestParameters { TagForUnderAgeOfConsent = config.TagForUnderAgeOfConsent };
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (config.EnableConsentDebug)
            {
                parameters.ConsentDebugSettings = new ConsentDebugSettings
                {
                    DebugGeography = config.ConsentDebugGeography == ConsentTestGeography.Eea ? DebugGeography.EEA :
                        config.ConsentDebugGeography == ConsentTestGeography.Other ? DebugGeography.Other : DebugGeography.Disabled,
                    // Configure on the test device; never serialize real hashed IDs into assets.
                    TestDeviceHashedIds = new List<string>(UnityEngine.PlayerPrefs.GetString("SHIFT.UMP.DebugDevices", "")
                        .Split(new[] { ',', ';', ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                };
            }
#endif
        }
        public void Update(Action<bool> completed) => ConsentInformation.Update(parameters,
            error => MobileAdsEventExecutor.ExecuteInUpdate(() => completed(error == null)));
        public void LoadAndShowIfRequired(Action<bool> completed) => ConsentForm.LoadAndShowConsentFormIfRequired(
            error => MobileAdsEventExecutor.ExecuteInUpdate(() => completed(error == null)));
        public void ShowPrivacyOptions(Action<bool> completed) => ConsentForm.ShowPrivacyOptionsForm(
            error => MobileAdsEventExecutor.ExecuteInUpdate(() => completed(error == null)));
    }
}
