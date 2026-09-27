using System;
using System.Collections;
using System.IO;
using System.Linq;
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
    public sealed class MasteryTests
    {
        private const string Prefix="SHIFT.Tests.Mastery.";
        private SaveService saves;
        private LevelData[] levels;
        [SetUp] public void Setup(){saves=new SaveService(Prefix);saves.Reset();levels=LevelValidation.AllLevels().ToArray();}
        [TearDown] public void Cleanup(){new SaveService(Prefix).Reset();new SettingsService(Prefix+"Settings.").Reset();}
        private LevelProgression Progress() => new LevelProgression(40,saves,levels);
        private static AttemptMetrics Run(LevelData level,int index,bool undo=false)
        {
            var board=new BoardManager();board.Load(level);var telemetry=new TelemetryTracker(null,()=>0);
            telemetry.StartSession();telemetry.StartLevel(level,index,level.VerifiedOptimalMoveCount);
            if(undo){board.RequestMove(level.KnownSolution[0]);telemetry.AcceptedTap(board.ReactionDepth,board.MovesRemaining,false);board.CompleteResolution();Assert.That(board.Undo(),Is.True);telemetry.UndoUsed(board.MovesRemaining);}
            foreach(var tap in level.KnownSolution){Assert.That(board.RequestMove(tap),Is.True);telemetry.AcceptedTap(board.ReactionDepth,board.MovesRemaining,board.LastReactionWasCancelled);board.CompleteResolution();}
            Assert.That(board.State,Is.EqualTo(GameState.Won));telemetry.Finish(true);return telemetry.Result;
        }
        [TestCase(0,0,-1)] [TestCase(9,4,8)] [TestCase(19,19,19)] [TestCase(39,17,39)]
        public void LegacyMigrationPreservesProgressWithoutInventingPerfect(int unlocked,int current,int completed)
        {
            saves.Save(new ProgressData(unlocked,current,completed));var p=Progress();
            Assert.That(p.HighestCompleted,Is.EqualTo(completed));Assert.That(p.HighestUnlocked,Is.EqualTo(unlocked==19&&completed==19?20:unlocked));
            for(int i=0;i<40;i++){Assert.That(p.IsPerfect(i),Is.False);if(i<=completed)Assert.That(p.Mastery(i),Is.EqualTo(LevelMasteryState.Completed));}
            Assert.That(p.Current,Is.EqualTo(current==19&&completed==19?20:current));
        }
        [Test] public void NormalThenFirstPerfectPersistsAndReplaysNeverDowngrade()
        {
            var p=Progress();p.UnlockAll();p.Select(9);p.Complete(9);Assert.That(p.Mastery(9),Is.EqualTo(LevelMasteryState.Completed));
            var result=Run(levels[9],9);Assert.That(p.RecordPerfect(9,result),Is.True);Assert.That(p.RecordPerfect(9,result),Is.False);
            Assert.That(p.Mastery(9),Is.EqualTo(LevelMasteryState.Perfect));
            p.Complete(9);Assert.That(p.RecordPerfect(9,Run(levels[9],9,true)),Is.False);
            p.Select(0);p=Progress();Assert.That(p.IsPerfect(9),Is.True);Assert.That(p.Current,Is.Zero);
            p.Select(9);p.Complete(9);Assert.That(Progress().IsPerfect(9),Is.True);
        }
        [Test] public void OnlyActualCertificateAndMatchingSuccessfulAttemptCanRecord()
        {
            var clone=Object.Instantiate(levels[0]);
            clone.Configure(levels[0].Width+1,levels[0].Height,levels[0].MoveLimit,levels[0].TargetColor,levels[0].Placements);
            var metadata=(LevelData[])levels.Clone();metadata[0]=clone;
            var p=new LevelProgression(40,saves,metadata);p.UnlockAll();p.Select(0);p.Complete(0);var result=Run(levels[0],0);
            result.hasOptimal=result.perfectShift=true;result.verifiedOptimalMoves=1;
            Assert.That(p.RecordPerfect(0,result),Is.False);Object.DestroyImmediate(clone);
            p.Select(9);p.Complete(9);result=Run(levels[9],9);result.completed=false;Assert.That(p.RecordPerfect(9,result),Is.False);
            result.completed=true;result.levelIndex=8;Assert.That(p.RecordPerfect(9,result),Is.False);
            result.levelIndex=9;result.levelId="wrong";Assert.That(p.RecordPerfect(9,result),Is.False);
            result.levelId=levels[9].name;result.verifiedOptimalMoves++;Assert.That(p.RecordPerfect(9,result),Is.False);
            Assert.That(p.RecordPerfect(-1,result),Is.False);Assert.That(p.RecordPerfect(40,result),Is.False);
        }
        [Test] public void UndoCannotEraseEffortForPersistentPerfect()
        {
            var p=Progress();p.UnlockAll();p.Select(9);p.Complete(9);
            var result=Run(levels[9],9,true);Assert.That(result.successfulMoves,Is.EqualTo(levels[9].VerifiedOptimalMoveCount+1));
            Assert.That(result.perfectShift,Is.False);Assert.That(p.RecordPerfect(9,result),Is.False);Assert.That(Progress().IsPerfect(9),Is.False);
        }
        [TestCase(0,20)] [TestCase(1,20)]
        public void ChapterMasteryRequiresEveryCompletionAndEveryEligiblePerfectAndSurvivesReload(int chapter,int eligible)
        {
            var p=Progress();p.UnlockAll();int start=CampaignChapters.Start(chapter),end=CampaignChapters.End(chapter,40);
            for(int i=start;i<end;i++)
            {
                p.Select(i);p.Complete(i);
                if(i<end-1&&p.IsPerfectEligible(i))Assert.That(p.RecordPerfect(i,Run(levels[i],i)),Is.True);
            }
            var summary=p.ChapterSummary(chapter);Assert.That(summary.Total,Is.EqualTo(20));Assert.That(summary.Completed,Is.EqualTo(20));
            Assert.That(summary.Eligible,Is.EqualTo(eligible));Assert.That(summary.Perfect,Is.EqualTo(eligible-1));Assert.That(summary.Mastered,Is.False);
            Assert.That(p.RecordPerfect(end-1,Run(levels[end-1],end-1)),Is.True);summary=Progress().ChapterSummary(chapter);
            Assert.That(summary.Mastered,Is.True);Assert.That(summary.Perfect,Is.EqualTo(eligible));Assert.That(summary.RemainingPerfect,Is.Zero);
            Assert.That(new ChapterMastery(chapter,20,19,eligible,eligible).Mastered,Is.False);
            Assert.That(new ChapterMastery(chapter,20,20,0,0).Mastered,Is.False);
        }
        [TestCase("")] [TestCase("broken-json{}")] [TestCase("0x001?1")] [TestCase("111111111111111111111111111111111111111111111111")]
        public void MalformedPartialAndExcessMasteryCannotLoseProgressOrGrantUnverifiedPerfect(string flags)
        {
            saves.Save(new ProgressData(10,9,9));PlayerPrefs.SetString(Prefix+"Mastery.v1.Perfect",flags);
            var p=Progress();Assert.That(p.HighestUnlocked,Is.EqualTo(10));Assert.That(p.HighestCompleted,Is.EqualTo(9));
            for(int i=0;i<40;i++)Assert.That(p.IsPerfect(i),Is.EqualTo(i<=9&&p.IsPerfectEligible(i)&&i<flags.Length&&flags[i]=='1'));
            p.Select(9);Assert.That(p.ChapterSummary(0).Completed,Is.EqualTo(10));
        }
        [Test] public void LegacySaveWritesAndShortChapterLoadsRetainLaterMastery()
        {
            var p=Progress();p.UnlockAll();p.Select(39);p.Complete(39);Assert.That(p.RecordPerfect(39,Run(levels[39],39)),Is.True);
            saves.Save(new ProgressData(39,0,39));new LevelProgression(20,saves,levels).Select(0);
            // Reloading a short legacy view preserves flags even though it intentionally writes its own frontier.
            saves.Save(new ProgressData(39,0,39));Assert.That(Progress().IsPerfect(39),Is.True);
        }
        [Test] public void GroupRangesRejectInvalidChaptersAndCoverCampaignOnce()
        {
            var p=Progress();Assert.That(p.IsChapterComplete(-1),Is.False);Assert.That(p.IsChapterComplete(2),Is.False);
            Assert.That(p.ChapterSummary(2).Total,Is.Zero);Assert.That(CampaignChapters.Count(40),Is.EqualTo(2));
            Assert.That(CampaignChapters.Start(1),Is.EqualTo(20));Assert.That(CampaignChapters.End(1,40),Is.EqualTo(40));
            Assert.That(CampaignChapters.IsLast(19,40),Is.True);Assert.That(CampaignChapters.IsLast(39,40),Is.True);Assert.That(CampaignChapters.IsLast(40,40),Is.False);
        }
        [Test] public void EligibilityAuditMatchesExistingCertificatesOnly()
        {
            var p=Progress();Assert.That(Enumerable.Range(0,40).Count(p.IsPerfectEligible),Is.EqualTo(40));
            Assert.That(Enumerable.Range(0,40).Where(i=>!p.IsPerfectEligible(i)).Select(i=>i+1),Is.Empty);
        }
        [Test] public void AnalyticsCarriesMasteryCountsAndKeepsExistingResult()
        {
            var provider=new LocalAnalyticsService();var t=new TelemetryTracker(provider,()=>0);t.StartSession();t.StartLevel(levels[9],9,4);
            for(int i=0;i<4;i++)t.AcceptedTap(1,3-i,false);t.Finish(true);var result=t.Result;
            var chapter=new ChapterMastery(0,20,20,6,6);
            foreach(var name in new[]{"perfect_shift_first_earned","level_mastery_viewed","chapter_mastery_completed"})t.MasteryEvent(name,chapter,name!="level_mastery_viewed");
            var e=provider.Events.Last();Assert.That(e.chapterIndex,Is.Zero);Assert.That(e.chapterPerfectCount,Is.EqualTo(6));Assert.That(e.chapterTotalCount,Is.EqualTo(20));Assert.That(e.chapterEligibleCount,Is.EqualTo(6));Assert.That(e.firstTime,Is.True);Assert.That(t.Result,Is.SameAs(result));
        }
        [Test] public void ProtectedGameplayHintUndoCertificatesAndAssetsStayIdentical()
        {
            using var sha=SHA256.Create();
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/MasteryProtectedFiles.txt")))
            {var p=line.Split('|');Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,p[0])))).Replace("-",""),Is.EqualTo(p[1]),p[0]);}
        }
        [UnityTest] public IEnumerator PersistentMixedStatesFirstPerfectReplayChapterCompletionAndMasteredCaptures()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            saves.Save(new ProgressData(22,21,21));var seeded=Progress();seeded.Select(20);seeded.RecordPerfect(20,Run(levels[20],20));foreach(int i in new[]{6,8,9,14,17,19}){seeded.Select(i);Assert.That(seeded.RecordPerfect(i,Run(levels[i],i)),Is.True);}seeded.Select(21);
            yield return new EnterPlayMode();yield return new WaitForSecondsRealtime(.3f);
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.OpenChapter();yield return new WaitForSecondsRealtime(.3f);
            Assert.That(GameObject.Find("Level 21").transform.Find("Mastery Marker").GetComponent<Text>().text,Is.EqualTo("◆"));
            Assert.That(game.Progress.Mastery(22),Is.EqualTo(LevelMasteryState.Available));Assert.That(game.Progress.Mastery(23),Is.EqualTo(LevelMasteryState.Locked));
            Object.FindFirstObjectByType<ChapterSelect>().ShowPage(0);yield return null;
            Assert.That(GameObject.Find("Level 1").transform.Find("Mastery Marker").GetComponent<Text>().text,Is.EqualTo("✓"));
            Assert.That(GameObject.Find("Chapter Hint").GetComponent<Text>().text,Is.EqualTo("20/20 Complete · 6/20 Perfect Shift"));
            Assert.That(GameObject.Find("Mastery Replay").GetComponent<Text>().text,Is.EqualTo("14 puzzles can still be perfected."));
            AssertCleanMasteryUi();Capture("01-mixed-completed");Object.FindFirstObjectByType<ChapterSelect>().ShowPage(1);yield return null;Capture("01-mixed-states");Capture("02-chapter-summary");AssertCleanMasteryUi();
            game.CloseChapter();yield return new WaitForSecondsRealtime(.3f);yield return Solve(game);
            Assert.That(game.Progress.IsPerfect(21),Is.True);Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.Contain("NEW PERFECT SHIFT"));Capture("03-first-perfect");
            game.Restart();yield return null;Assert.That(game.Progress.IsPerfect(21),Is.True);yield return Solve(game);
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.Contain("PERFECT SHIFT").And.Not.Contain("NEW"));Capture("04-perfect-replay");
            game.UnlockAllLevels();
            for(int i=20;i<39;i++){game.Progress.Select(i);game.Progress.Complete(i);game.Progress.RecordPerfect(i,Run(gameObjectLevel(i),i));}
            Assert.That(game.SelectLevel(39),Is.True);yield return new WaitForSecondsRealtime(.3f);
            Assert.That(GameObject.Find("Goal").GetComponent<Text>().text,Is.EqualTo("Clear Both"));
            foreach(string n in new[]{"Undo","Restart","Hint","Open Chapter"})Assert.That(GameObject.Find(n),Is.Not.Null);
            Capture("07-gameplay");
            var route=game.CurrentLevel.KnownSolution.Take(5).Concat(new[]{new GridPosition(2,1),new GridPosition(3,1),new GridPosition(3,0)}).ToArray();
            foreach(var tap in route){Tap(tap);yield return Settled(game);}
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won));Assert.That(game.Progress.IsPerfect(39),Is.False);
            Assert.That(game.Progress.ChapterSummary(1).Perfect,Is.EqualTo(19));Capture("05-chapter-complete");
            game.Restart();yield return null;game.OpenSettings();GameObject.Find("Reduced Motion Setting").GetComponent<Button>().onClick.Invoke();game.CloseSettings();yield return new WaitForSecondsRealtime(.25f);
            yield return Solve(game);yield return new WaitForSecondsRealtime(1.8f);
            Assert.That(game.Progress.ChapterSummary(1).Mastered,Is.True);Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.StartWith("CHAPTER MASTERED"));Capture("06-chapter-mastered");
            Assert.That(GameObject.Find("Undo"),Is.Null);Assert.That(GameObject.Find("Hint"),Is.Null);Assert.That(GameObject.Find("Next Level"),Is.Null);
            Assert.That(GameObject.Find("Result Panel").transform.localScale,Is.EqualTo(Vector3.one));
            game.OpenChapter();yield return new WaitForSecondsRealtime(.25f);Capture("06-mastered-select");
            var select=Object.FindFirstObjectByType<ChapterSelect>();select.Language=HintLanguage.Turkish;select.ShowPage(1);yield return null;
            Assert.That(GameObject.Find("Chapter Title").GetComponent<Text>().text,Is.EqualTo(MasteryText.Mastered(HintLanguage.Turkish)));Capture("08-turkish-mastery");AssertCleanMasteryUi();
            var hud=Object.FindFirstObjectByType<GameHud>();game.CloseChapter();hud.ShowChapterMastery(game.Progress.ChapterSummary(1),HintLanguage.Turkish);hud.ShowMastery(game.Telemetry.Result,true,HintLanguage.Turkish);yield return null;Capture("09-turkish-first-perfect");
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.Contain("YENİ PERFECT SHIFT"));
            yield return new ExitPlayMode();
            Assert.That(Progress().ChapterSummary(1).Mastered,Is.True);
            LevelData gameObjectLevel(int i)=>LevelValidation.AllLevels()[i];
        }
        private static void AssertCleanMasteryUi()
        {
            Canvas.ForceUpdateCanvases();
            Assert.That(GameObject.Find("Mastery Availability"),Is.Null);
            var select=Object.FindFirstObjectByType<ChapterSelect>();
            foreach(var label in select.GetComponentsInChildren<Text>())
            {
                Assert.That(label.text,Does.Not.Contain("eligible").And.Not.Contain("uygun").And.Not.Contain("available on"));
                if(label.name=="Mastery Marker")Assert.That(label.text,Is.Not.EqualTo("·"));
                if(label.name=="Chapter Hint"||label.name=="Mastery Legend"||label.name=="Mastery Replay"||label.name=="Chapter Title")
                    Assert.That(label.preferredHeight,Is.LessThanOrEqualTo(label.rectTransform.rect.height+1),label.name+" vertical clipping");
            }
            Assert.That(GameObject.Find("Chapter Hint").GetComponent<Text>().text,Does.Contain("/20 Perfect Shift"));
        }
        private static IEnumerator Solve(PrototypeGame game){foreach(var tap in game.CurrentLevel.KnownSolution){Tap(tap);yield return Settled(game);}Assert.That(game.Board.State,Is.EqualTo(GameState.Won));}
        private static IEnumerator Settled(PrototypeGame game){float deadline=Time.realtimeSinceStartup+12;while(game.Board.State==GameState.Resolving&&Time.realtimeSinceStartup<deadline)yield return null;Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));}
        private static void Tap(GridPosition p){var piece=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(x=>x.Data.Type==PieceType.Normal&&x.Data.Active&&x.Data.Position==p);piece.GetComponentInChildren<Button>().onClick.Invoke();}
        private static void Capture(string name){Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/mastery"));SprintPresentationTests.Capture("mastery/"+name+".png");}
    }
}
