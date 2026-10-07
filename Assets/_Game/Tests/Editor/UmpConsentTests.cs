using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Shift.Game.Tests
{
    public sealed class UmpConsentTests
    {
        private sealed class Ump : IUmpConsentClient
        {
            public bool CanRequestAds { get; set; }
            public bool IsPrivacyOptionsRequired { get; set; }
            public Action<bool> Updated, Form, Privacy;
            public int Updates, Forms, PrivacyShows;
            public void Update(Action<bool> done) { Updates++; Updated = done; }
            public void LoadAndShowIfRequired(Action<bool> done) { Forms++; Form = done; }
            public void ShowPrivacyOptions(Action<bool> done) { PrivacyShows++; Privacy = done; }
        }
        private sealed class Ad : IRewardedAdHandle
        {
            public bool CanShow => Disposals == 0;
            public int Disposals, Shows;
            public Action Earn, Close, Fail;
            public void Show(Action earned, Action closed, Action failed) { Shows++; Earn = earned; Close = closed; Fail = failed; }
            public void Dispose() { Disposals++; }
        }
        private sealed class Ads : IRewardedAdClient
        {
            public int Initializations, Loads;
            public Action<bool> Initialized;
            public Action<IRewardedAdHandle> Loaded;
            public void Initialize(Action<bool> done) { Initializations++; Initialized = done; }
            public void Load(string unit, Action<IRewardedAdHandle> done) { Loads++; Loaded = done; }
        }
        private Ump ump;
        private Ads ads;
        private GoogleUmpConsentGate gate;
        private ConsentedRewardedAds session;
        private double now;
        private const string Prefix = "SHIFT.Tests.Ump.";
        [SetUp] public void Setup() => Create();
        private void Create()
        {
            now = 0; ump = new Ump(); ads = new Ads(); gate = new GoogleUmpConsentGate(ump, () => now);
            session = new ConsentedRewardedAds(gate, ads, "mock-unit", () => now);
        }
        [TearDown] public void Cleanup()
        {
            session?.Dispose(); new SaveService(Prefix).Reset(); new SettingsService(Prefix + "Settings.").Reset();
            PlayerPrefs.DeleteKey(Prefix + "Daily.v1.History");
        }
        private Ad Ready()
        {
            session.Start(); ump.Updated(true); ump.CanRequestAds = true; ump.Form(true);
            ads.Initialized(true); session.Tick(); var ad = new Ad(); ads.Loaded(ad); return ad;
        }
        [Test] public void UndoRequiresCurrentConsentAndRevokedConsentCannotReward()
        {
            session.Start(); bool? result = null;
            session.ShowRewardedAd(RewardReason.Undo, value => result = value);
            Assert.That(result, Is.False); Assert.That(ads.Initializations, Is.Zero);
            ump.Updated(true); ump.CanRequestAds = true; ump.Form(true); ads.Initialized(true); session.Tick();
            var ad = new Ad(); ads.Loaded(ad); var results = new List<bool>();
            session.ShowRewardedAd(RewardReason.Undo, results.Add);
            ump.CanRequestAds = false; session.Tick(); ad.Earn(); ad.Close();
            Assert.That(results, Is.EqualTo(new[] { false }));
        }
        [Test] public void UnresolvedAndFalsePermissionNeverInitializeOrShow()
        {
            session.Start(); session.Tick(); bool? result = null; session.ShowRewardedAd(RewardReason.Hint, x => result = x);
            Assert.That(result, Is.False); Assert.That(ads.Initializations, Is.Zero);
            ump.Updated(true); ump.Form(true); session.Tick();
            Assert.That(gate.CanRequestAds, Is.False); Assert.That(ads.Initializations, Is.Zero); Assert.That(ads.Loads, Is.Zero);
        }
        [Test] public void CachedTrueDoesNotBypassCurrentUpdateAndForm()
        {
            ump.CanRequestAds = true; session.Start(); session.Tick(); Assert.That(ads.Initializations, Is.Zero);
            ump.Updated(true); session.Tick(); Assert.That(ads.Initializations, Is.Zero);
            ump.Form(true); Assert.That(ads.Initializations, Is.EqualTo(1));
        }
        [Test] public void RepeatedConsentCallbacksInitializeExactlyOnce()
        {
            Ready(); ump.Updated(true); ump.Form(true); session.Start(); session.Tick();
            Assert.That(ump.Updates, Is.EqualTo(1)); Assert.That(ump.Forms, Is.EqualTo(1));
            Assert.That(ads.Initializations, Is.EqualTo(1)); Assert.That(ads.Loads, Is.EqualTo(1));
        }
        [TestCase(false)] [TestCase(true)]
        public void UpdateOrFormFailureBlocksEvenWithPreviousEligibility(bool formFailure)
        {
            ump.CanRequestAds = true; session.Start(); ump.Updated(formFailure);
            if (formFailure) ump.Form(false);
            session.Tick(); Assert.That(gate.CanRequestAds, Is.False); Assert.That(ads.Initializations, Is.Zero);
            Assert.That(session.IsRewardedAdAvailable, Is.False);
        }
        [Test] public void UpdateTimeoutRejectsLateCallbackAndCanBeExplicitlyRetried()
        {
            session.Start(); var late = ump.Updated; now = 31; session.Tick();
            Assert.That(gate.Status, Is.EqualTo("update_timeout")); ump.CanRequestAds = true; late(true);
            Assert.That(ads.Initializations, Is.Zero); gate.Prepare(_ => { }); ump.Updated(true); ump.Form(true);
            Assert.That(ads.Initializations, Is.EqualTo(1));
        }
        [Test] public void SlowUserFormDoesNotBlockGameOrGrantPermission()
        {
            session.Start(); ump.Updated(true); now = 10000; session.Tick();
            Assert.That(gate.IsBusy, Is.True); Assert.That(ads.Initializations, Is.Zero);
            ump.CanRequestAds = true; ump.Form(true); Assert.That(ads.Initializations, Is.EqualTo(1));
        }
        [Test] public void PrivacyEntryTracksRequirementAndAbsentOptionsFailCalmly()
        {
            bool? result = null; session.ShowPrivacyOptions(x => result = x);
            Assert.That(result, Is.False); Assert.That(ump.PrivacyShows, Is.Zero);
            ump.IsPrivacyOptionsRequired = true; Assert.That(session.IsPrivacyOptionsRequired, Is.True);
            ump.IsPrivacyOptionsRequired = false; Assert.That(session.IsPrivacyOptionsRequired, Is.False);
        }
        [TestCase(false)] [TestCase(true)]
        public void PrivacyChangeInvalidatesCachedAdAndRechecksPermission(bool allowed)
        {
            var old = Ready(); ump.IsPrivacyOptionsRequired = true;
            session.ShowPrivacyOptions(_ => { }); session.ShowPrivacyOptions(_ => { });
            Assert.That(old.Disposals, Is.EqualTo(1)); Assert.That(session.IsRewardedAdAvailable, Is.False);
            Assert.That(ump.PrivacyShows, Is.EqualTo(1));
            ump.CanRequestAds = allowed; ump.Privacy(true); session.Tick();
            Assert.That(ads.Initializations, Is.EqualTo(1)); Assert.That(ads.Loads, Is.EqualTo(allowed ? 2 : 1));
        }
        [Test] public void PrivacyFailureKeepsAdsBlockedAndCanRetryThroughInternalApi()
        {
            Ready(); ump.IsPrivacyOptionsRequired = true; session.ShowPrivacyOptions(_ => { }); ump.Privacy(false);
            session.Tick(); Assert.That(session.IsRewardedAdAvailable, Is.False); Assert.That(ads.Loads, Is.EqualTo(1));
            session.ShowPrivacyOptions(_ => { }); ump.Privacy(true); session.Tick();
            Assert.That(ads.Loads, Is.EqualTo(2)); Assert.That(ads.Initializations, Is.EqualTo(1));
        }
        [Test] public void LateAdLoadCannotSurvivePrivacyChange()
        {
            session.Start(); ump.Updated(true); ump.CanRequestAds = true; ump.Form(true); ads.Initialized(true); session.Tick();
            var late = ads.Loaded; ump.IsPrivacyOptionsRequired = true; session.ShowPrivacyOptions(_ => { });
            var ad = new Ad(); late(ad); Assert.That(ad.Disposals, Is.EqualTo(1));
            ump.CanRequestAds = false; ump.Privacy(true); session.Tick(); Assert.That(ads.Loads, Is.EqualTo(1));
        }
        [Test] public void PrivacyDuringInitializationNeverInitializesTwice()
        {
            session.Start(); ump.Updated(true); ump.CanRequestAds = true; ump.Form(true);
            ump.IsPrivacyOptionsRequired = true; session.ShowPrivacyOptions(_ => { }); ads.Initialized(true); session.Tick();
            Assert.That(ads.Loads, Is.Zero); ump.Privacy(true); session.Tick();
            Assert.That(ads.Initializations, Is.EqualTo(1)); Assert.That(ads.Loads, Is.EqualTo(1));
        }
        [Test] public void DirectEligibilityRevocationBlocksShowAndPreload()
        {
            var ad = Ready(); ump.CanRequestAds = false;
            Assert.That(session.IsRewardedAdAvailable, Is.False); bool? result = null;
            session.ShowRewardedAd(RewardReason.Hint, x => result = x); session.Tick();
            Assert.That(result, Is.False); Assert.That(ad.Shows, Is.Zero); Assert.That(ad.Disposals, Is.EqualTo(1));
            now += 4000; session.Tick(); Assert.That(ads.Loads, Is.EqualTo(1));
        }
        [TestCase(false)] [TestCase(true)]
        public void ConsentDoesNotChangeRewardSafety(bool earned)
        {
            var ad = Ready(); var hints = new HintSession(); var level = LevelValidation.AllLevels()[0];
            for (int i = 0; i < 3; i++) hints.TryUse(level, out _);
            session.ShowRewardedAd(RewardReason.Hint, ok => { if (ok) hints.GrantReward(); });
            session.Pause(true); session.Pause(false); Assert.That(hints.Remaining, Is.Zero);
            if (earned) { ad.Earn(); ad.Earn(); }
            ad.Close(); ad.Earn(); ad.Fail(); Assert.That(hints.Remaining, Is.EqualTo(earned ? 1 : 0));
        }
        [Test] public void PrivacyCannotOverlapAnActiveRewardedAd()
        {
            var ad = Ready(); ump.IsPrivacyOptionsRequired = true; session.ShowRewardedAd(RewardReason.Hint, _ => { });
            session.ShowPrivacyOptions(_ => { }); Assert.That(ump.PrivacyShows, Is.Zero);
            ad.Close(); session.ShowPrivacyOptions(_ => { }); Assert.That(ump.PrivacyShows, Is.EqualTo(1));
        }
        [Test] public void DisposalBlocksLateConsentAndNativeCallbacks()
        {
            session.Start(); session.Dispose(); ump.CanRequestAds = true; ump.Updated(true);
            Assert.That(ads.Initializations, Is.Zero); Assert.That(gate.CanRequestAds, Is.False);
        }
        [TestCase(AdPlatform.Android)] [TestCase(AdPlatform.Ios)]
        public void CentralProductionIdsAndBuildSyncResolveSamePlatform(AdPlatform platform)
        {
            var config = Resources.Load<AdConfiguration>("AdConfiguration");
            Assert.That(config.TryResolve(platform, false, false, out var unit, out _), Is.True);
            Assert.That(unit, Is.EqualTo(platform == AdPlatform.Android ? config.AndroidRewardedAdUnitId : config.IosRewardedAdUnitId));
            Assert.That(AdMobBuildConfiguration.ResolveAppId(config, platform, false), Is.EqualTo(platform == AdPlatform.Android ? config.AndroidAppId : config.IosAppId));
            Assert.That(config.ConsentProviderConfigured, Is.True);
            Assert.That(config.EnableConsentDebug, Is.False); Assert.That(config.UseTestAdsInDevelopment, Is.True);
        }
        [TestCase(AdPlatform.Android)] [TestCase(AdPlatform.Ios)]
        public void DevelopmentAndEditorIgnoreConfiguredLiveInventory(AdPlatform platform)
        {
            var config = Resources.Load<AdConfiguration>("AdConfiguration");
            foreach (bool editor in new[] { false, true })
            {
                Assert.That(config.TryResolve(platform, editor, !editor, out var unit, out _), Is.True);
                Assert.That(unit, Is.EqualTo(platform == AdPlatform.Android ? AdConfiguration.AndroidTestRewardedId : AdConfiguration.IosTestRewardedId));
            }
            Assert.That(AdMobBuildConfiguration.ResolveAppId(config, platform, true), Is.EqualTo(platform == AdPlatform.Android ? AdConfiguration.AndroidTestAppId : AdConfiguration.IosTestAppId));
        }
        [TestCase("missing-app")] [TestCase("missing-unit")] [TestCase("malformed-app")]
        [TestCase("malformed-unit")] [TestCase("swapped")] [TestCase("publisher")]
        [TestCase("sample")] [TestCase("consent-provider")]
        public void ReleaseBuildRejectsInvalidConfiguration(string problem)
        {
            var config = Object.Instantiate(Resources.Load<AdConfiguration>("AdConfiguration"));
            try
            {
                switch (problem)
                {
                    case "missing-app": config.AndroidAppId = ""; break;
                    case "missing-unit": config.AndroidRewardedAdUnitId = ""; break;
                    case "malformed-app": config.AndroidAppId = "invalid"; break;
                    case "malformed-unit": config.AndroidRewardedAdUnitId += "\n"; break;
                    case "swapped": config.AndroidAppId = config.AndroidRewardedAdUnitId; break;
                    case "publisher": config.AndroidAppId = "ca-app-pub-1111111111111111~1111111111"; break;
                    case "sample": config.AndroidAppId = AdConfiguration.AndroidTestAppId; config.AndroidRewardedAdUnitId = AdConfiguration.AndroidTestRewardedId; break;
                    case "consent-provider": config.ConsentProviderConfigured = false; break;
                }
                Assert.That(config.TryResolve(AdPlatform.Android, false, false, out var unit, out _), Is.False); Assert.That(unit, Is.Empty);
                Assert.Throws<BuildFailedException>(() => AdMobBuildConfiguration.ResolveAppId(config, AdPlatform.Android, false));
            }
            finally { Object.DestroyImmediate(config); }
        }
        [Test] public void UnsupportedPlatformFailsClosed()
        {
            var config = Resources.Load<AdConfiguration>("AdConfiguration");
            Assert.That(config.TryResolve(AdPlatform.Unsupported, false, false, out _, out _), Is.False);
        }
        [Test] public void ProtectedGameplayMatchesPreConsentBaseline()
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            foreach (var line in File.ReadAllLines(Path.Combine(Application.dataPath, "_Game/Tests/Editor/UmpProtectedFiles.txt")))
            {
                var parts = line.Split('|');
                Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath, parts[0])))).Replace("-", ""), Is.EqualTo(parts[1]), parts[0]);
            }
        }
        [UnityTest] public IEnumerator ConsentPrivacySettingsCampaignDailyAndCaptures()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode(); yield return new WaitForSecondsRealtime(.35f); Create();
            var game = Object.FindFirstObjectByType<PrototypeGame>(); game.LocalNow = () => new DateTime(2026, 10, 5); game.UnlockAllLevels(); game.SelectLevel(35); game.RewardedAds = session;
            session.Start(); yield return new WaitForSecondsRealtime(.35f); Capture("07-campaign");
            game.OpenHint(); game.OpenHint(); game.OpenHint(); game.SelectLevel(34); yield return new WaitForSecondsRealtime(.35f); game.OpenHint();
            Assert.That(GameObject.Find("Watch Ad").GetComponent<Button>().interactable, Is.False); Capture("02-before-consent");
            Assert.That(ads.Initializations, Is.Zero); ump.Updated(true); ump.CanRequestAds = true; ump.IsPrivacyOptionsRequired = true;
            ump.Form(true); ads.Initialized(true); session.Tick(); var ad = new Ad(); ads.Loaded(ad);
            yield return new WaitForSecondsRealtime(.1f); Assert.That(GameObject.Find("Watch Ad").GetComponent<Button>().interactable, Is.True); Capture("01-ready-after-consent");
            game.CloseHint(); game.SelectLevel(36); yield return new WaitForSecondsRealtime(.35f);
            game.OpenHint(); game.RequestRewardedHint(); game.RequestRewardedHint(); Assert.That(ad.Shows, Is.EqualTo(1));
            ad.Earn(); ad.Earn(); ad.Close(); Assert.That(game.Hints.Remaining, Is.Zero); Assert.That(game.Hints.Stage(game.CurrentLevel), Is.EqualTo(1)); Capture("03-earned-hint");
            game.OpenSettings(); yield return new WaitForSecondsRealtime(.1f);
            AssertPrivacyControls(true); Capture("04-settings-required-en");
            // The conditional Settings action reuses the existing privacy-options capability.
            GameObject.Find("Privacy Choices").GetComponent<Button>().onClick.Invoke();
            Assert.That(ump.PrivacyShows, Is.EqualTo(1)); ump.CanRequestAds = false; ump.Privacy(true); session.Tick(); game.CloseSettings();
            game.DailyLanguage = HintLanguage.Turkish; game.OpenHint(); yield return new WaitForSecondsRealtime(.1f);
            Assert.That(GameObject.Find("Hint Availability").GetComponent<Text>().text, Is.EqualTo("Reklam henüz hazır değil.")); Capture("05-unavailable-tr");
            game.CloseHint(); game.OpenSettings(); yield return new WaitForSecondsRealtime(.1f);
            AssertPrivacyControls(true); Capture("08-settings-required-tr");
            session.ShowPrivacyOptions(_ => { }); ump.CanRequestAds = true; ump.Privacy(true); session.Tick();
            ad = new Ad(); ads.Loaded(ad); game.CloseSettings(); game.DailyLanguage = HintLanguage.English;
            game.LocalNow = () => new DateTime(2026, 10, 5); game.StartDaily(game.LocalNow()); yield return new WaitForSecondsRealtime(.35f);
            Assert.That(game.RewardedAds, Is.SameAs(session)); game.OpenHint(); Capture("06-daily");
            game.RequestRewardedHint(); ad.Earn(); ad.Close(); Assert.That(game.Hints.Stage(game.CurrentLevel), Is.EqualTo(1));
            Assert.That(ads.Initializations, Is.EqualTo(1));
            game.OpenSettings(); ump.IsPrivacyOptionsRequired = false; yield return new WaitForSecondsRealtime(.1f);
            AssertPrivacyControls(false);
            yield return new ExitPlayMode();
        }
        private static void AssertPrivacyControls(bool required)
        {
            var settings = Object.FindFirstObjectByType<SettingsPanel>();
            Assert.That(settings.transform.Find("Privacy Choices").gameObject.activeSelf, Is.EqualTo(required));
            Assert.That(settings.GetComponentsInChildren<Button>().Length, Is.EqualTo(required ? 7 : 6));
        }
        private static void Capture(string name)
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "../Validation/ump-ui")); SprintPresentationTests.Capture("ump-ui/" + name + ".png");
        }
    }
}
