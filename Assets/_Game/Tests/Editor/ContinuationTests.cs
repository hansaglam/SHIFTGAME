using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Shift.Game.Tests
{
    public sealed class ContinuationTests
    {
        private const string Prefix = "SHIFT.Tests.Continuation.";
        private static PrototypeGame Game => Object.FindFirstObjectByType<PrototypeGame>();
        private static ChapterSelect Chapter => Object.FindFirstObjectByType<ChapterSelect>(FindObjectsInactive.Include);
        private static string ContinueText => GameObject.Find("Play Current").GetComponentInChildren<Text>().text;
        private static string DailyTextValue => GameObject.Find("Daily Shift Entry").GetComponentInChildren<Text>().text;
        private static void Seed(int completed, int current = 0, bool mastered = false)
        {
            var saves = new SaveService(Prefix); saves.Reset();
            saves.Save(new ProgressData(Math.Min(39, completed + 1), current, completed));
            if (mastered) for (int i = 0; i < 40; i++) saves.RecordPerfect(i,40);
        }
        private static void Scene(int completed, int current = 0, bool mastered = false, bool deferred = false)
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);
            ProgressionPresentationTests.IsolateSave(Prefix);
            PlayerPrefs.DeleteKey(Prefix+"Daily.v1.History");
            Seed(completed,current,mastered);
            PrototypeGame.SetDirectGameplayForValidation(Prefix, false);
            Game.enabled = !deferred;
        }
        private static IEnumerator Ready() { yield return null; yield return new WaitForSecondsRealtime(.35f); }
        private static void Capture(string name)
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/resume"));
            SprintPresentationTests.Capture("resume/"+name+".png");
            foreach (var label in GameObject.Find("SHIFT UI").GetComponentsInChildren<Text>())
            {
                if (label.transform.parent.name != "Play Current" && label.transform.parent.name != "Daily Shift Entry") continue;
                Assert.That(label.preferredWidth,Is.LessThanOrEqualTo(label.rectTransform.rect.width+2),label.text);
                Assert.That(label.preferredHeight,Is.LessThanOrEqualTo(label.rectTransform.rect.height+2),label.text);
            }
        }
        [TearDown] public void Cleanup()
        {
            new SaveService(Prefix).Reset();
            foreach(var key in new[]{"Daily.v1.History","Onboarding.v1","Language.v1","DailyAllowance.v1"}) PlayerPrefs.DeleteKey(Prefix+key);
        }
        [TestCase(-1,0,0)][TestCase(0,0,1)][TestCase(11,4,12)][TestCase(19,4,20)][TestCase(38,1,39)][TestCase(39,4,-1)]
        public void ContinueUsesPersistedFrontierNotLastReplay(int completed,int replay,int expected)
        {
            Seed(completed,replay);
            var p = new LevelProgression(40,new SaveService(Prefix),LevelValidation.AllLevels());
            Assert.That(CampaignContinuation.LevelIndex(p),Is.EqualTo(expected));
            if(expected>=0)Assert.That(p.IsUnlocked(expected),Is.True);
        }
        [Test] public void CompletedReplayDoesNotMoveContinueBackwardAfterReload()
        {
            Seed(11,4);var p=new LevelProgression(40,new SaveService(Prefix));p.Complete(4);
            Assert.That(new LevelProgression(40,new SaveService(Prefix)).Current,Is.EqualTo(5));
            Assert.That(CampaignContinuation.LevelIndex(new LevelProgression(40,new SaveService(Prefix))),Is.EqualTo(12));
        }
        [Test] public void CorruptAndOutOfRangeProgressIsClampedAndEmptyIsSafe()
        {
            PlayerPrefs.SetInt(Prefix+"Unlocked",999);PlayerPrefs.SetInt(Prefix+"Current",999);PlayerPrefs.SetInt(Prefix+"Completed",-50);
            var p=new LevelProgression(40,new SaveService(Prefix));Assert.That(CampaignContinuation.LevelIndex(p),Is.Zero);
            Assert.That(CampaignContinuation.LevelIndex(null),Is.EqualTo(-1));Assert.That(CampaignContinuation.Chapter(null),Is.Zero);
        }
        [TestCase(GameLanguage.English,"Continue Level 13")][TestCase(GameLanguage.Turkish,"13. Bölüme Devam Et")]
        public void ContinueCopyIsLocalized(GameLanguage language,string expected)
        { Assert.That(LocalizationCatalog.Format("resume.continue_level",language,13),Is.EqualTo(expected)); }
        [UnityTest] public IEnumerator ColdLaunchLevelTwoThenContinueAndForegroundPreserveBoard()
        {
            Scene(0);yield return new EnterPlayMode();yield return Ready();
            Assert.That(Game.IsOnboardingActive,Is.False);Assert.That(Chapter.IsOpen,Is.True);
            Assert.That(ContinueText,Is.EqualTo("Continue Level 2"));Capture("01-return-level2");
            var campaign = new SaveService(Prefix).Load(40);Assert.That(campaign.Current,Is.Zero,"Viewing does not overwrite replay selection");
            GameObject.Find("Play Current").GetComponent<Button>().onClick.Invoke();yield return Ready();
            Assert.That(Game.Progress.Current,Is.EqualTo(1));Assert.That(Game.CurrentLevel.name,Does.StartWith("Level02"));
            var tap = Game.CurrentLevel.KnownSolution[0];
            Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(x=>x.Data.Active&&x.Data.Type==PieceType.Normal&&x.Data.Position==tap).GetComponentInChildren<Button>().onClick.Invoke();
            yield return new WaitForSecondsRealtime(1);
            var board=Game.Board;var positions=board.Pieces.Select(x=>x.Position).ToArray();int moves=board.MovesRemaining;
            Lifecycle(true);Lifecycle(false);yield return Ready();
            Assert.That(Game.Board,Is.SameAs(board));Assert.That(board.MovesRemaining,Is.EqualTo(moves));Assert.That(board.Pieces.Select(x=>x.Position),Is.EqualTo(positions));
            Assert.That(Chapter.IsOpen,Is.False);Capture("11-background-resume-gameplay");
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator MidCampaignReplayDailyIsolationAndLiveLanguages()
        {
            Scene(11,4);yield return new EnterPlayMode();yield return Ready();
            Assert.That(ContinueText,Is.EqualTo("Continue Level 13"));Capture("02-return-level13");Capture("03-daily-available");Capture("09-English");
            Assert.That(GameObject.Find("Level 13").GetComponentInChildren<Text>().text,Does.Contain("PLAY"));
            GameLanguageService.Shared.Select(GameLanguage.Turkish);yield return null;
            Assert.That(ContinueText,Is.EqualTo("13. Bölüme Devam Et"));Capture("08-Turkish");
            GameLanguageService.Shared.Select(GameLanguage.English);
            int hints=Game.Allowances.HintsRemaining,undos=Game.Allowances.UndosRemaining;
            string perfects=PlayerPrefs.GetString(Prefix+"Mastery.v1.Perfect"),onboarding=PlayerPrefs.GetString(Prefix+"Onboarding.v1");
            string daily=PlayerPrefs.GetString(Prefix+"Daily.v1.History");bool sound=Game.Settings.Sound;
            Assert.That(Game.SelectLevel(4),Is.True);Game.Progress.Complete(4);Game.OpenChapter();yield return Ready();
            Assert.That(ContinueText,Is.EqualTo("Continue Level 13"));Capture("10-replay-forward");
            Assert.That(Game.StartDaily(DateTime.Now),Is.True);yield return Ready();Assert.That(Game.ContinueLevelIndex,Is.EqualTo(12));
            Assert.That(Game.StartDaily(DateTime.Now.AddDays(-1),true),Is.True);yield return Ready();Game.OpenChapter();yield return Ready();
            Assert.That(ContinueText,Is.EqualTo("Continue Level 13"));
            Assert.That(Game.Allowances.HintsRemaining,Is.EqualTo(hints));Assert.That(Game.Allowances.UndosRemaining,Is.EqualTo(undos));
            Assert.That(PlayerPrefs.GetString(Prefix+"Mastery.v1.Perfect"),Is.EqualTo(perfects));Assert.That(PlayerPrefs.GetString(Prefix+"Daily.v1.History"),Is.EqualTo(daily));
            Assert.That(PlayerPrefs.GetString(Prefix+"Onboarding.v1"),Is.EqualTo(onboarding));Assert.That(Game.Settings.Sound,Is.EqualTo(sound));
            Game.ContinueCampaign();yield return Ready();Assert.That(Game.IsDaily,Is.False);Assert.That(Game.Progress.Current,Is.EqualTo(12));
            Assert.That(Game.Board.MovesRemaining,Is.EqualTo(Game.CurrentLevel.MoveLimit));
            yield return new ExitPlayMode();
        }
        private static void StoreDaily(bool perfect)
        {
            var puzzle=new DailyPool(LevelValidation.AllLevels()).For(DateTime.Now);
            var result=new AttemptMetrics{completed=true,levelId=puzzle.Level.name,levelIndex=puzzle.SourceIndex,perfectShift=perfect,hasOptimal=true,verifiedOptimalMoves=puzzle.Level.VerifiedOptimalMoveCount.Value};
            new DailySaveService(Prefix+"Daily.v1.History").Record(puzzle,result,out _,out _);
        }
        [UnityTest] public IEnumerator PersistedDailyCompleteIsOptionalOnColdLaunch()
        {
            Scene(11);StoreDaily(false);yield return new EnterPlayMode();yield return Ready();
            Assert.That(DailyTextValue,Does.Contain("Completed"));Assert.That(Game.IsDaily,Is.False);Assert.That(ContinueText,Is.EqualTo("Continue Level 13"));Capture("04-daily-complete");
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator PersistedDailyPerfectIsOptionalOnColdLaunch()
        {
            Scene(11);StoreDaily(true);yield return new EnterPlayMode();yield return Ready();
            Assert.That(DailyTextValue,Does.Contain("Perfect Shift"));Assert.That(Game.IsDaily,Is.False);Capture("05-daily-perfect");
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator CompletedCampaignUsesMasteryNeverLevelFortyOne()
        {
            Scene(39,39);yield return new EnterPlayMode();yield return Ready();
            Assert.That(Game.ContinueLevelIndex,Is.EqualTo(-1));Assert.That(ContinueText,Is.EqualTo("Review Mastery"));
            GameObject.Find("Play Current").GetComponent<Button>().onClick.Invoke();yield return Ready();
            Assert.That(Chapter.IsOpen,Is.True);Assert.That(GameObject.Find("Level 41"),Is.Null);Capture("06-campaign-complete");
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator FullyMasteredCampaignRemainsValid()
        {
            Scene(39,39,true);yield return new EnterPlayMode();yield return Ready();
            Assert.That(Game.Progress.ChapterSummary(0).Mastered,Is.True);Assert.That(Game.Progress.ChapterSummary(1).Mastered,Is.True);
            Assert.That(GameObject.Find("Chapter Title").GetComponent<Text>().text,Is.EqualTo("CHAPTER MASTERED"));Capture("07-campaign-mastered");
            Game.ContinueCampaign();Assert.That(Chapter.IsOpen,Is.True);Assert.That(Game.ContinueLevelIndex,Is.EqualTo(-1));
            yield return new ExitPlayMode();
        }
        private sealed class BusyAds : IRewardedAdService,IPrivacyChoices
        {
            public bool Busy=true;public bool IsBusy=>Busy;public bool IsPrivacyOptionsRequired=>false;public bool IsRewardedAdAvailable=>false;
            public void ShowPrivacyOptions(Action<bool> done)=>done(false);
            public void ShowRewardedAd(RewardReason reason,Action<bool> done)=>done(false);
        }
        [UnityTest] public IEnumerator NativeConsentWaitsOnceAndAdReturnDoesNotReopenNavigation()
        {
            Scene(11,4,deferred:true);yield return new EnterPlayMode();
            var ads=new BusyAds();Game.RewardedAds=ads;Game.enabled=true;yield return Ready();
            Assert.That(Game.IsStartupWaiting,Is.True);Assert.That(Chapter,Is.Null);Assert.That(Game.Board.Pieces,Is.Null);
            Assert.That(Game.SelectLevel(0),Is.False);Game.ContinueCampaign();Game.Restart();
            ads.Busy=false;yield return Ready();Assert.That(Chapter.IsOpen,Is.True);Assert.That(ContinueText,Is.EqualTo("Continue Level 13"));
            Game.CloseChapter();yield return Ready();Assert.That(Game.Progress.Current,Is.EqualTo(12));
            var board=Game.Board;ads.Busy=true;Lifecycle(true);ads.Busy=false;Lifecycle(false);yield return Ready();
            Assert.That(Chapter.IsOpen,Is.False);Assert.That(Game.Board,Is.SameAs(board));Assert.That(Game.IsStartupWaiting,Is.False);
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator FreshInstallStillOnboardsDirectlyIntoLevelOne()
        {
            Scene(-1);PlayerPrefs.DeleteKey(Prefix+"Onboarding.v1");yield return new EnterPlayMode();yield return Ready();
            Assert.That(Game.IsOnboardingActive,Is.True);Assert.That(Chapter,Is.Null);Capture("12-fresh-onboarding");
            var panel=Object.FindFirstObjectByType<FirstLaunchOnboardingPanel>();
            // Existing first-page secondary action is Skip; no onboarding implementation is altered.
            panel.Secondary();yield return Ready();Assert.That(Game.IsOnboardingActive,Is.False);
            Assert.That(Game.CurrentLevel.name,Does.StartWith("Level01"));Assert.That(Chapter.IsOpen,Is.False);
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator EmptyCampaignAndInvalidDailyDateFailSafely()
        {
            Scene(-1,deferred:true);var serialized=new SerializedObject(Game);serialized.FindProperty("levels").arraySize=0;serialized.FindProperty("level").objectReferenceValue=null;serialized.ApplyModifiedPropertiesWithoutUndo();
            yield return new EnterPlayMode();Game.enabled=true;yield return Ready();
            Assert.That(GameObject.Find("Campaign Unavailable"),Is.Not.Null);Assert.That(Game.ContinueLevelIndex,Is.EqualTo(-1));Game.ContinueCampaign();Game.Restart();
            yield return new ExitPlayMode();
            Scene(-1);yield return new EnterPlayMode();yield return Ready();Game.LocalNow=()=>DateTime.MinValue;Game.OpenChapter();yield return Ready();
            Assert.That(GameObject.Find("Daily Shift Entry").GetComponent<Button>().interactable,Is.False);Assert.That(Game.StartDaily(DateTime.MinValue),Is.False);
            Assert.That(Game.ContinueLevelIndex,Is.Zero);Game.LocalNow=()=>DateTime.Now;
            yield return new ExitPlayMode();
        }
        [Test] public void LockedGameplayOnboardingAndPersistentServicesRemainByteIdentical()
        {
            using var sha=SHA256.Create();
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/ContinuationProtectedFiles.txt")))
            {
                var split=line.IndexOf(' ');var path=line.Substring(split+1);
                Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,"..",path)))).Replace("-","").ToLowerInvariant(),Is.EqualTo(line.Substring(0,split)),path);
            }
        }
        private static void Lifecycle(bool paused)
        {
            typeof(PrototypeGame).GetMethod("OnApplicationPause",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(Game,new object[]{paused});
            typeof(PrototypeGame).GetMethod("OnApplicationFocus",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(Game,new object[]{!paused});
        }
    }
}
