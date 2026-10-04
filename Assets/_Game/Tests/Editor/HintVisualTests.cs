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
    public sealed class HintVisualTests
    {
        private const string Prefix = "SHIFT.Tests.HintVisual.";
        [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void AllFortyStagesHaveValidTargetsAndNeverMutateTheBoard(int stage)
        {
            foreach (var level in LevelValidation.AllLevels())
            {
                var board = new BoardManager(); board.Load(level); var before = new HintBoardState(board);
                Assert.That(HintVisualCatalog.Find(level), Is.Not.Null, level.name);
                var target = new HintVisualResolver(level).Resolve(board,stage);
                Assert.That(target, Is.Not.Null, level.name); Assert.That(target.Cells.Length, Is.InRange(1,3));
                foreach (var cell in target.Cells) Assert.That(board.IsValid(cell), Is.True);
                Assert.That(before.Matches(board), Is.True); Assert.That(board.CanUndo, Is.False);
                Assert.That(target.ExactMove, Is.EqualTo(stage == 3), level.name);
                if (stage == 2) Assert.That(target.Relationship, Is.True, level.name);
            }
        }
        [Test] public void EveryCertifiedPrefixRecommendsOnlyItsVerifiedNextTap()
        {
            foreach (var level in LevelValidation.AllLevels())
            {
                var board = new BoardManager(); board.Load(level); var resolver = new HintVisualResolver(level);
                foreach (var tap in level.KnownSolution)
                {
                    var target = resolver.Resolve(board,3); Assert.That(target.ExactMove, Is.True,level.name);
                    Assert.That(target.Cells, Is.EqualTo(new[]{tap}),level.name);
                    Assert.That(board.RequestMove(tap), Is.True); board.CompleteResolution();
                }
                Assert.That(board.State, Is.EqualTo(GameState.Won)); Assert.That(resolver.Resolve(board,3), Is.Null);
            }
        }
        [Test] public void DeviatingCommittedMovesNeverClaimAnUnverifiedExactTap()
        {
            int checkedStates = 0;
            foreach (var level in LevelValidation.AllLevels())
            {
                var resolver = new HintVisualResolver(level);
                foreach (var placement in level.Placements.Where(x=>x.type==PieceType.Normal))
                {
                    var board = new BoardManager(); board.Load(level);
                    if (!board.RequestMove(placement.position)) continue; board.CompleteResolution();
                    if (board.MovesRemaining == level.MoveLimit || board.State != GameState.Playing) continue;
                    var before = new HintBoardState(board); var target = resolver.Resolve(board,3);
                    if (target != null && target.ExactMove)
                    {
                        var replay = new BoardManager(); replay.Load(level); bool matched = false;
                        foreach (var tap in level.KnownSolution) { if (new HintBoardState(replay).Matches(board)) { Assert.That(target.Cells[0],Is.EqualTo(tap)); matched=true; break; } replay.RequestMove(tap); replay.CompleteResolution(); }
                        Assert.That(matched,Is.True);
                    }
                    else checkedStates++;
                    Assert.That(before.Matches(board), Is.True);
                }
            }
            Assert.That(checkedStates,Is.GreaterThan(0));
        }
        [Test] public void UnknownOrChangedMetadataFallsBackToTextIncludingDaily()
        {
            var original=LevelValidation.AllLevels()[0]; var copy=Object.Instantiate(original);
            try
            {
                copy.name="UnknownDaily"; var board=new BoardManager();board.Load(copy);
                Assert.That(new HintVisualResolver(copy).Resolve(board,1),Is.Null);
                Assert.That(HintCatalog.Get(copy,1),Is.Not.Empty);
                copy.name=original.name; copy.Configure(4,4,9,PieceColor.Red,original.Placements);
                Assert.That(HintVisualCatalog.Find(copy),Is.Null);
                Assert.That(new HintVisualResolver(null).Resolve(board,3),Is.Null);
            }
            finally {Object.DestroyImmediate(copy);}
        }
        [Test] public void LockedAssetsAndSystemsRemainByteIdentical()
        {
            using var sha=SHA256.Create();
            foreach(var row in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/HintVisualProtectedFiles.txt")))
            {var parts=row.Split('|');Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,"..",parts[0])))).Replace("-","").ToLowerInvariant(),Is.EqualTo(parts[1]),parts[0]);}
        }
        private static HintVisualOverlay Overlay => Object.FindFirstObjectByType<HintVisualOverlay>();
        private static IEnumerator Ready(){yield return new WaitForSecondsRealtime(.4f);}
        private static void Tap(GridPosition p)=>Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(x=>x.Data.Active&&x.Data.Type==PieceType.Normal&&x.Data.Position==p).GetComponentInChildren<Button>().onClick.Invoke();
        private static IEnumerator Settle(PrototypeGame game){float end=Time.realtimeSinceStartup+20;while(game.Board.State==GameState.Resolving&&Time.realtimeSinceStartup<end)yield return null;Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));yield return Ready();}
        private static void Capture(string name){Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/hint-visual"));SprintPresentationTests.Capture("hint-visual/"+name+".png"); var mesh=Overlay.canvasRenderer.GetMesh(); if(Overlay.IsVisible) Assert.That(mesh != null && mesh.vertexCount > 0,Is.True,"Visible hint must produce render geometry"); File.AppendAllText(Path.Combine(Application.dataPath,"../Validation/hint-visual/render-state.txt"),$"{name}: visible={Overlay.IsVisible} vertices={(mesh == null ? 0 : mesh.vertexCount)} rect={Overlay.rectTransform.rect} alpha={Overlay.GetComponent<CanvasGroup>().alpha} inherited={Overlay.canvasRenderer.GetInheritedAlpha()} cull={Overlay.canvasRenderer.cull} layer={Overlay.gameObject.layer}\n");}
        private static void Hint(PrototypeGame game){if(game.Hints.Remaining==0)game.Hints.GrantReward();game.OpenHint();}
        [UnityTest] public IEnumerator RealHintStagesClearingLocalizationDailyAndPortraitCaptures()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return Ready();var game=Object.FindFirstObjectByType<PrototypeGame>();game.UnlockAllLevels();game.SelectLevel(20);yield return Ready();
            game.DailyLanguage=HintLanguage.Turkish;var before=new HintBoardState(game.Board);int hintSounds=0;
            game.GetComponent<AudioManager>().CuePlayed+=cue=>{if(cue==AudioCue.Hint)hintSounds++;};
            for(int stage=1;stage<=3;stage++)
            {
                Hint(game);Assert.That(Overlay.Target.Stage,Is.EqualTo(stage));Assert.That(Overlay.Target.ExactMove,Is.EqualTo(stage==3));
                Assert.That(before.Matches(game.Board),Is.True);Assert.That(Overlay.raycastTarget,Is.False);
                Assert.That(Overlay.GetComponent<CanvasGroup>().blocksRaycasts,Is.False);yield return Ready();Capture("0"+stage+"-campaign-tr");
                if(stage==2)Capture("05-switch-gate");
            }
            Assert.That(hintSounds,Is.EqualTo(3));game.DailyLanguage=HintLanguage.English;yield return Ready();Capture("04-next-move-en");
            var tap=Overlay.Target.Cells[0];Tap(tap);Assert.That(Overlay.IsVisible,Is.False);yield return Settle(game);Capture("10-post-move-clear");
            Overlay.Show(1);Assert.That(Overlay.IsVisible,Is.True);Assert.That(game.Undo(),Is.True);Assert.That(Overlay.IsVisible,Is.False);
            Overlay.Show(1);Assert.That(Overlay.IsVisible,Is.True);game.Restart();yield return Ready();Assert.That(Overlay.IsVisible,Is.False);
            Overlay.Show(1);game.SelectLevel(7);yield return Ready();Assert.That(Overlay.IsVisible,Is.False);
            Hint(game);Hint(game);yield return Ready();Capture("06-rotator");
            var feel=(GameFeelSettings)typeof(PrototypeGame).GetField("gameFeel",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(game);
            feel.reducedMotion=true;yield return Ready();float alpha=Overlay.GetComponent<CanvasGroup>().alpha;yield return Ready();Assert.That(Overlay.IsStatic,Is.True);Assert.That(Overlay.GetComponent<CanvasGroup>().alpha,Is.EqualTo(alpha));Assert.That(Overlay.transform.localScale,Is.EqualTo(Vector3.one));Capture("07-reduced-motion");feel.reducedMotion=false;
            game.SelectLevel(35);yield return Ready();Hint(game);Hint(game);Hint(game);yield return Ready();Capture("08-advanced-36");
            game.OpenChapter();Assert.That(Overlay.IsVisible,Is.False);game.CloseChapter();Overlay.Show(1);game.OpenDaily();Assert.That(Overlay.IsVisible,Is.False);game.CloseDaily();
            game.LocalNow=()=>new DateTime(2026,10,5);Assert.That(game.StartDaily(game.LocalNow()),Is.True);yield return Ready();
            Hint(game);Assert.That(Overlay.IsVisible,Is.True);yield return Ready();Capture("09-daily");
            game.LoadLevel(0);yield return Ready();Overlay.Show(3);Tap(game.CurrentLevel.KnownSolution[0]);yield return Settle(game);Assert.That(game.Board.State,Is.EqualTo(GameState.Won));Assert.That(Overlay.IsVisible,Is.False);
            game.LoadLevel(9);yield return Ready();Overlay.Show(1);Tap(new GridPosition(4,3));yield return Settle(game);Assert.That(Overlay.IsVisible,Is.False);
            yield return new ExitPlayMode();
        }
        private sealed class Ad : IRewardedAdService {public bool IsRewardedAdAvailable=>true;public Action<bool> Done;public void ShowRewardedAd(RewardReason reason,Action<bool> done){Done=done;}}
        [UnityTest] public IEnumerator OnlyVerifiedRewardShowsVisualAndHintAudioOnce()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return Ready();var game=Object.FindFirstObjectByType<PrototypeGame>();game.UnlockAllLevels();game.SelectLevel(9);yield return Ready();
            var ad=new Ad();game.RewardedAds=ad;while(game.Allowances.TryConsumeHint()){}int sounds=0;game.GetComponent<AudioManager>().CuePlayed+=cue=>{if(cue==AudioCue.Hint)sounds++;};
            var before=new HintBoardState(game.Board);game.OpenHint();Assert.That(Overlay.IsVisible,Is.False);game.RequestRewardedHint();ad.Done(false);Assert.That(Overlay.IsVisible,Is.False);Assert.That(sounds,Is.Zero);
            game.CloseHint();game.OpenHint();game.RequestRewardedHint();ad.Done(true);ad.Done(true);Assert.That(Overlay.IsVisible,Is.True);Assert.That(game.Hints.Stage(game.CurrentLevel),Is.EqualTo(1));Assert.That(sounds,Is.EqualTo(1));
            yield return new WaitForSecondsRealtime(3.2f);Assert.That(sounds,Is.EqualTo(1));Assert.That(before.Matches(game.Board),Is.True);
            yield return new ExitPlayMode();
        }
    }
}
