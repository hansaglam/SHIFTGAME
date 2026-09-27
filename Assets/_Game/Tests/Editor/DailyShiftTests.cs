using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
using Object=UnityEngine.Object;
namespace Shift.Game.Tests
{
    public sealed class DailyShiftTests
    {
        private const string Prefix="SHIFT.Tests.Daily.";
        private const string Key=Prefix+"Daily.v1.History";
        private LevelData[] levels;
        private DailyPool pool;
        [SetUp] public void Setup(){levels=LevelValidation.AllLevels();pool=new DailyPool(levels);new SaveService(Prefix).Reset();PlayerPrefs.DeleteKey(Key);}
        [TearDown] public void Cleanup(){new SaveService(Prefix).Reset();new SettingsService(Prefix+"Settings.").Reset();PlayerPrefs.DeleteKey(Key);}
        private static AttemptMetrics Run(DailyPuzzle puzzle,bool undo=false)
        {
            var b=new BoardManager();b.Load(puzzle.Level);var t=new TelemetryTracker(null,()=>0);t.StartLevel(puzzle.Level,puzzle.SourceIndex,puzzle.Level.VerifiedOptimalMoveCount);
            if(undo){Assert.That(b.RequestMove(puzzle.Level.KnownSolution[0]),Is.True);t.AcceptedTap(b.ReactionDepth,b.MovesRemaining,false);b.CompleteResolution();Assert.That(b.Undo(),Is.True);t.UndoUsed(b.MovesRemaining);}
            foreach(var tap in puzzle.Level.KnownSolution){Assert.That(b.RequestMove(tap),Is.True);t.AcceptedTap(b.ReactionDepth,b.MovesRemaining,b.LastReactionWasCancelled);b.CompleteResolution();}
            Assert.That(b.State,Is.EqualTo(GameState.Won));t.Finish(true);return t.Result;
        }
        [Test] public void ExplicitPoolUsesTwelveUnmodifiedVerifiedReferencesAndBalancedMetadata()
        {
            Assert.That(DailyPool.Version,Is.EqualTo(1));CollectionAssert.AreEquivalent(new[]{20,21,23,25,30,35,24,27,29,34,37,39},pool.SourceIndices);
            foreach(int index in pool.SourceIndices){Assert.That(levels[index].VerifiedOptimalMoveCount,Is.Not.Null);Assert.That(levels[index].Design,Is.Not.Null);}
            Assert.That(pool.SourceIndices.Count(i=>levels[i].ResolvedTargetColors.Count>1),Is.GreaterThan(0));
            Assert.That(pool.SourceIndices.Count(i=>levels[i].Design.difficultyBand<=DifficultyBand.Planning),Is.EqualTo(6));
        }
        [TestCase("en-US")] [TestCase("tr-TR")] [TestCase("ar-SA")]
        public void LocalCalendarMappingIsStableAcrossCultureSessionAndTimeOfDay(string culture)
        {
            var prior=CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture);var date=new DateTime(2026,9,23);
                Assert.That(DailyPool.Key(date),Is.EqualTo("2026-09-23"));var a=pool.For(date);var b=new DailyPool(levels).For(date.AddHours(23).AddMinutes(59));
                Assert.That(a.SourceIndex,Is.EqualTo(b.SourceIndex));Assert.That(a.Level,Is.SameAs(levels[a.SourceIndex]));
                Assert.That(DailyPool.TryDate(a.DateKey,out var parsed),Is.True);Assert.That(parsed,Is.EqualTo(date));
            }
            finally{CultureInfo.CurrentCulture=prior;}
        }
        [Test] public void TenYearsOfCyclesHaveNoRepeatsOrAdjacentHardPuzzlesIncludingBoundaries()
        {
            long day=new DateTime(2020,1,1).Ticks/TimeSpan.TicksPerDay;day-=day%12;
            for(int cycle=0;cycle<305;cycle++)
            {
                var dates=Enumerable.Range(0,12).Select(i=>pool.For(new DateTime((day+cycle*12+i)*TimeSpan.TicksPerDay))).ToArray();
                Assert.That(dates.Select(x=>x.SourceIndex).Distinct().Count(),Is.EqualTo(12));
                foreach(var a in dates)
                {
                    var b=pool.For(a.Date.AddDays(1));Assert.That(a.SourceIndex,Is.Not.EqualTo(b.SourceIndex));
                    Assert.That(a.Level.Design.difficultyBand>DifficultyBand.Planning&&b.Level.Design.difficultyBand>DifficultyBand.Planning,Is.False);
                }
            }
        }
        [Test] public void VersionOneDateMappingHasFixedGoldenSequence()
        {
            var date=new DateTime(2026,9,23);
            Assert.That(Enumerable.Range(0,24).Select(i=>pool.For(date.AddDays(i)).SourceIndex),Is.EqualTo(new[]{29,21,24,35,29,30,37,21,24,20,27,25,39,23,34,20,27,23,39,30,24,21,29,35}));
        }
        [Test] public void DifferentPoolVersionOrSourceNeverTransfersPerfectToAnotherPuzzle()
        {
            var puzzle=pool.For(new DateTime(2026,9,23));
            PlayerPrefs.SetString(Key,"{\"version\":1,\"records\":[{\"dateKey\":\"2026-09-23\",\"sourceId\":\"wrong\",\"sourceIndex\":29,\"poolVersion\":1,\"completed\":true,\"perfect\":true}]}");
            Assert.That(new DailySaveService(Key).Get(puzzle).perfect,Is.False);
            PlayerPrefs.SetString(Key,"{\"version\":1,\"records\":[{\"dateKey\":\"2026-09-23\",\"sourceId\":\""+puzzle.Level.name+"\",\"sourceIndex\":29,\"poolVersion\":2,\"completed\":true,\"perfect\":true}]}");
            Assert.That(new DailySaveService(Key).Get(puzzle).completed,Is.False);
        }
        [Test] public void FirstCompletionPerfectReplayAndThirtyDayHistoryPersistWithoutCampaignWrites()
        {
            var campaign=new SaveService(Prefix);campaign.Save(new ProgressData(9,9,8));campaign.RecordPerfect(6,40);
            var saves=new DailySaveService(Key);var date=new DateTime(2026,9,23);var puzzle=pool.For(date);
            Assert.That(saves.Record(puzzle,Run(puzzle,true),out var first,out var perfect),Is.True);Assert.That(first,Is.True);Assert.That(perfect,Is.False);
            Assert.That(saves.Record(puzzle,Run(puzzle),out first,out perfect),Is.True);Assert.That(first,Is.False);Assert.That(perfect,Is.True);
            saves.Record(puzzle,Run(puzzle,true),out first,out perfect);Assert.That(first||perfect,Is.False);
            for(int i=1;i<=40;i++){var old=pool.For(date.AddDays(-i));saves.Record(old,Run(old),out _,out _);}
            saves=new DailySaveService(Key);Assert.That(saves.Get(puzzle).perfect,Is.True);Assert.That(saves.Get(pool.For(date.AddDays(-40))).perfect,Is.True);
            var p=new LevelProgression(40,campaign,levels);Assert.That(p.HighestUnlocked,Is.EqualTo(9));Assert.That(p.Current,Is.EqualTo(9));Assert.That(p.HighestCompleted,Is.EqualTo(8));Assert.That(p.IsPerfect(6),Is.True);Assert.That(p.ChapterSummary(0).Perfect,Is.EqualTo(1));
        }
        [TestCase("")] [TestCase("broken-json")] [TestCase("{\"version\":99,\"records\":[]}")] [TestCase("{\"version\":1,\"records\":[null,{\"dateKey\":\"oops\"}]}")]
        public void MalformedDailyHistoryFailsSafely(string json)
        {
            PlayerPrefs.SetString(Key,json);DailySaveService saves=null;Assert.DoesNotThrow(()=>saves=new DailySaveService(Key));
            Assert.That(saves.Get(pool.For(new DateTime(2026,9,23))).completed,Is.False);
        }
        [Test] public void IncompleteOrMismatchedAttemptsNeverWriteHistory()
        {
            var puzzle=pool.For(new DateTime(2026,9,23));var saves=new DailySaveService(Key);var result=Run(puzzle);
            result.completed=false;Assert.That(saves.Record(puzzle,result,out _,out _),Is.False);
            result.completed=true;result.levelIndex=-1;Assert.That(saves.Record(puzzle,result,out _,out _),Is.False);
            Assert.That(PlayerPrefs.HasKey(Key),Is.False);
        }
        [Test] public void ArchiveIsExactlyTodayAndSixPreviousDatesRegardlessOfStoredHistory()
        {
            var today=new DateTime(2026,3,1);var archive=pool.Archive(today);Assert.That(archive.Length,Is.EqualTo(7));
            Assert.That(archive.Select(p=>p.Date),Is.EqualTo(Enumerable.Range(0,7).Select(i=>today.AddDays(-i))));
            Assert.That(archive.Last().DateKey,Is.EqualTo("2026-02-23"));
        }
        [Test] public void GameplayCampaignPersistenceHintsAndCertificatesRemainByteIdentical()
        {
            using var sha=SHA256.Create();
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/DailyProtectedFiles.txt")))
            {var parts=line.Split('|');Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,parts[0])))).Replace("-",""),Is.EqualTo(parts[1]),parts[0]);}
        }
        [UnityTest] public IEnumerator DailyGameplayArchiveCompletionSeparationAndLocalizedScreens()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            var campaign=new SaveService(Prefix);campaign.Save(new ProgressData(9,9,8));campaign.RecordPerfect(6,40);
            var now=new DateTime(2026,9,23);while(pool.For(now).SourceIndex!=39)now=now.AddDays(1);
            var seeded=new DailySaveService(Key);var yesterday=pool.For(now.AddDays(-1));seeded.Record(yesterday,Run(yesterday,true),out _,out _);
            var prior=pool.For(now.AddDays(-2));seeded.Record(prior,Run(prior),out _,out _);
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.LocalNow=()=>now;
            var provider=(LocalAnalyticsService)typeof(TelemetryTracker).GetField("provider",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game.Telemetry);
            game.OpenChapter();yield return new WaitForSecondsRealtime(.3f);Assert.That(GameObject.Find("Daily Shift Entry"),Is.Not.Null);Capture("01-entry");
            GameObject.Find("Daily Shift Entry").GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.3f);Capture("02-today");Capture("06-archive-mixed");AssertDailyBounds();
            Assert.That(GameObject.Find("Daily Date 0").GetComponentInChildren<Text>().text,Does.Contain("Today"));
            Assert.That(GameObject.Find("Daily Date 1").GetComponentInChildren<Text>().text,Does.Contain("✓"));Assert.That(GameObject.Find("Daily Date 2").GetComponentInChildren<Text>().text,Does.Contain("◆"));
            Assert.That(provider.Events.Count(e=>e.name=="daily_shift_opened"),Is.EqualTo(1));Assert.That(provider.Events.Count(e=>e.name=="daily_archive_opened"),Is.EqualTo(1));
            GameObject.Find("Play Today").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.That(game.IsDaily,Is.True);Assert.That(game.CurrentLevel,Is.SameAs(LevelValidation.AllLevels()[39]));Assert.That(GameObject.Find("Goal").GetComponent<Text>().text,Is.EqualTo("Clear Both"));
            Assert.That(GameObject.Find("Level Title").GetComponent<Text>().text,Does.StartWith("DAILY SHIFT").And.Not.Contain("40"));yield return new WaitForSecondsRealtime(.35f);Capture("03-gameplay");
            int allowance=game.Hints.Remaining;game.OpenHint();Assert.That(game.Hints.Remaining,Is.EqualTo(allowance-1));Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.Contain(HintCatalog.Get(game.CurrentLevel,1)));
            Tap(game.CurrentLevel.KnownSolution[0]);yield return Settled(game);Assert.That(game.Undo(),Is.True);yield return Solve(game);
            Assert.That(game.Telemetry.Result.perfectShift,Is.False);Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Is.EqualTo("DAILY SHIFT COMPLETE"));Capture("04-complete");
            Assert.That(GameObject.Find("Next Level"),Is.Null);Assert.That(game.NextLevel(),Is.False);Assert.That(GameObject.Find("Back to Daily"),Is.Not.Null);
            Assert.That(game.DailySaves.Get(game.ActiveDaily).completed,Is.True);Assert.That(game.DailySaves.Get(game.ActiveDaily).perfect,Is.False);
            game.Restart();yield return null;Assert.That(game.Hints.Remaining,Is.EqualTo(allowance-1));yield return Solve(game);
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Is.EqualTo("DAILY PERFECT SHIFT"));Capture("05-perfect");
            Assert.That(provider.Events.Count(e=>e.name=="daily_shift_completed"),Is.EqualTo(1));Assert.That(provider.Events.Count(e=>e.name=="daily_shift_perfect"),Is.EqualTo(1));
            Assert.That(provider.Events.Count(e=>e.name=="daily_shift_replayed"),Is.EqualTo(1));Assert.That(provider.Events.Count(e=>e.name=="daily_shift_started"),Is.EqualTo(2));
            Assert.That(provider.Events.Single(e=>e.name=="daily_shift_completed").completedBefore,Is.False);
            Assert.That(provider.Events.Single(e=>e.name=="daily_shift_perfect").completedBefore,Is.True);
            game.Restart();yield return null;yield return Solve(game);
            Assert.That(provider.Events.Count(e=>e.name=="daily_shift_completed"),Is.EqualTo(1));Assert.That(provider.Events.Count(e=>e.name=="daily_shift_perfect"),Is.EqualTo(1));
            Assert.That(GameObject.Find("Undo"),Is.Null);Assert.That(GameObject.Find("Hint"),Is.Null);
            Assert.That(game.Progress.Current,Is.EqualTo(9));Assert.That(game.Progress.HighestCompleted,Is.EqualTo(8));Assert.That(game.Progress.ChapterSummary(0).Perfect,Is.EqualTo(1));Assert.That(game.Progress.IsPerfect(39),Is.False);
            GameObject.Find("Back to Daily").GetComponent<Button>().onClick.Invoke();yield return null;Assert.That(GameObject.Find("Play Today").GetComponentInChildren<Text>().text,Does.Contain("◆"));
            GameObject.Find("Daily Date 1").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.That(game.ActiveDaily.Date,Is.EqualTo(now.AddDays(-1)));Assert.That(game.CurrentLevel,Is.SameAs(LevelValidation.AllLevels()[yesterday.SourceIndex]));yield return new WaitForSecondsRealtime(.35f);Capture("07-archive-replay");
            Assert.That(provider.Events.Last(e=>e.name=="daily_archive_selected").dateKey,Is.EqualTo(yesterday.DateKey));
            // Pin an in-flight attempt across midnight; reopening the archive rolls today forward.
            var pinned=game.ActiveDaily.Date;now=now.AddDays(1);Assert.That(game.ActiveDaily.Date,Is.EqualTo(pinned));game.OpenDaily();yield return null;
            Assert.That(GameObject.Find("Play Today").GetComponentInChildren<Text>().text,Does.Contain(DailyText.Date(now,HintLanguage.English)));
            game.DailyLanguage=HintLanguage.Turkish;game.OpenDaily();yield return null;Capture("09-turkish");AssertDailyBounds();
            Assert.That(GameObject.Find("Daily Title").GetComponent<Text>().text,Is.EqualTo("GÜNÜN SHIFT'İ"));
            game.CloseDaily();Assert.That(game.SelectLevel(9),Is.True);yield return null;Assert.That(game.IsDaily,Is.False);yield return new WaitForSecondsRealtime(.35f);Capture("08-campaign");
            Assert.That(game.StartDaily(now.AddDays(-7)),Is.False);Assert.That(game.IsDaily,Is.False);game.CloseDaily();
            Assert.That(game.StartDaily(now.AddDays(1)),Is.False);Assert.That(game.IsDaily,Is.False);game.CloseDaily();
            game.OpenChapter();yield return new WaitForSecondsRealtime(.3f);Assert.That(GameObject.Find("Level 10"),Is.Not.Null);
            yield return new ExitPlayMode();
            var reloaded=new LevelProgression(40,new SaveService(Prefix),LevelValidation.AllLevels());Assert.That(reloaded.HighestCompleted,Is.EqualTo(8));Assert.That(reloaded.ChapterSummary(0).Perfect,Is.EqualTo(1));
            Assert.That(new DailySaveService(Key).Get(pool.For(now.AddDays(-1))).perfect,Is.True);
        }
        private static void AssertDailyBounds()
        {
            Canvas.ForceUpdateCanvases();var panel=Object.FindFirstObjectByType<DailyShiftPanel>();
            Assert.That(panel.GetComponentsInChildren<Button>().Count(x=>x.name.StartsWith("Daily Date ")),Is.EqualTo(7));
            foreach(var text in panel.GetComponentsInChildren<Text>())Assert.That(text.preferredHeight,Is.LessThanOrEqualTo(text.rectTransform.rect.height+1),text.name);
        }
        private static IEnumerator Solve(PrototypeGame game){foreach(var tap in game.CurrentLevel.KnownSolution){Tap(tap);yield return Settled(game);}Assert.That(game.Board.State,Is.EqualTo(GameState.Won));}
        private static IEnumerator Settled(PrototypeGame game){float end=Time.realtimeSinceStartup+12;while(game.Board.State==GameState.Resolving&&Time.realtimeSinceStartup<end)yield return null;Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));}
        private static void Tap(GridPosition p){Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(x=>x.Data.Active&&x.Data.Type==PieceType.Normal&&x.Data.Position==p).GetComponentInChildren<Button>().onClick.Invoke();}
        private static void Capture(string name){Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/daily"));SprintPresentationTests.Capture("daily/"+name+".png");}
    }
}
