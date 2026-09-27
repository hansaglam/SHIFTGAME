using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Shift.Game.Tests
{
    public sealed class AdMobRewardedTests
    {
        private sealed class Ad : IRewardedAdHandle
        {
            public bool CanShow { get; set; } = true;
            public Action Earn, Close, Fail;
            public int Shows, Disposals;
            public bool Throw;
            public void Show(Action earned, Action closed, Action failed)
            { Shows++; Earn = earned; Close = closed; Fail = failed; if (Throw) throw new InvalidOperationException(); }
            public void Dispose() { Disposals++; }
        }
        private sealed class Client : IRewardedAdClient
        {
            public int Starts, Loads;
            public Action<bool> Init;
            public Action<IRewardedAdHandle> Loaded;
            public void Initialize(Action<bool> completed) { Starts++; Init = completed; }
            public void Load(string unit, Action<IRewardedAdHandle> completed) { Loads++; Loaded = completed; }
        }
        private double time;
        private Client client;
        private AdMobRewardedAdService service;
        [SetUp] public void Setup() { time = 0; client = new Client(); service = new AdMobRewardedAdService(client, "mock", () => time); }
        [TearDown] public void Cleanup()
        {
            service?.Dispose(); new SaveService(Prefix).Reset(); new SettingsService(Prefix + "Settings.").Reset();
            PlayerPrefs.DeleteKey(Prefix + "Daily.v1.History");
        }
        private Ad Ready()
        {
            service.Start(); client.Init(true); service.Tick();
            var ad = new Ad(); client.Loaded(ad); return ad;
        }
        [Test] public void SameProviderRoutesHintAndUndoAndRejectsDuplicateEarnedCallbacks()
        {
            var ad = Ready();
            foreach (var reason in new[] { RewardReason.Hint, RewardReason.Undo })
            {
                var result = new List<bool>(); service.ShowRewardedAd(reason, result.Add);
                ad.Earn(); ad.Earn(); ad.Close(); ad.Fail();
                Assert.That(result, Is.EqualTo(new[] { true }));
                time += 2; service.Tick(); ad = new Ad(); client.Loaded(ad);
            }
            Assert.That(client.Starts, Is.EqualTo(1));
        }
        [Test] public void UndoDismissalNeverRewardsEvenWithLateEarnedCallback()
        {
            var ad = Ready(); var result = new List<bool>(); service.ShowRewardedAd(RewardReason.Undo, result.Add);
            ad.Close(); ad.Earn(); Assert.That(result, Is.EqualTo(new[] { false }));
        }
        [Test] public void InitializationIsOnceAndAvailabilityRequiresLoad()
        {
            Assert.That(service.IsRewardedAdAvailable, Is.False);
            service.Start(); service.Start(); Assert.That(client.Starts, Is.EqualTo(1));
            client.Init(true); client.Init(true); service.Tick(); Assert.That(client.Loads, Is.EqualTo(1));
            Assert.That(service.IsRewardedAdAvailable, Is.False);
            client.Loaded(new Ad()); Assert.That(service.IsRewardedAdAvailable, Is.True);
        }
        [Test] public void RewardAndCloseCannotReportTwiceOrDoubleShow()
        {
            var ad = Ready(); var result = new List<bool>();
            service.ShowRewardedAd(RewardReason.Hint, result.Add);
            Assert.That(service.IsRewardedAdAvailable, Is.False);
            bool? second = null; service.ShowRewardedAd(RewardReason.Hint, b => second = b);
            Assert.That(second, Is.False); Assert.That(ad.Shows, Is.EqualTo(1));
            ad.Earn(); ad.Earn(); Assert.That(result, Is.EqualTo(new[] { true }));
            Assert.That(service.IsRewardedAdAvailable, Is.False);
            ad.Close(); ad.Close(); ad.Fail(); Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(ad.Disposals, Is.EqualTo(1)); time = 1; service.Tick(); Assert.That(client.Loads, Is.EqualTo(2));
        }
        [TestCase(false)] [TestCase(true)]
        public void DismissalOrFailureWithoutRewardReturnsFalse(bool failure)
        {
            var ad = Ready(); var result = new List<bool>(); service.ShowRewardedAd(RewardReason.Hint, result.Add);
            if (failure) ad.Fail(); else ad.Close(); ad.Earn();
            Assert.That(result, Is.EqualTo(new[] { false })); Assert.That(ad.Disposals, Is.EqualTo(1));
            time = 1; service.Tick(); Assert.That(client.Loads, Is.EqualTo(2));
        }
        [Test] public void ShowExceptionFailsAndReloads()
        {
            var ad = Ready(); ad.Throw = true; bool? result = null;
            Assert.DoesNotThrow(() => service.ShowRewardedAd(RewardReason.Hint, b => result = b));
            Assert.That(result, Is.False); time = 1; service.Tick(); Assert.That(client.Loads, Is.EqualTo(2));
        }
        [Test] public void LoadFailureRetriesAreDelayedAndBounded()
        {
            service.Start(); client.Init(true); service.Tick();
            for (int i = 0; i < 4; i++)
            {
                client.Loaded(null); service.Tick(); Assert.That(client.Loads, Is.EqualTo(i + 1));
                time += 61; service.Tick();
            }
            time += 100000; service.Tick(); Assert.That(client.Loads, Is.EqualTo(4)); Assert.That(service.IsRewardedAdAvailable, Is.False);
        }
        [Test] public void LateLoadAfterTimeoutIsDisposed()
        {
            service.Start(); client.Init(true); service.Tick(); var late = client.Loaded;
            time = 31; service.Tick(); var ad = new Ad(); late(ad);
            Assert.That(ad.Disposals, Is.EqualTo(1)); Assert.That(service.IsRewardedAdAvailable, Is.False);
        }
        [TestCase(false)] [TestCase(true)]
        public void InitializationFailureOrTimeoutLeavesGameUnavailable(bool timeout)
        {
            service.Start(); if (timeout) { time = 31; service.Tick(); client.Init(true); } else client.Init(false);
            service.Start(); service.Tick(); Assert.That(client.Starts, Is.EqualTo(1)); Assert.That(client.Loads, Is.Zero);
            Assert.That(service.IsRewardedAdAvailable, Is.False);
        }
        [Test] public void DisposalInvalidatesLateRewardAndPendingLoad()
        {
            var ad = Ready(); var result = new List<bool>(); service.ShowRewardedAd(RewardReason.Hint, result.Add);
            service.Dispose(); service.Dispose(); ad.Earn(); ad.Close();
            Assert.That(result, Is.EqualTo(new[] { false })); Assert.That(ad.Disposals, Is.EqualTo(1));
            var other = new AdMobRewardedAdService(client, "mock", () => time);
            other.Start(); client.Init(true); other.Tick(); other.Dispose(); var late = new Ad(); client.Loaded(late);
            Assert.That(late.Disposals, Is.EqualTo(1));
        }
        [Test] public void PauseResumeDoesNotCreateRewardAndExpiredAdIsReplaced()
        {
            var ad = Ready(); service.Pause(true); Assert.That(service.IsRewardedAdAvailable, Is.False);
            service.Pause(false); Assert.That(service.IsRewardedAdAvailable, Is.True);
            time = 3301; Assert.That(service.IsRewardedAdAvailable, Is.False); service.Tick();
            Assert.That(ad.Disposals, Is.EqualTo(1)); Assert.That(client.Loads, Is.EqualTo(2));
        }
        [Test] public void ConfirmedRewardGrantsExactlyOneHintBeforeConsumption()
        {
            var hints = new HintSession(); var level = LevelValidation.AllLevels()[0];
            for (int i = 0; i < 3; i++) hints.TryUse(level, out _);
            var ad = Ready(); service.ShowRewardedAd(RewardReason.Hint, ok => { if (ok) hints.GrantReward(); });
            ad.Earn(); ad.Earn(); ad.Close(); Assert.That(hints.Remaining, Is.EqualTo(1));
        }
        [TestCase(AdPlatform.Android, false, false, "ca-app-pub-1111111111111111/1111111111")]
        [TestCase(AdPlatform.Ios, false, false, "ca-app-pub-1111111111111111/2222222222")]
        [TestCase(AdPlatform.Android, true, false, AdConfiguration.AndroidTestRewardedId)]
        [TestCase(AdPlatform.Ios, false, true, AdConfiguration.IosTestRewardedId)]
        public void ConfigSelectsPlatformAndOnlyTestIdsInDevelopment(AdPlatform platform, bool editor, bool development, string expected)
        {
            var config = ScriptableObject.CreateInstance<AdConfiguration>();
            try
            {
                config.AndroidAppId = "ca-app-pub-1111111111111111~1111111111"; config.IosAppId = "ca-app-pub-1111111111111111~2222222222";
                config.AndroidRewardedAdUnitId = "ca-app-pub-1111111111111111/1111111111"; config.IosRewardedAdUnitId = "ca-app-pub-1111111111111111/2222222222"; config.ProductionConsentReady = true;
                Assert.That(config.TryResolve(platform, editor, development, out var unit, out _), Is.True); Assert.That(unit, Is.EqualTo(expected));
            }
            finally { Object.DestroyImmediate(config); }
        }
        [Test] public void MissingIdsAndConsentAreClosedByDefault()
        {
            var config = ScriptableObject.CreateInstance<AdConfiguration>();
            try
            {
                Assert.That(config.TryResolve(AdPlatform.Android, false, false, out _, out var reason), Is.False);
                Assert.That(reason, Is.EqualTo("missing_production_ids"));
                config.AndroidAppId = "ca-app-pub-1111111111111111~1111111111"; config.AndroidRewardedAdUnitId = "ca-app-pub-1111111111111111/1111111111";
                Assert.That(config.TryResolve(AdPlatform.Android, false, false, out _, out reason), Is.False);
                Assert.That(reason, Is.EqualTo("consent_not_configured"));
                config.UseTestAdsInDevelopment = false;
                Assert.That(config.TryResolve(AdPlatform.Android, false, true, out _, out _), Is.False);
                Assert.That(config.TryResolve(AdPlatform.Android, true, false, out var unit, out _), Is.True);
                Assert.That(unit, Is.EqualTo(AdConfiguration.AndroidTestRewardedId));
            }
            finally { Object.DestroyImmediate(config); }
        }
        [Test] public void ProtectedGameplayAndExistingProjectFilesAreUnchanged()
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            foreach (string line in File.ReadAllLines(Path.Combine(Application.dataPath, "_Game/Tests/Editor/AdMobProtectedFiles.txt")))
            {
                var pair = line.Split('|');
                Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath, pair[0])))).Replace("-", ""), Is.EqualTo(pair[1]), pair[0]);
            }
        }
        private const string Prefix = "SHIFT.Tests.AdMob.";
        [UnityTest] public IEnumerator CampaignAndDailyModalUseSameProviderWithActualCaptures()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode(); yield return new WaitForSecondsRealtime(.35f);
            time = 0; client = new Client(); service = new AdMobRewardedAdService(client, "mock", () => time);
            var game = Object.FindFirstObjectByType<PrototypeGame>(); game.LocalNow = () => new DateTime(2026, 10, 5); game.UnlockAllLevels(); game.SelectLevel(35);
            game.RewardedAds = service; service.Start(); client.Init(true); service.Tick();
            yield return new WaitForSecondsRealtime(.35f); Capture("06-normal-gameplay");
            game.OpenHint(); game.OpenHint(); game.OpenHint(); game.SelectLevel(34); yield return new WaitForSecondsRealtime(.35f); game.OpenHint();
            Assert.That(game.Hints.Remaining, Is.Zero);
            Assert.That(GameObject.Find("Watch Ad").GetComponent<Button>().interactable, Is.False);
            Assert.That(GameObject.Find("Hint Availability").GetComponent<Text>().text, Is.EqualTo("Ad not ready yet.")); Capture("02-unavailable");
            var ad = new Ad(); client.Loaded(ad); yield return new WaitForSecondsRealtime(.1f);
            Assert.That(GameObject.Find("Watch Ad").GetComponent<Button>().interactable, Is.True); Capture("01-available-simulated");
            game.RequestRewardedHint(); game.RequestRewardedHint(); Assert.That(ad.Shows, Is.EqualTo(1));
            ad.Fail(); Assert.That(game.Hints.Remaining, Is.Zero); Capture("04-no-reward");
            time += 2; service.Tick(); ad = new Ad(); client.Loaded(ad); yield return new WaitForSecondsRealtime(.1f);
            game.CloseHint(); game.SelectLevel(36); yield return new WaitForSecondsRealtime(.35f);
            game.OpenHint(); game.RequestRewardedHint(); ad.Earn(); ad.Earn(); ad.Close();
            Assert.That(game.Hints.Remaining, Is.Zero); Assert.That(game.Hints.Stage(game.CurrentLevel), Is.EqualTo(1));
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text, Does.StartWith("Hint 1/3")); Capture("03-earned-hint-revealed");
            time += 2; service.Tick(); ad = new Ad(); client.Loaded(ad);
            game.LocalNow = () => new DateTime(2026, 10, 5); game.StartDaily(game.LocalNow()); yield return new WaitForSecondsRealtime(.35f);
            Assert.That(game.RewardedAds, Is.SameAs(service)); game.OpenHint(); Capture("05-daily-zero-hints");
            game.RequestRewardedHint(); ad.Close(); ad.Earn(); Assert.That(game.Hints.Stage(game.CurrentLevel), Is.Zero);
            time += 2; service.Tick(); ad = new Ad(); client.Loaded(ad); yield return new WaitForSecondsRealtime(.1f);
            game.RequestRewardedHint(); ad.Earn(); ad.Close(); Assert.That(game.Hints.Stage(game.CurrentLevel), Is.EqualTo(1));
            time += 2; service.Tick(); ad = new Ad(); client.Loaded(ad); game.OpenHint(); game.RequestRewardedHint();
            game.Restart(); ad.Earn(); ad.Close(); Assert.That(game.Hints.Remaining, Is.Zero); Assert.That(game.Hints.Stage(game.CurrentLevel), Is.EqualTo(1));
            yield return null; game.DailyLanguage = HintLanguage.Turkish; game.OpenHint(); yield return new WaitForSecondsRealtime(.1f);
            Assert.That(GameObject.Find("Hint Availability").GetComponent<Text>().text, Is.EqualTo("Reklam henüz hazır değil.")); Capture("07-unavailable-tr");
            yield return new ExitPlayMode();
        }
        private static void Capture(string name)
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "../Validation/admob"));
            SprintPresentationTests.Capture("admob/" + name + ".png");
        }
    }
}
