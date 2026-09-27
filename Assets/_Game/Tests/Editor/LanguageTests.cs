using System;
using System.Collections;
using System.Collections.Generic;
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
    public sealed class LanguageTests
    {
        private const string Prefix = "SHIFT.Tests.Language.";
        private const string Key = Prefix + "Unit.v1";
        [TearDown] public void Cleanup() { PlayerPrefs.DeleteKey(Key); }
        [TestCase(SystemLanguage.Turkish, GameLanguage.Turkish)]
        [TestCase(SystemLanguage.English, GameLanguage.English)]
        [TestCase(SystemLanguage.German, GameLanguage.English)]
        [TestCase(SystemLanguage.French, GameLanguage.English)]
        [TestCase(SystemLanguage.Portuguese, GameLanguage.English)]
        [TestCase(SystemLanguage.Unknown, GameLanguage.English)]
        public void FreshDeviceResolution(SystemLanguage device, GameLanguage expected)
        { PlayerPrefs.DeleteKey(Key); Assert.That(new GameLanguageService(device, Key).CurrentLanguage, Is.EqualTo(expected)); Assert.That(PlayerPrefs.GetString(Key), Is.EqualTo(expected.ToString())); }
        [TestCase(GameLanguage.English, SystemLanguage.Turkish)]
        [TestCase(GameLanguage.Turkish, SystemLanguage.English)]
        public void ExplicitChoicePersistsAndOverridesSystem(GameLanguage choice, SystemLanguage device)
        { var service = new GameLanguageService(device, Key); service.Select(choice); Assert.That(PlayerPrefs.GetString(Key), Is.EqualTo(choice.ToString())); Assert.That(new GameLanguageService(device, Key).CurrentLanguage, Is.EqualTo(choice)); }
        [TestCase("", SystemLanguage.Turkish, GameLanguage.Turkish)]
        [TestCase("3", SystemLanguage.English, GameLanguage.English)]
        [TestCase("german", SystemLanguage.Turkish, GameLanguage.Turkish)]
        [TestCase("Turkish ", SystemLanguage.English, GameLanguage.English)]
        public void MalformedSaveUsesDevice(string value, SystemLanguage device, GameLanguage expected)
        { PlayerPrefs.SetString(Key, value); Assert.That(new GameLanguageService(device, Key).CurrentLanguage, Is.EqualTo(expected)); }
        [Test] public void FirstDeviceResolutionIsNotRepeatedOnLaterLaunches()
        { PlayerPrefs.DeleteKey(Key); new GameLanguageService(SystemLanguage.Turkish, Key); Assert.That(new GameLanguageService(SystemLanguage.English, Key).CurrentLanguage, Is.EqualTo(GameLanguage.Turkish)); }
        [Test] public void EventOnlyOnRealChangesAndInvalidEnumRejected()
        {
            PlayerPrefs.SetString(Key,"English"); var service = new GameLanguageService(SystemLanguage.English,Key); int calls = 0; service.LanguageChanged += () => calls++;
            service.Select(GameLanguage.English); service.Select(GameLanguage.Turkish); service.Select(GameLanguage.Turkish); service.Select(GameLanguage.English);
            Assert.That(calls,Is.EqualTo(2)); Assert.Throws<ArgumentOutOfRangeException>(()=>service.Select((GameLanguage)8));
        }
        [Test] public void CatalogCoverageAndFortyTitlesAreComplete()
        {
            var rows = new List<string>{"key,en,tr"};
            foreach(var key in LocalizationCatalog.Keys)
            {
                string en=LocalizationCatalog.Format(key,GameLanguage.English),tr=LocalizationCatalog.Format(key,GameLanguage.Turkish);
                if(key!="chain.Normal"){Assert.That(en,Is.Not.Empty,key);Assert.That(tr,Is.Not.Empty,key);}
                Assert.That(System.Text.RegularExpressions.Regex.Matches(en,@"\{\d+\}").Cast<System.Text.RegularExpressions.Match>().Select(m=>m.Value).OrderBy(x=>x),
                    Is.EqualTo(System.Text.RegularExpressions.Regex.Matches(tr,@"\{\d+\}").Cast<System.Text.RegularExpressions.Match>().Select(m=>m.Value).OrderBy(x=>x)),key);
                rows.Add(key+",present,present");
            }
            var levels=LevelValidation.AllLevels();Assert.That(levels.Length,Is.EqualTo(40));
            var titles=new HashSet<string>();
            foreach(var level in levels)
            {
                string key="level."+level.name+".title";Assert.That(LocalizationCatalog.Contains(key),Is.True);Assert.That(LocalizationCatalog.Contains("level."+level.name+".instruction"),Is.True);
                Assert.That(LocalizationCatalog.Format(key,GameLanguage.English),Is.EqualTo(level.DisplayTitle));
                Assert.That(titles.Add(LocalizationCatalog.Format(key,GameLanguage.Turkish)),Is.True);
                for(int stage=1;stage<=3;stage++)foreach(HintLanguage l in Enum.GetValues(typeof(HintLanguage)))Assert.That(HintCatalog.Get(level,stage,l),Is.Not.Empty);
            }
            Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/language"));File.WriteAllLines(Path.Combine(Application.dataPath,"../Validation/language/coverage.csv"),rows);
        }
        [TestCase(PieceColor.Red,"Kırmızıyı temizle")]
        [TestCase(PieceColor.Blue,"Maviyi temizle")]
        [TestCase(PieceColor.Green,"Yeşili temizle")]
        [TestCase(PieceColor.Yellow,"Sarıyı temizle")]
        public void SingleObjectives(PieceColor color,string expected)
        {Assert.That(ObjectiveText.Format(new[]{color},GameLanguage.Turkish),Is.EqualTo(expected));Assert.That(ObjectiveText.Format(new[]{color},GameLanguage.English),Is.EqualTo("Clear "+color));}
        [Test] public void MultiObjectivesAndDateDisplayLeaveInvariantKeys()
        {
            Assert.That(ObjectiveText.Format(new[]{PieceColor.Red,PieceColor.Blue},GameLanguage.Turkish),Is.EqualTo("İkisini temizle"));
            Assert.That(ObjectiveText.Format(new[]{PieceColor.Red,PieceColor.Blue,PieceColor.Yellow},GameLanguage.Turkish),Is.EqualTo("Hepsini temizle"));
            var date=new DateTime(2026,10,5);Assert.That(DailyText.Date(date,HintLanguage.English),Is.EqualTo("5 October"));Assert.That(DailyText.Date(date,HintLanguage.Turkish),Is.EqualTo("5 Ekim"));
            var pool=new DailyPool(LevelValidation.AllLevels());Assert.That(pool.For(date).DateKey,Is.EqualTo("2026-10-05"));
        }
        [Test] public void ProtectedGameplayFilesAreByteIdentical()
        {
            using var sha=SHA256.Create();
            foreach(string line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/LanguageProtectedFiles.txt")))
            {
                int split=line.IndexOf(' ');string hash=line.Substring(0,split),path=line.Substring(split+1);
                string actual=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,"..",path)))).Replace("-","").ToLowerInvariant();
                Assert.That(actual,Is.EqualTo(hash),path);
            }
        }
        private sealed class Ads : IRewardedAdService
        {public bool IsRewardedAdAvailable=>true;public Action<bool> Done;public int Calls;public void ShowRewardedAd(RewardReason reason,Action<bool> completed){Calls++;Done=completed;}}
        private static string BoardState(PrototypeGame g)=>g.Board.State+"|"+g.Board.MovesRemaining+"|"+g.Board.CanUndo+"|"+string.Join(";",g.Board.Pieces.Select(p=>p.Id+":"+p.Active+":"+p.Position+":"+p.Direction+":"+p.GateOpen));
        private static string SessionState(PrototypeGame g)=>BoardState(g)+"|"+g.Hints.Stage(g.CurrentLevel)+"|"+g.Allowances.HintsRemaining+"|"+g.Allowances.UndosRemaining+"|"+g.ActiveDaily?.DateKey+"|"+JsonUtility.ToJson(g.Telemetry.Result);
        private static Text Text(string name)=>GameObject.Find(name).GetComponent<Text>();
        private static IEnumerator Ready(){yield return new WaitForSecondsRealtime(.4f);}
        private static void Tap(GridPosition p)=>Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(x=>x.Data.Active&&x.Data.Type==PieceType.Normal&&x.Data.Position==p).GetComponentInChildren<Button>().onClick.Invoke();
        private static IEnumerator Move(PrototypeGame g,GridPosition p){Tap(p);float end=Time.realtimeSinceStartup+15;while(g.Board.State==GameState.Resolving&&Time.realtimeSinceStartup<end)yield return null;Assert.That(g.Board.State,Is.Not.EqualTo(GameState.Resolving));yield return new WaitForSecondsRealtime(.12f);}
        private static void Capture(string name){Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/language"));SprintPresentationTests.Capture("language/"+name+".png");}
        private static void Switch(PrototypeGame g,GameLanguage l)
        {
            object snapshot=typeof(BoardManager).GetField("undoSnapshot",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(g.Board); string before=SessionState(g);var provider=(LocalAnalyticsService)typeof(TelemetryTracker).GetField("provider",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(g.Telemetry);int events=provider.Events.Count();
            GameLanguageService.Shared.Select(l);Assert.That(SessionState(g),Is.EqualTo(before));Assert.That(provider.Events.Count(),Is.EqualTo(events)); Assert.That(typeof(BoardManager).GetField("undoSnapshot",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(g.Board),Is.SameAs(snapshot));
        }
        private static AttemptMetrics Certified(LevelData level,int index)
        {
            var b=new BoardManager();b.Load(level);var t=new TelemetryTracker(new NullAnalyticsService(),()=>0);t.StartSession();t.StartLevel(level,index,level.VerifiedOptimalMoveCount);
            foreach(var p in level.KnownSolution){b.RequestMove(p);t.AcceptedTap(b.ReactionDepth,b.MovesRemaining,b.LastReactionWasCancelled);b.CompleteResolution();}t.Finish(true);return t.Result;
        }
        [UnityTest] public IEnumerator GateSnapshotMasteredAndDailyResultRefreshArePresentationOnly()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix+"States.");
            yield return new EnterPlayMode();yield return Ready();
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.LocalNow=()=>new DateTime(2026,10,5);game.UnlockAllLevels();game.SelectLevel(39);yield return Ready();
            var audio=game.GetComponent<AudioManager>();var cues=new List<AudioCue>();audio.CuePlayed+=cues.Add;
            var pulses=new List<HapticCue>();((HapticService)typeof(PrototypeGame).GetField("haptics",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game)).PulseSent+=pulses.Add;
            bool toggled=false;
            foreach(var p in game.CurrentLevel.KnownSolution)
            {
                yield return Move(game,p);
                toggled |= game.Board.Actions.Any(a=>a.Type==BoardActionType.GateOpened||a.Type==BoardActionType.GateClosed);
                cues.Clear();pulses.Clear();Switch(game,GameLanguage.Turkish);Switch(game,GameLanguage.English);
                Assert.That(cues,Is.Empty);Assert.That(pulses,Is.Empty);
            }
            Assert.That(toggled,Is.True);Assert.That(game.Board.State,Is.EqualTo(GameState.Won));
            var levels=LevelValidation.AllLevels();for(int i=20;i<40;i++){game.Progress.Select(i);game.Progress.Complete(i);game.Progress.RecordPerfect(i,Certified(levels[i],i));}
            game.OpenChapter();yield return Ready();Object.FindFirstObjectByType<ChapterSelect>().ShowPage(1);
            Switch(game,GameLanguage.Turkish);Assert.That(Text("Chapter Title").text,Is.EqualTo("BÖLÜMDE USTALAŞILDI"));Capture("14-tr-mastered");
            Switch(game,GameLanguage.English);Assert.That(Text("Chapter Title").text,Is.EqualTo("CHAPTER MASTERED"));Capture("15-en-mastered");game.CloseChapter();
            game.StartDaily(game.LocalNow());yield return Ready();foreach(var p in game.CurrentLevel.KnownSolution)yield return Move(game,p);
            cues.Clear();Switch(game,GameLanguage.Turkish);Assert.That(Text("Status").text,Is.EqualTo("GÜNLÜK PERFECT SHIFT"));Assert.That(cues,Is.Empty);Capture("16-tr-daily-perfect");
            game.OpenDaily();yield return Ready();Assert.That(GameObject.Find("Daily Date 0").GetComponentInChildren<Text>().text,Does.Contain("Perfect Shift"));
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator LiveSwitchAllSurfacesRewardsAndPortraitEvidence()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return Ready();
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.LocalNow=()=>new DateTime(2026,10,5);game.UnlockAllLevels();game.SelectLevel(9);yield return Ready();
            var language=GameLanguageService.Shared;var cues=new List<AudioCue>();game.GetComponent<AudioManager>().CuePlayed+=cues.Add;
            Capture("01-en-gameplay");Assert.That(Text("Goal").text,Is.EqualTo("Clear Red"));
            Switch(game,GameLanguage.Turkish);Assert.That(Text("Goal").text,Is.EqualTo("Kırmızıyı temizle"));Assert.That(Text("Moves Label").text,Is.EqualTo("Hamle:"));Capture("02-tr-gameplay");
            Assert.That(GameObject.Find("Undo").GetComponentInChildren<Text>().text,Is.EqualTo("Geri Al"));
            game.OpenSettings();yield return Ready();Assert.That(Text("Settings Title").text,Is.EqualTo("AYARLAR"));Capture("04-tr-settings");
            var panel=Object.FindFirstObjectByType<SettingsPanel>();Assert.That(panel.GetComponentsInChildren<Button>().Select(b=>b.name),Is.EquivalentTo(new[]{"Sound Setting","Haptics Setting","Reduced Motion Setting","Language Setting","Close Settings"}));
            bool sound=game.Settings.Sound,haptics=game.Settings.Haptics,motion=game.Settings.ReducedMotion;
            GameObject.Find("Language Setting").GetComponent<Button>().onClick.Invoke();Assert.That(language.CurrentLanguage,Is.EqualTo(GameLanguage.English));Assert.That(Text("Settings Title").text,Is.EqualTo("SETTINGS"));Capture("03-en-settings");
            Assert.That(game.Settings.Sound,Is.EqualTo(sound));Assert.That(game.Settings.Haptics,Is.EqualTo(haptics));Assert.That(game.Settings.ReducedMotion,Is.EqualTo(motion));game.CloseSettings();yield return Ready();
            game.OpenHint();Assert.That(Text("Status").text,Does.Contain(HintCatalog.Get(game.CurrentLevel,1,HintLanguage.English)));
            yield return Move(game,game.CurrentLevel.KnownSolution[0]);string committed=BoardState(game);Assert.That(game.Board.CanUndo,Is.True);cues.Clear();
            Switch(game,GameLanguage.Turkish);Assert.That(cues,Is.Empty);game.OpenSettings();yield return Ready();game.CloseSettings();yield return Ready();Capture("12-live-switch-no-reset");
            Assert.That(BoardState(game),Is.EqualTo(committed));Assert.That(game.Hints.Stage(game.CurrentLevel),Is.EqualTo(1));Assert.That(game.Undo(),Is.True);
            Switch(game,GameLanguage.English);game.OpenHint();Switch(game,GameLanguage.Turkish);Assert.That(Text("Status").text,Does.Contain(HintCatalog.Get(game.CurrentLevel,2,HintLanguage.Turkish)));Assert.That(game.Hints.Stage(game.CurrentLevel),Is.EqualTo(2));
            game.OpenChapter();yield return Ready();Capture("06-tr-level-select");Switch(game,GameLanguage.English);Capture("05-en-level-select");Assert.That(Text("Chapter Title").text,Is.EqualTo("CHAPTER 1"));
            game.OpenDaily();yield return Ready();Capture("07-en-daily");Switch(game,GameLanguage.Turkish);Capture("08-tr-daily");Assert.That(Text("Daily Title").text,Is.EqualTo("GÜNÜN SHIFT'İ"));game.CloseDaily();
            game.StartDaily(game.LocalNow());yield return Ready();string daily=game.ActiveDaily.DateKey;Switch(game,GameLanguage.English);Switch(game,GameLanguage.Turkish);Assert.That(game.ActiveDaily.DateKey,Is.EqualTo(daily));
            game.SelectLevel(39);yield return Ready();Capture("11-tr-level40");Assert.That(Text("Goal").text,Is.EqualTo("İkisini temizle"));Assert.That(Text("Level Title").text,Does.Contain("Renk Makinesi"));
            game.SelectLevel(9);yield return Ready();var ads=new Ads();game.RewardedAds=ads;while(game.Allowances.HintsRemaining>0)game.Allowances.TryConsumeHint();
            game.OpenHint();yield return Ready();Capture("09-tr-rewarded-hint");Switch(game,GameLanguage.English);Assert.That(Text("Hint Title").text,Is.EqualTo("Need another hint?"));
            game.RequestRewardedHint();int calls=ads.Calls;Switch(game,GameLanguage.Turkish);Assert.That(Text("Hint Availability").text,Is.EqualTo("Reklam sonucu bekleniyor…"));Assert.That(ads.Calls,Is.EqualTo(calls));ads.Done(true);ads.Done(true);Assert.That(game.Hints.Stage(game.CurrentLevel),Is.EqualTo(3));
            yield return Move(game,game.CurrentLevel.KnownSolution[0]);while(game.Allowances.UndosRemaining>0)game.Allowances.TryUndo(()=>true);Assert.That(game.Undo(),Is.False);yield return Ready();Capture("10-tr-rewarded-undo");
            Switch(game,GameLanguage.English);Assert.That(Text("Hint Title").text,Is.EqualTo("Need another Undo?"));game.RequestRewardedHint();Switch(game,GameLanguage.Turkish);ads.Done(false);Assert.That(game.Allowances.UndosRemaining,Is.Zero);
            game.RequestRewardedHint();Switch(game,GameLanguage.English);ads.Done(true);ads.Done(true);Assert.That(game.Allowances.UndosRemaining,Is.EqualTo(1));Assert.That(game.Board.CanUndo,Is.True);Assert.That(game.Undo(),Is.True);
            // Open/close cycles cannot accumulate listeners. Refresh cannot replay results or telemetry.
            yield return Ready();int listeners=language.ListenerCount;
            for(int i=0;i<5;i++){game.OpenSettings();yield return Ready();Switch(game,GameLanguage.Turkish);Switch(game,GameLanguage.English);game.CloseSettings();yield return Ready();}
            Assert.That(language.ListenerCount,Is.EqualTo(listeners));
            game.SelectLevel(0);yield return Ready();foreach(var p in game.CurrentLevel.KnownSolution)yield return Move(game,p);
            cues.Clear();Switch(game,GameLanguage.Turkish);Assert.That(cues,Is.Empty);Assert.That(Text("Status").text,Does.Not.Contain("GOAL"));yield return new WaitForSecondsRealtime(1.7f);Assert.That(Text("Status").text,Does.Not.Contain("GOAL"));Capture("13-tr-result");
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");const string glyphs="ÇçĞğİıÖöŞşÜü";font.RequestCharactersInTexture(glyphs,32,FontStyle.Normal);
            foreach(char c in glyphs){Assert.That(font.HasCharacter(c),Is.True,c.ToString());Assert.That(font.GetCharacterInfo(c,out var info,32),Is.True,c.ToString());Assert.That(info.advance,Is.GreaterThan(0));}
            Assert.That(new GameLanguageService(SystemLanguage.English,Prefix+"Language.v1").CurrentLanguage,Is.EqualTo(GameLanguage.Turkish));
            game.Restart();yield return Ready();Assert.That(Text("Moves Label").text,Is.EqualTo("Hamle:"));Assert.That(language.ListenerCount,Is.LessThan(listeners+10));
            UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene,UnityEngine.SceneManagement.LoadSceneMode> loaded=(_,__) =>
            {
                var reloaded=Object.FindFirstObjectByType<PrototypeGame>();
                typeof(PrototypeGame).GetField("saveKeyPrefix",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(reloaded,Prefix);
                typeof(PrototypeGame).GetField("settingsKeyPrefix",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(reloaded,Prefix+"Settings.");
            };
            UnityEngine.SceneManagement.SceneManager.sceneLoaded+=loaded;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Prototype");yield return Ready();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded-=loaded;
            Assert.That(language.ListenerCount,Is.Zero,"Destroyed scene releases the prior service");
            Assert.That(GameLanguageService.Shared.CurrentLanguage,Is.EqualTo(GameLanguage.Turkish));
            Assert.That(Text("Moves Label").text,Is.EqualTo("Hamle:"));
            yield return new ExitPlayMode();
        }
    }
}
