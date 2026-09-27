using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Shift.Game.Tests
{
    public sealed class CampaignEndingTests
    {
        private const string Prefix="SHIFT.Tests.Ending.";
        private const string Key=Prefix+"CampaignEnding.Seen.v1";
        private static PrototypeGame Game=>Object.FindFirstObjectByType<PrototypeGame>();
        private static CampaignEndingPanel Panel=>Object.FindFirstObjectByType<CampaignEndingPanel>(FindObjectsInactive.Include);
        [SetUp] public void Reset()=>PlayerPrefs.DeleteKey(Key);
        [TearDown] public void Cleanup() { PlayerPrefs.DeleteKey(Key);new SaveService(Prefix).Reset(); }
        [TestCase(39,true,false,false,true,true)]
        [TestCase(39,true,true,false,true,false)]
        [TestCase(39,true,false,true,true,false)]
        [TestCase(38,true,false,false,true,false)]
        [TestCase(39,false,false,false,true,false)]
        [TestCase(39,true,false,false,false,false)]
        public void TriggerIsOnlyValidCampaignFinale(int index,bool won,bool daily,bool debug,bool complete,bool expected)
        {Assert.That(new CampaignEndingState(Key).Eligible(index,won,daily,debug,complete),Is.EqualTo(expected));}
        [Test] public void SeenPersistsAndIsIdempotent()
        {var state=new CampaignEndingState(Key);Assert.That(state.MarkSeen(),Is.True);Assert.That(new CampaignEndingState(Key).Seen,Is.True);Assert.That(state.MarkSeen(),Is.False);Assert.That(state.Eligible(39,true,false,false,true),Is.False);}
        [TestCase(-1)][TestCase(2)][TestCase(999)] public void MalformedSeenFlagIsMigrationSafe(int value)
        {PlayerPrefs.SetInt(Key,value);Assert.That(new CampaignEndingState(Key).Seen,Is.False);}
        [Test] public void OldCompletedPlayerCanSeeEndingAfterReplay()
        {Assert.That(new CampaignEndingState(Key).Eligible(39,true,false,false,true),Is.True);}
        [TestCase(GameLanguage.English)][TestCase(GameLanguage.Turkish)]
        public void EveryEndingStringHasTranslation(GameLanguage language)
        {
            var keys=LocalizationCatalog.Keys.Where(x=>x.StartsWith("ending.")).ToArray();Assert.That(keys.Length,Is.EqualTo(13));
            foreach(var key in keys)Assert.That(LocalizationCatalog.Format(key,language,23),Is.Not.Empty);
            Assert.That(LocalizationCatalog.Format("ending.continue",language),Is.EqualTo(language==GameLanguage.English?"CONTINUE":"DEVAM"));
        }
        [Test] public void EndingTelemetryUsesSemanticEventsAndNoAttemptMutation()
        {
            var provider=new LocalAnalyticsService();var tracker=new TelemetryTracker(provider,()=>0);tracker.StartSession();
            foreach(var name in new[]{"campaign_ending_viewed","campaign_ending_continue","campaign_ending_mastery_selected","campaign_ending_daily_selected"})tracker.CampaignEndingEvent(name,40);
            foreach(var e in provider.Events.Where(x=>x.name.StartsWith("campaign_ending_")))
            {Assert.That(e.source,Is.EqualTo("level40"));Assert.That(e.perfectCount,Is.EqualTo(40));Assert.That(e.mastered,Is.True);Assert.That(e.attempt,Is.Null);}
        }
        [Test] public void MasteryDestinationNeverProducesLevelFortyOne()
        {
            new SaveService(Prefix).Save(new ProgressData(39,39,39));
            var progress=new LevelProgression(40,new SaveService(Prefix),LevelValidation.AllLevels());
            Assert.That(CampaignContinuation.LevelIndex(progress),Is.EqualTo(-1));Assert.That(CampaignContinuation.Chapter(progress),Is.Zero);
        }
        [TestCase(false)][TestCase(true)]
        public void EndingButtonsRouteOnlyToExistingDestinations(bool mastered)
        {
            var root=new GameObject("Ending route fixture",typeof(RectTransform));var circle=PlaceholderVisuals.CreateCircle();var rounded=PlaceholderVisuals.CreateRounded();
            int mastery=0,daily=0,levels=0;
            try
            {
                var panel=root.AddComponent<CampaignEndingPanel>();
                panel.Build(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),rounded,circle,new GameFeelSettings{reducedMotion=true},mastered?40:23,()=>mastery++,()=>daily++,()=>levels++);
                panel.Open();Assert.That(panel.RevealDuration,Is.InRange(.25f,.45f));
                root.transform.Find("Ending Primary").GetComponent<Button>().onClick.Invoke();
                Assert.That(mastered?daily:mastery,Is.EqualTo(1));
                root.transform.Find("Ending Secondary").GetComponent<Button>().onClick.Invoke();
                Assert.That(mastered?levels:daily,Is.EqualTo(1));
                if(!mastered){root.transform.Find("Ending Levels").GetComponent<Button>().onClick.Invoke();Assert.That(levels,Is.EqualTo(1));}
            }
            finally{Object.DestroyImmediate(root);Object.DestroyImmediate(circle.texture);Object.DestroyImmediate(circle);Object.DestroyImmediate(rounded.texture);Object.DestroyImmediate(rounded);}
        }
        private static void Scene(bool alreadyComplete=false,bool allEarlierPerfect=false,bool reduced=false,bool cold=false)
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            PlayerPrefs.DeleteKey(Key);PlayerPrefs.DeleteKey(Prefix+"Daily.v1.History");
            var saves=new SaveService(Prefix);saves.Save(new ProgressData(39,39,alreadyComplete?39:38));
            if(allEarlierPerfect)for(int i=0;i<39;i++)saves.RecordPerfect(i,40);
            new SettingsService(Prefix+"Settings.").Save(true,true,reduced);
            PrototypeGame.SetDirectGameplayForValidation(Prefix,!cold);
        }
        private static IEnumerator Ready(){yield return null;yield return new WaitForSecondsRealtime(.4f);}
        private static IEnumerator Solve(bool beforeCapture=false,bool nonPerfect=false)
        {
            var route=nonPerfect?Game.CurrentLevel.KnownSolution.Take(5).Concat(new[]{new GridPosition(2,1),new GridPosition(3,1),new GridPosition(3,0)}).ToArray():Game.CurrentLevel.KnownSolution.ToArray();
            for(int i=0;i<route.Length;i++)
            {
                if(beforeCapture&&i==route.Length-1)Capture("01-final-move-before");
                var p=route[i];Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(x=>x.Data.Active&&x.Data.Type==PieceType.Normal&&x.Data.Position==p).GetComponentInChildren<Button>().onClick.Invoke();
                float deadline=Time.realtimeSinceStartup+12;
                while(Game.Board.State==GameState.Resolving&&Time.realtimeSinceStartup<deadline)yield return null;
                Assert.That(Game.Board.State,Is.Not.EqualTo(GameState.Resolving));
            }
            yield return Ready();Assert.That(Game.Board.State,Is.EqualTo(GameState.Won));
        }
        private static string TextOf(string name)=>GameObject.Find(name).GetComponent<Text>().text;
        private static void Capture(string name)
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/campaign-ending"));
            SprintPresentationTests.Capture("campaign-ending/"+name+".png");
            foreach(var label in GameObject.Find("SHIFT UI").GetComponentsInChildren<Text>())
                if(label.name.StartsWith("Ending ")||label.transform.parent.name.StartsWith("Ending ")||label.name=="Perfect Final Shift")
                {Assert.That(label.preferredHeight,Is.LessThanOrEqualTo(label.rectTransform.rect.height+3),label.name+": "+label.text);}
        }
        private static int Events(string name)
        {
            var provider=(LocalAnalyticsService)typeof(TelemetryTracker).GetField("provider",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Game.Telemetry);
            return provider.Events.Count(x=>x.name==name);
        }
        [UnityTest] public IEnumerator FirstPerfectFinaleTwoStagesMasteryAndReplay()
        {
            Scene();yield return new EnterPlayMode();yield return Ready();yield return Solve(true);
            Assert.That(Game.IsCampaignEndingPending,Is.True);Assert.That(TextOf("Status"),Does.StartWith("CAMPAIGN COMPLETE"));
            Assert.That(TextOf("Perfect Final Shift"),Is.EqualTo("PERFECT FINAL SHIFT"));Assert.That(GameObject.Find("Next Level"),Is.Null);
            Assert.That(new CampaignEndingState(Key).Seen,Is.False);Capture("02-first-campaign-complete");
            int hints=Game.Allowances.HintsRemaining,undos=Game.Allowances.UndosRemaining;var board=Game.Board;
            string daily=PlayerPrefs.GetString(Prefix+"Daily.v1.History"),perfect=PlayerPrefs.GetString(Prefix+"Mastery.v1.Perfect"),onboarding=PlayerPrefs.GetString(Prefix+"Onboarding.v1");
            var soundEvents=new System.Collections.Generic.List<AudioCue>();var pulses=new System.Collections.Generic.List<HapticCue>();
            Game.GetComponent<AudioManager>().CuePlayed+=soundEvents.Add;
            ((HapticService)typeof(PrototypeGame).GetField("haptics",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Game)).PulseSent+=pulses.Add;
            GameObject.Find("Campaign Continue").GetComponent<Button>().onClick.Invoke();yield return Ready();
            Assert.That(soundEvents.Count(x=>x==AudioCue.Win),Is.EqualTo(1));Assert.That(pulses.Count,Is.EqualTo(1));
            Assert.That(Panel.IsOpen,Is.True);Assert.That(Panel.Mastered,Is.False);Assert.That(Game.Board,Is.SameAs(board));
            Assert.That(new CampaignEndingState(Key).Seen,Is.True);Capture("03-ending-incomplete-mastery");Capture("05-English");
            GameLanguageService.Shared.Select(GameLanguage.Turkish);yield return null;Assert.That(TextOf("Ending Title"),Is.EqualTo("KAMPANYA TAMAMLANDI"));Capture("06-Turkish");
            GameLanguageService.Shared.Select(GameLanguage.English);
            Lifecycle(true);Lifecycle(false);Game.OpenCampaignEnding();yield return Ready();
            Assert.That(Object.FindObjectsByType<CampaignEndingPanel>(FindObjectsSortMode.None).Length,Is.EqualTo(1));Assert.That(Events("campaign_ending_viewed"),Is.EqualTo(1));Assert.That(Events("campaign_ending_continue"),Is.EqualTo(1));
            Assert.That(Game.Allowances.HintsRemaining,Is.EqualTo(hints));Assert.That(Game.Allowances.UndosRemaining,Is.EqualTo(undos));
            Assert.That(PlayerPrefs.GetString(Prefix+"Daily.v1.History"),Is.EqualTo(daily));Assert.That(PlayerPrefs.GetString(Prefix+"Mastery.v1.Perfect"),Is.EqualTo(perfect));Assert.That(PlayerPrefs.GetString(Prefix+"Onboarding.v1"),Is.EqualTo(onboarding));
            GameObject.Find("Ending Primary").GetComponent<Button>().onClick.Invoke();yield return Ready();
            Assert.That(Object.FindFirstObjectByType<ChapterSelect>().IsOpen,Is.True);Assert.That(TextOf("Chapter Title"),Is.EqualTo("CHAPTER 1"));Assert.That(Events("campaign_ending_mastery_selected"),Is.EqualTo(1));Capture("07-mastery-destination");Assert.That(GameObject.Find("Campaign Continue"),Is.Null);
            Game.SelectLevel(39);yield return Ready();yield return Solve();
            Assert.That(Game.IsCampaignEndingPending,Is.False);Assert.That(GameObject.Find("Campaign Continue"),Is.Null);Assert.That(GameObject.Find("Next Level"),Is.Null);Capture("09-seen-replay-normal");
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator FullyMasteredReducedMotionPrioritizesDaily()
        {
            Scene(allEarlierPerfect:true,reduced:true);yield return new EnterPlayMode();yield return Ready();yield return Solve();
            Game.OpenCampaignEnding();Assert.That(Panel.GetComponent<CanvasGroup>().alpha,Is.EqualTo(1));Assert.That(Panel.transform.localScale,Is.EqualTo(Vector3.one));
            Assert.That(Panel.Mastered,Is.True);yield return Ready();Capture("04-ending-mastered");Capture("10-reduced-motion");
            Assert.That(GameObject.Find("Ending Primary").GetComponentInChildren<Text>().text,Is.EqualTo("DAILY SHIFT"));
            GameObject.Find("Ending Primary").GetComponent<Button>().onClick.Invoke();yield return Ready();
            Assert.That(Object.FindFirstObjectByType<DailyShiftPanel>().IsOpen,Is.True);Assert.That(Events("campaign_ending_daily_selected"),Is.EqualTo(1));Capture("08-daily-destination");
            Assert.That(Game.ContinueLevelIndex,Is.EqualTo(-1));yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator OldSaveColdStartDoesNotForceEndingThenNonPerfectReplayCanShowIt()
        {
            Scene(alreadyComplete:true,cold:true);yield return new EnterPlayMode();yield return Ready();
            Assert.That(Panel,Is.Null);Assert.That(Game.IsCampaignEndingPending,Is.False);Assert.That(Object.FindFirstObjectByType<ChapterSelect>().IsOpen,Is.True);
            Game.SelectLevel(39);yield return Ready();yield return Solve(nonPerfect:true);
            Assert.That(Game.Telemetry.Result.perfectShift,Is.False);Assert.That(Game.Progress.ChapterComplete,Is.True);Assert.That(Game.IsCampaignEndingPending,Is.True);Assert.That(GameObject.Find("Perfect Final Shift"),Is.Null);
            Game.OpenCampaignEnding();yield return Ready();Assert.That(Panel.IsOpen,Is.True);Assert.That(new CampaignEndingState(Key).Seen,Is.True);
            yield return new ExitPlayMode();
        }
        private sealed class BusyAds:IRewardedAdService,IPrivacyChoices
        {
            public bool Busy=true;public bool IsBusy=>Busy;public bool IsPrivacyOptionsRequired=>false;public bool IsRewardedAdAvailable=>false;
            public void ShowPrivacyOptions(Action<bool> done)=>done(false);public void ShowRewardedAd(RewardReason reason,Action<bool> done)=>done(false);
        }
        [UnityTest] public IEnumerator NativeBusyAndReturnCannotDuplicateReveal()
        {
            Scene();yield return new EnterPlayMode();yield return Ready();yield return Solve();
            var ads=new BusyAds();Game.RewardedAds=ads;Game.OpenCampaignEnding();Game.OpenCampaignEnding();yield return Ready();
            Assert.That(Panel,Is.Null);Assert.That(new CampaignEndingState(Key).Seen,Is.False);
            Lifecycle(true);ads.Busy=false;Lifecycle(false);yield return Ready();
            Assert.That(Panel.IsOpen,Is.True);Game.OpenCampaignEnding();Lifecycle(true);Lifecycle(false);yield return Ready();
            Assert.That(Events("campaign_ending_viewed"),Is.EqualTo(1));Assert.That(Events("campaign_ending_continue"),Is.EqualTo(1));
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator DailyUsingLevelFortyNeverOffersEnding()
        {
            Scene();yield return new EnterPlayMode();yield return Ready();
            var date=DateTime.Today;while(Game.DailyPool.For(date).SourceIndex!=39)date=date.AddDays(-1);
            Game.LocalNow=()=>date;Assert.That(Game.StartDaily(date),Is.True);yield return Ready();yield return Solve();
            Assert.That(Game.IsCampaignEndingPending,Is.False);Assert.That(Panel,Is.Null);Assert.That(new CampaignEndingState(Key).Seen,Is.False);Assert.That(GameObject.Find("Campaign Continue"),Is.Null);
            yield return new ExitPlayMode();
        }
        [Test] public void ProtectedGameplayResumeOnboardingAndServicesRemainIdentical()
        {
            using var sha=SHA256.Create();foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/EndingProtectedFiles.txt")))
            {var split=line.IndexOf(' ');var path=line.Substring(split+1);Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,"..",path)))).Replace("-","").ToLowerInvariant(),Is.EqualTo(line.Substring(0,split)),path);}
        }
        private static void Lifecycle(bool paused)
        {
            typeof(PrototypeGame).GetMethod("OnApplicationPause",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(Game,new object[]{paused});
            typeof(PrototypeGame).GetMethod("OnApplicationFocus",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(Game,new object[]{!paused});
        }
    }
}
