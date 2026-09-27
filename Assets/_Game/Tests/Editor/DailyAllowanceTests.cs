using System;
using System.Collections;
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
    public sealed class DailyAllowanceTests
    {
        private const string Key = "SHIFT.Tests.Allowance.Unit";
        private const string Prefix = "SHIFT.Tests.Allowance.UI.";
        private DailyAllowanceConfig config;
        private DateTime now;
        [SetUp] public void Setup()
        {
            config = ScriptableObject.CreateInstance<DailyAllowanceConfig>();
            now = new DateTime(2026, 10, 5, 23, 59, 0);
            PlayerPrefs.DeleteKey(Key);
        }
        [TearDown] public void Cleanup() { Object.DestroyImmediate(config); PlayerPrefs.DeleteKey(Key); }
        private DailyAllowanceService Store() => new DailyAllowanceService(config, () => now, Key);

        [Test] public void DefaultsAndCentralResourceAgree()
        {
            var asset = Resources.Load<DailyAllowanceConfig>("DailyAllowanceConfig");
            foreach (var c in new[] {config, asset})
            {
                Assert.That(c.DailyFreeHints, Is.EqualTo(3)); Assert.That(c.DailyFreeUndos, Is.EqualTo(5));
                Assert.That(c.RewardedHintAmount, Is.EqualTo(1)); Assert.That(c.RewardedUndoAmount, Is.EqualTo(1));
            }
        }
        [Test] public void ConfigurableFreeAmountsAndRewardsAreUsed()
        {
            config.DailyFreeHints = 7; config.DailyFreeUndos = 9; config.RewardedHintAmount = 2; config.RewardedUndoAmount = 3;
            var s = Store(); s.Grant(RewardReason.Hint, s.DateKey); s.Grant(RewardReason.Undo, s.DateKey);
            Assert.That(s.HintsRemaining, Is.EqualTo(9)); Assert.That(s.UndosRemaining, Is.EqualTo(12));
        }
        [Test] public void FreshStoreAndSameDayReloadPersistBothBalances()
        {
            var s = Store(); Assert.That(s.DateKey, Is.EqualTo(DailyPool.Key(now)));
            Assert.That(s.HintsRemaining, Is.EqualTo(3)); Assert.That(s.UndosRemaining, Is.EqualTo(5));
            s.TryConsumeHint(); s.TryUndo(() => true);
            var loaded = Store(); Assert.That(loaded.HintsRemaining, Is.EqualTo(2)); Assert.That(loaded.UndosRemaining, Is.EqualTo(4));
            loaded.TryConsumeHint(); Assert.That(s.HintsRemaining, Is.EqualTo(1), "Old scene service must observe current persisted state");
        }
        [TestCase(1)] [TestCase(-1)] [TestCase(30)]
        public void CalendarChangeResetsWithoutCarryover(int days)
        {
            var s = Store(); s.TryConsumeHint(); s.TryUndo(() => true); now = now.AddDays(days);
            Assert.That(s.Refresh(), Is.True); Assert.That(s.HintsRemaining, Is.EqualTo(3)); Assert.That(s.UndosRemaining, Is.EqualTo(5));
            Assert.That(s.Refresh(), Is.False);
        }
        [Test] public void RewardedCreditsExpireAndOldDayCallbacksCannotGrant()
        {
            var s = Store(); string date = s.DateKey; s.Grant(RewardReason.Hint, date); s.Grant(RewardReason.Undo, date);
            now = now.AddMinutes(2); Assert.That(s.Grant(RewardReason.Undo, date), Is.False);
            Assert.That(s.Grant(RewardReason.Hint, date), Is.False);
            Assert.That(s.HintsRemaining, Is.EqualTo(3)); Assert.That(s.UndosRemaining, Is.EqualTo(5));
        }
        [TestCase("")] [TestCase("broken")] [TestCase("{}")] [TestCase("null")]
        [TestCase("{\"version\":1,\"dateKey\":\"2026-10-05\",\"hintsRemaining\":-2,\"undosRemaining\":4}")]
        [TestCase("{\"version\":1,\"dateKey\":\"2026-10-05\"}")]
        public void MalformedSaveResetsSafely(string json)
        {
            PlayerPrefs.SetString(Key, json); var s = Store();
            Assert.That(s.HintsRemaining, Is.EqualTo(3)); Assert.That(s.UndosRemaining, Is.EqualTo(5));
        }
        [Test] public void RejectedUndoNeverSpendsAndBalancesNeverGoNegative()
        {
            var s = Store(); Assert.That(s.TryUndo(() => false), Is.False); Assert.That(s.TryUndo(null), Is.False);
            Assert.That(s.UndosRemaining, Is.EqualTo(5));
            for (int i = 0; i < 5; i++) Assert.That(s.TryUndo(() => true), Is.True);
            bool called = false; Assert.That(s.TryUndo(() => called = true), Is.False); Assert.That(called, Is.False);
            for (int i = 0; i < 3; i++) Assert.That(s.TryConsumeHint(), Is.True);
            Assert.That(s.TryConsumeHint(), Is.False); Assert.That(s.HintsRemaining, Is.Zero); Assert.That(s.UndosRemaining, Is.Zero);
        }
        [TestCase(RewardReason.Hint)] [TestCase(RewardReason.Undo)]
        public void DisabledRecoveryCannotGrant(RewardReason reason)
        {
            var s = Store(); config.EnableRewardedHintRecovery = false; config.EnableRewardedUndoRecovery = false;
            Assert.That(s.Grant(reason, s.DateKey), Is.False);
            Assert.That(s.HintsRemaining, Is.EqualTo(3)); Assert.That(s.UndosRemaining, Is.EqualTo(5));
        }
        [Test] public void StageExhaustionAndMidnightNeverResetAuthoredKnowledge()
        {
            var s = Store(); var hints = new HintSession(s); var level = LevelValidation.AllLevels()[9];
            for (int i = 1; i <= 3; i++) { Assert.That(hints.TryUse(level, out _), Is.True); Assert.That(hints.Stage(level), Is.EqualTo(i)); }
            now = now.AddDays(1); Assert.That(hints.TryUse(level, out _), Is.False);
            Assert.That(hints.Remaining, Is.EqualTo(3)); Assert.That(hints.Stage(level), Is.EqualTo(3));
        }
        [Test] public void AllowanceWritesLeaveOtherPlayerRecordsUntouched()
        {
            string[] keys = {"SHIFT.Tests.Allowance.Progress", "SHIFT.Tests.Allowance.Perfect", "SHIFT.Tests.Allowance.Daily", "SHIFT.Tests.Allowance.Settings", "SHIFT.Tests.Allowance.Consent"};
            try
            {
                foreach (string k in keys) PlayerPrefs.SetString(k, "sentinel");
                var s = Store(); s.TryConsumeHint(); s.TryUndo(() => true); s.Grant(RewardReason.Undo, s.DateKey); now = now.AddDays(1); s.Refresh();
                foreach (string k in keys) Assert.That(PlayerPrefs.GetString(k), Is.EqualTo("sentinel"));
            }
            finally { foreach (string k in keys) PlayerPrefs.DeleteKey(k); }
        }
        [Test] public void ProtectedProjectFilesMatchPreAllowanceBaseline()
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            foreach (var line in File.ReadAllLines(Path.Combine(Application.dataPath, "_Game/Tests/Editor/AllowanceProtectedFiles.txt")))
            {
                var pair = line.Split('|');
                Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath, "..", pair[0])))).Replace("-", ""), Is.EqualTo(pair[1]), pair[0]);
            }
        }
        private sealed class Ads : IRewardedAdService
        {
            public bool IsRewardedAdAvailable { get; set; } = true;
            public Action<bool> Done;
            public int Calls;
            public RewardReason Reason;
            public void ShowRewardedAd(RewardReason reason, Action<bool> complete) { Reason = reason; Calls++; Done = complete; }
        }
        private static IEnumerator Ready() { yield return new WaitForSecondsRealtime(.35f); }
        private static IEnumerator Move(PrototypeGame game)
        {
            var tap = game.CurrentLevel.KnownSolution[0];
            Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(x => x.Data.Active && x.Data.Type == PieceType.Normal && x.Data.Position == tap).GetComponentInChildren<Button>().onClick.Invoke();
            int balance = game.Allowances.UndosRemaining; Assert.That(game.Undo(), Is.False); Assert.That(game.Allowances.UndosRemaining, Is.EqualTo(balance));
            float end = Time.realtimeSinceStartup + 12;
            while (game.Board.State == GameState.Resolving && Time.realtimeSinceStartup < end) yield return null;
            Assert.That(game.Board.CanUndo, Is.True);
        }
        private static void Balances(PrototypeGame game, int hints, int undos)
        {
            Assert.That(game.Allowances.HintsRemaining, Is.EqualTo(hints)); Assert.That(game.Allowances.UndosRemaining, Is.EqualTo(undos));
            Assert.That(GameObject.Find("Hint Count").GetComponent<Text>().text, Is.EqualTo(hints.ToString()));
            Assert.That(GameObject.Find("Undo Count").GetComponent<Text>().text, Is.EqualTo(undos.ToString()));
        }
        private static void Capture(string name)
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "../Validation/allowances"));
            SprintPresentationTests.Capture("allowances/" + name + ".png");
        }
        [UnityTest] public IEnumerator MidnightAndInterruptedRewardsNeverCreditWrongDay()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode(); yield return Ready();
            var game = Object.FindFirstObjectByType<PrototypeGame>(); var date = new DateTime(2026, 10, 5);
            game.LocalNow = () => date; game.UnlockAllLevels(); game.SelectLevel(9); var ads = new Ads(); game.RewardedAds = ads; yield return Ready();
            for (int i = 0; i < 3; i++) game.Allowances.TryConsumeHint();
            game.OpenHint(); game.RequestRewardedHint(); var late = ads.Done;
            date = date.AddDays(1); game.SendMessage("OnApplicationPause", false); late(true); late(true);
            Balances(game, 3, 5); Assert.That(game.Hints.Stage(game.CurrentLevel), Is.Zero); game.CloseHint();
            for (int i = 0; i < 3; i++) game.Allowances.TryConsumeHint();
            game.OpenHint(); int calls = ads.Calls; date = date.AddDays(1); game.RequestRewardedHint();
            Assert.That(ads.Calls, Is.EqualTo(calls)); Balances(game, 2, 5); Assert.That(game.Hints.Stage(game.CurrentLevel), Is.EqualTo(1));
            for (int i = 0; i < 5; i++) game.Allowances.TryUndo(() => true);
            yield return Move(game); game.Undo(); game.RequestRewardedHint(); late = ads.Done;
            date = date.AddDays(1); late(true); Balances(game, 3, 5); Assert.That(game.Board.CanUndo, Is.True); game.CloseHint();
            for (int i = 0; i < 5; i++) game.Allowances.TryUndo(() => true);
            game.Undo(); game.RequestRewardedHint(); late = ads.Done; game.SelectLevel(20); late(true); yield return Ready();
            Balances(game, 3, 0); Assert.That(game.Board.CanUndo, Is.False);
            var provider = (LocalAnalyticsService)typeof(TelemetryTracker).GetField("provider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(game.Telemetry);
            Assert.That(provider.Events.Any(e => e.name == "rewarded_undo_requested"), Is.True);
            Assert.That(provider.Events.Any(e => e.name == "rewarded_undo_failed"), Is.True);
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator SharedDailyPoolsRecoveryAndScreenshots()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath); ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode(); yield return Ready();
            var game = Object.FindFirstObjectByType<PrototypeGame>(); var date = new DateTime(2026, 10, 5);
            game.LocalNow = () => date; game.UnlockAllLevels(); game.SelectLevel(9); var ads = new Ads(); game.RewardedAds = ads; yield return Ready();
            Balances(game, 3, 5); Assert.That(game.Undo(), Is.False); Capture("01-start-5-3");
            game.OpenHint();
            for (int i = 0; i < 2; i++) { yield return Move(game); Assert.That(game.Undo(), Is.True); Assert.That(game.Undo(), Is.False); }
            Balances(game, 2, 3); yield return Ready(); Capture("02-spent");
            game.Restart(); yield return Ready(); Balances(game, 2, 3); Assert.That(game.Hints.Stage(game.CurrentLevel), Is.EqualTo(1));
            Assert.That(game.StartDaily(date), Is.True); yield return Ready(); Balances(game, 2, 3); game.OpenHint(); Balances(game, 1, 3); Capture("07-daily-shared");
            game.SelectLevel(9); yield return Ready(); Balances(game, 1, 3); Capture("06-campaign-after-daily");
            Assert.That(game.StartDaily(date.AddDays(-1), true), Is.True); yield return Ready(); Balances(game, 1, 3);
            game.SelectLevel(9); yield return Ready(); game.OpenHint(); Balances(game, 0, 3);
            game.OpenHint(); yield return Ready(); Capture("03-zero-hint");
            game.RequestRewardedHint(); game.RequestRewardedHint(); Assert.That(ads.Calls, Is.EqualTo(1)); Assert.That(ads.Reason, Is.EqualTo(RewardReason.Hint));
            ads.Done(false); Balances(game, 0, 3); ads.Done(true); Balances(game, 0, 3);
            game.RequestRewardedHint(); ads.Done(true); ads.Done(true); Balances(game, 0, 3); Assert.That(game.Hints.Stage(game.CurrentLevel), Is.EqualTo(3));
            game.OpenHint(); Assert.That(GameObject.Find("Rewarded Hint Panel"), Is.Null); Assert.That(GameObject.Find("Status").GetComponent<Text>().text, Is.EqualTo("No more hints available."));
            for (int i = 0; i < 3; i++) { yield return Move(game); Assert.That(game.Undo(), Is.True); }
            Balances(game, 0, 0); Assert.That(game.Undo(), Is.False); Assert.That(GameObject.Find("Rewarded Hint Panel"), Is.Null);
            yield return Move(game); int moves = game.Board.MovesRemaining; Assert.That(game.Undo(), Is.False); yield return Ready(); Capture("04-zero-undo");
            game.RequestRewardedHint(); game.RequestRewardedHint(); Assert.That(ads.Reason, Is.EqualTo(RewardReason.Undo));
            ads.Done(false); Balances(game, 0, 0); ads.Done(true); Balances(game, 0, 0);
            game.RequestRewardedHint(); ads.Done(true); ads.Done(true); Balances(game, 0, 1);
            Assert.That(game.Board.MovesRemaining, Is.EqualTo(moves)); Assert.That(game.Board.CanUndo, Is.True); yield return Ready(); Capture("05-undo-credit-earned");
            var provider = (LocalAnalyticsService)typeof(TelemetryTracker).GetField("provider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(game.Telemetry);
            Assert.That(provider.Events.Count(e => e.name == "rewarded_undo_completed"), Is.EqualTo(1));
            Assert.That(provider.Events.Count(e => e.name == "rewarded_hint_completed"), Is.EqualTo(1));
            Assert.That(game.Undo(), Is.True); Balances(game, 0, 0); Assert.That(game.Board.MovesRemaining, Is.EqualTo(moves + 1));
            yield return Move(game); game.DailyLanguage = HintLanguage.Turkish; game.Undo(); yield return Ready(); Capture("09-turkish-undo");
            Assert.That(GameObject.Find("Hint Title").GetComponent<Text>().text, Does.Contain("geri alma"));
            game.RequestRewardedHint(); var stale = ads.Done; game.Restart(); stale(true); yield return Ready(); Balances(game, 0, 0);
            game.OpenSettings(); yield return Ready(); Capture("10-settings");
            foreach (var t in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            { Assert.That(t.text, Does.Not.Contain("Privacy choices")); Assert.That(t.text, Does.Not.Contain("Gizlilik tercihleri")); }
            game.CloseSettings(); int stage = game.Hints.Stage(game.CurrentLevel); moves = game.Board.MovesRemaining;
            date = date.AddDays(1); game.SendMessage("OnApplicationPause", false); yield return Ready(); Balances(game, 3, 5); Capture("08-next-day");
            Assert.That(game.Hints.Stage(game.CurrentLevel), Is.EqualTo(stage)); Assert.That(game.Board.MovesRemaining, Is.EqualTo(moves));
            game.OpenHint(); Balances(game, 3, 5); Assert.That(GameObject.Find("Status").GetComponent<Text>().text, Is.EqualTo("Başka ipucu yok."));
            // A scene reload reuses the persisted balances; only hint knowledge is session-local.
            game.SelectLevel(20); yield return Ready(); game.OpenHint(); Balances(game, 2, 5);
            UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene, UnityEngine.SceneManagement.LoadSceneMode> loaded = (_, __) =>
            {
                var reloaded = Object.FindFirstObjectByType<PrototypeGame>();
                typeof(PrototypeGame).GetField("saveKeyPrefix", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(reloaded, Prefix);
                typeof(PrototypeGame).GetField("settingsKeyPrefix", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(reloaded, Prefix + "Settings.");
                reloaded.LocalNow = () => date;
            };
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += loaded;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Prototype"); yield return Ready();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= loaded;
            game = Object.FindFirstObjectByType<PrototypeGame>(); Balances(game, 2, 5);
            yield return new ExitPlayMode();
        }
    }
}
