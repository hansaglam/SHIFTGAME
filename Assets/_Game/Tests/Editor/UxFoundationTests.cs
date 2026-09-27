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
    public sealed class UxFoundationTests
    {
        private const string Prefix = "SHIFT.Tests.UxFoundation.";
        [TearDown] public void Cleanup() { new SaveService(Prefix).Reset(); new SettingsService(Prefix+"Settings.").Reset(); }
        private static string State(BoardManager b) => b.MovesRemaining + ":" + b.State + ":" + string.Join(";", b.Pieces.Select(p => $"{p.Id}:{p.Position.x},{p.Position.y}:{p.Direction}:{p.Active}:{p.GateOpen}"));
        private static string Actions(BoardManager b) => string.Join(";", b.Actions.Select(a => $"{a.Type}:{a.PieceId}:{a.From.x},{a.From.y}:{a.To.x},{a.To.y}:{a.Direction}"));

        [Test] public void AllProtectedAssetsSourcesAndCertificatesRemainByteIdentical()
        {
            using var sha=System.Security.Cryptography.SHA256.Create();
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/UxProtectedFiles.txt")))
            {
                var pair=line.Split('|');var hash=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,pair[0])))).Replace("-","");
                Assert.That(hash,Is.EqualTo(pair[1]),pair[0]);
            }
        }
        [TestCase(1600)] [TestCase(1920)] [TestCase(2400)]
        public void ControlGeometryFitsInsetSafeAreaAndKeepsBoardSpace(int height)
        {
            var root=new GameObject("Layout fixture",typeof(RectTransform),typeof(Canvas));
            root.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            var rect=root.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(1080,height);
            var rounded=PlaceholderVisuals.CreateRounded();var circle=PlaceholderVisuals.CreateCircle();
            try
            {
                var safe=PlaceholderVisuals.Rect("Inset",rect,new Vector2(.025f,.035f),new Vector2(.975f,.945f));
                var hud=safe.gameObject.AddComponent<GameHud>();var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                hud.Build(font,LevelValidation.AllLevels()[39],rounded,new GameFeelSettings{reducedMotion=true},()=>{},circle);
                hud.BuildNavigation(font,rounded,()=>{},()=>{},true);hud.BuildUtilities(font,rounded,()=>{},()=>{});
                Canvas.ForceUpdateCanvases();
                var controls=new[]{"Undo","Restart","Hint"}.Select(n=>(RectTransform)safe.Find(n)).ToArray();
                var levels=RectTransformUtility.CalculateRelativeRectTransformBounds(safe,safe.Find("Open Chapter"));
                var card=RectTransformUtility.CalculateRelativeRectTransformBounds(safe,safe.Find("Result Panel"));
                foreach(var control in controls)
                {
                    Assert.That(control.rect.height,Is.GreaterThan(88));
                    var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(safe,control);
                    Assert.That(bounds.min.y,Is.GreaterThan(levels.max.y));Assert.That(bounds.max.y,Is.LessThan(card.min.y));
                    Assert.That(bounds.min.x,Is.GreaterThan(safe.rect.xMin));Assert.That(bounds.max.x,Is.LessThan(safe.rect.xMax));
                    foreach(var icon in control.GetComponentsInChildren<IdentityIcon>())Assert.That(icon.raycastTarget,Is.False);
                }
                Assert.That(controls[0].anchoredPosition.y,Is.EqualTo(controls[2].anchoredPosition.y));
            }
            finally { Object.DestroyImmediate(root);Object.DestroyImmediate(rounded.texture);Object.DestroyImmediate(rounded);Object.DestroyImmediate(circle.texture);Object.DestroyImmediate(circle); }
        }

        [Test] public void AllCampaignNonWinningMovesRestoreEveryFieldAndReplayIdentically()
        {
            int delivered = 0, turned = 0, gates = 0;
            foreach (var level in LevelValidation.AllLevels())
            {
                var b = new BoardManager(); b.Load(level); Assert.That(b.CanUndo, Is.False); Assert.That(b.Undo(), Is.False);
                foreach (var tap in level.KnownSolution)
                {
                    string before = State(b); Assert.That(b.RequestMove(tap), Is.True);
                    string actions = Actions(b); Assert.That(b.CanUndo, Is.False); Assert.That(b.Undo(), Is.False);
                    b.CompleteResolution(); string after = State(b);
                    if (b.State == GameState.Won) { Assert.That(b.Undo(), Is.False); continue; }
                    Assert.That(b.CanUndo, Is.True, level.name);
                    delivered += b.Actions.Count(a => a.Type == BoardActionType.Deliver);
                    turned += b.Actions.Count(a => a.Type == BoardActionType.Turn);
                    gates += b.Actions.Count(a => a.Type == BoardActionType.GateOpened || a.Type == BoardActionType.GateClosed);
                    Assert.That(b.Undo(), Is.True); Assert.That(State(b), Is.EqualTo(before), level.name);
                    foreach (var p in b.Pieces.Where(p => p.Active && p.Movable)) Assert.That(b.GetOccupant(p.Position), Is.SameAs(p));
                    Assert.That(b.Actions, Is.Empty); Assert.That(b.CanUndo, Is.False); Assert.That(b.Undo(), Is.False);
                    b.RequestMove(tap); Assert.That(Actions(b), Is.EqualTo(actions), level.name);
                    b.CompleteResolution(); Assert.That(State(b), Is.EqualTo(after), level.name);
                }
                Assert.That(b.State, Is.EqualTo(GameState.Won), level.name);
            }
            Assert.That(delivered, Is.GreaterThan(0)); Assert.That(turned, Is.GreaterThan(0)); Assert.That(gates, Is.GreaterThan(0));
        }
        [Test] public void BlockedAndCancelledReactionsNeverCreateHistory()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            try
            {
                PiecePlacement P(PieceType type,int x,int y,Direction d) => new PiecePlacement(type,type==PieceType.Normal?PieceColor.Red:PieceColor.None,d,new GridPosition(x,y));
                level.Configure(6,6,3,PieceColor.Red,new[]{P(PieceType.Normal,0,0,Direction.Left)});
                var b=new BoardManager(); b.Load(level); b.RequestMove(new GridPosition(0,0)); b.CompleteResolution();
                Assert.That(b.CanUndo,Is.False);
                level.Configure(6,6,3,PieceColor.Red,new[]{P(PieceType.Normal,0,0,Direction.Right),P(PieceType.Direction,1,0,Direction.Up),P(PieceType.Direction,1,1,Direction.Right),P(PieceType.Direction,2,1,Direction.Down),P(PieceType.Direction,2,0,Direction.Left)});
                b.Load(level); b.RequestMove(new GridPosition(0,0)); b.CompleteResolution();
                Assert.That(b.LastReactionWasCancelled,Is.True); Assert.That(b.CanUndo,Is.False);
                level.Configure(6,6,3,PieceColor.Red,level.Placements.Concat(new[]{P(PieceType.Normal,4,4,Direction.Right)}).ToArray());
                b.Load(level);string before=State(b);b.RequestMove(new GridPosition(4,4));b.CompleteResolution();
                b.RequestMove(new GridPosition(0,0));b.CompleteResolution();Assert.That(b.LastReactionWasCancelled,Is.True);
                Assert.That(b.Undo(),Is.True);Assert.That(State(b),Is.EqualTo(before));
            }
            finally { Object.DestroyImmediate(level); }
        }
        [Test] public void BlockedTapPreservesPreviousCommittedSnapshotAndLossCanBeRecovered()
        {
            var l=ScriptableObject.CreateInstance<LevelData>();
            try
            {
                l.Configure(4,4,2,PieceColor.Red,new[]{new PiecePlacement(PieceType.Normal,PieceColor.Red,Direction.Right,new GridPosition(2,0))});
                var b=new BoardManager();b.Load(l);string before=State(b);
                b.RequestMove(new GridPosition(2,0));b.CompleteResolution();b.RequestMove(new GridPosition(3,0));b.CompleteResolution();
                Assert.That(b.Undo(),Is.True);Assert.That(State(b),Is.EqualTo(before));
                l.Configure(4,4,1,PieceColor.Red,l.Placements);b.Load(l);before=State(b);
                b.RequestMove(new GridPosition(2,0));b.CompleteResolution();Assert.That(b.State,Is.EqualTo(GameState.Lost));
                Assert.That(b.Undo(),Is.True);Assert.That(State(b),Is.EqualTo(before));
                b.RequestMove(new GridPosition(2,0));b.CompleteResolution();b.Load(l);Assert.That(b.CanUndo,Is.False);
                b.Load(LevelValidation.AllLevels()[9]);Assert.That(b.CanUndo,Is.False);
            }
            finally { Object.DestroyImmediate(l); }
        }
        [TestCase(1)] [TestCase(10)] [TestCase(21)] [TestCase(25)] [TestCase(30)]
        [TestCase(35)] [TestCase(36)] [TestCase(38)] [TestCase(40)]
        public void RepresentativeHintsAreAuthoredDistinctAndStaged(int number)
        {
            var l=LevelValidation.AllLevels()[number-1];Assert.That(HintCatalog.AuthoredCount(l),Is.EqualTo(3),l.name);
            var s=new HintSession();Assert.That(s.Remaining,Is.EqualTo(3));
            var texts=new string[3];
            for(int i=0;i<3;i++){Assert.That(s.TryUse(l,out texts[i]),Is.True);Assert.That(s.Stage(l),Is.EqualTo(i+1));Assert.That(s.Remaining,Is.EqualTo(2-i));}
            Assert.That(texts.Distinct().Count(),Is.EqualTo(3));Assert.That(s.TryUse(l,out _),Is.False);
            s.GrantReward();Assert.That(s.Remaining,Is.EqualTo(1));Assert.That(s.TryUse(l,out string repeated),Is.False);
            Assert.That(repeated,Is.Null);Assert.That(s.Remaining,Is.EqualTo(1));Assert.That(s.Stage(l),Is.EqualTo(3));
        }
        [Test] public void UnauthoredHintsFallBackWithoutSolverAndNullAdsNeverReward()
        {
            // Campaign coverage is complete; exercise fallback with genuinely missing metadata.
            var l=Object.Instantiate(LevelValidation.AllLevels()[1]);l.name="MissingHintMetadataFixture";
            try
            {
                Assert.That(HintCatalog.AuthoredCount(l),Is.Zero);
                Assert.That(HintCatalog.Get(l,2),Is.EqualTo(l.Hint));Assert.That(HintCatalog.Get(null,1),Is.Not.Empty);
            }
            finally { Object.DestroyImmediate(l); }
            var ads=new NullRewardedAdService();Assert.That(ads.IsRewardedAdAvailable,Is.False);
            bool? result=null;ads.ShowRewardedAd(RewardReason.Hint,b=>result=b);Assert.That(result,Is.False);
        }
        [Test] public void UtilityAnalyticsPreserveActualEffortAndCanResumeFailedAttempt()
        {
            var provider=new LocalAnalyticsService();var t=new TelemetryTracker(provider,()=>0);
            t.StartSession();var l=LevelValidation.AllLevels()[9];t.StartLevel(l,9,l.VerifiedOptimalMoveCount);
            t.AcceptedTap(2,0,false);t.Finish(false);t.UndoUsed(l.MoveLimit);
            foreach(string name in new[]{"hint_opened","hint_used","hint_exhausted","rewarded_hint_requested","rewarded_hint_completed","rewarded_hint_failed"})t.Utility(name,2,1,l.MoveLimit);
            Assert.That(t.Result,Is.Null);Assert.That(t.Snapshot().successfulMoves,Is.EqualTo(1));
            Assert.That(provider.Events.Any(e=>e.name=="undo_used"),Is.True);
            var last=provider.Events.Last();Assert.That(last.hintStage,Is.EqualTo(2));Assert.That(last.freeHintsRemaining,Is.EqualTo(1));Assert.That(last.attempt.levelNumber,Is.EqualTo(10));
            t.AcceptedTap(2,1,false);t.Finish(true);Assert.That(t.Result.completed,Is.True);
        }
        private sealed class FakeAds : IRewardedAdService
        {
            public bool IsRewardedAdAvailable => true;
            public Action<bool> Complete;
            public int Calls;
            public void ShowRewardedAd(RewardReason reason,Action<bool> done){Assert.That(reason,Is.EqualTo(RewardReason.Hint));Calls++;Complete=done;}
        }
        [UnityTest] public IEnumerator ControlsHintsRewardsUndoAndCompletionWithActualCaptures()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return new WaitForSecondsRealtime(.3f);
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.UnlockAllLevels();game.SelectLevel(9);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(GameObject.Find("Goal").GetComponent<Text>().text,Is.EqualTo("Clear Red"));
            var undo=GameObject.Find("Undo").GetComponent<Button>();Assert.That(undo.interactable,Is.False);Assert.That(undo.targetGraphic.raycastTarget,Is.False);
            Capture("01-undo-disabled");Capture("03-bottom-controls");
            Tap(game.CurrentLevel.KnownSolution[0]);Assert.That(game.Undo(),Is.False);yield return Settled(game);
            Assert.That(undo.interactable,Is.True);Capture("02-undo-enabled");
            int moves=game.Board.MovesRemaining;Assert.That(game.Undo(),Is.True);Assert.That(game.Board.MovesRemaining,Is.EqualTo(moves+1));
            game.OpenHint();Assert.That(GameObject.Find("Hint Count").GetComponent<Text>().text,Is.EqualTo("2"));
            Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.StartWith("Hint 1/3"));
            yield return new WaitForSecondsRealtime(.3f);Capture("04-hint-active");
            game.OpenHint();game.OpenHint();game.SelectLevel(34);yield return new WaitForSecondsRealtime(.35f);game.OpenHint();
            Assert.That(game.Hints.Remaining,Is.Zero);Assert.That(GameObject.Find("Rewarded Hint Panel"),Is.Not.Null);
            Assert.That(GameObject.Find("Watch Ad").GetComponent<Button>().interactable,Is.False);Capture("05-rewarded-hint-modal");
            Assert.That(game.Undo(),Is.False);game.CloseHint();
            var ads=new FakeAds();game.RewardedAds=ads;game.OpenHint();game.RequestRewardedHint();game.RequestRewardedHint();Assert.That(ads.Calls,Is.EqualTo(1));
            ads.Complete(false);Assert.That(game.Hints.Remaining,Is.Zero);game.CloseHint();
            game.SelectLevel(35);yield return new WaitForSecondsRealtime(.2f);
            game.OpenHint();game.RequestRewardedHint();ads.Complete(true);ads.Complete(true);
            Assert.That(game.Hints.Stage(game.CurrentLevel),Is.EqualTo(1));Assert.That(game.Hints.Remaining,Is.Zero);
            Assert.That(GameObject.Find("Rewarded Hint Panel"),Is.Null);
            game.OpenHint();game.RequestRewardedHint();var late=ads.Complete;game.Restart();late(true);yield return null;
            Assert.That(game.Hints.Remaining,Is.Zero);Assert.That(game.Board.CanUndo,Is.False);
            game.SelectLevel(39);yield return new WaitForSecondsRealtime(.3f);
            Assert.That(GameObject.Find("Goal").GetComponent<Text>().text,Is.EqualTo("Clear Both"));Capture("06-level40-controls");
            game.OpenSettings();GameObject.Find("Reduced Motion Setting").GetComponent<Button>().onClick.Invoke();game.CloseSettings();
            yield return new WaitForSecondsRealtime(.1f);
            Tap(game.CurrentLevel.KnownSolution[0]);yield return Settled(game);Assert.That(game.Undo(),Is.True);
            Assert.That(game.Board.State,Is.EqualTo(GameState.Playing));Assert.That(GameObject.Find("Board").transform.localScale,Is.EqualTo(Vector3.one));
            Tap(game.CurrentLevel.KnownSolution[0]);yield return Settled(game);game.Restart();yield return null;Assert.That(game.Board.CanUndo,Is.False);
            Tap(game.CurrentLevel.KnownSolution[0]);yield return Settled(game);game.SelectLevel(9);yield return null;Assert.That(game.Board.CanUndo,Is.False);
            foreach(var tap in game.CurrentLevel.KnownSolution){Tap(tap);yield return Settled(game);}
            yield return new WaitForSecondsRealtime(1.8f);
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won));Assert.That(game.Undo(),Is.False);
            Assert.That(GameObject.Find("Undo"),Is.Null);Assert.That(GameObject.Find("Hint"),Is.Null);
            Assert.That(GameObject.Find("Next Level"),Is.Not.Null);Assert.That(GameObject.Find("Open Chapter"),Is.Not.Null);
            Capture("07-completion");Assert.That(game.NextLevel(),Is.True);yield return null;Assert.That(game.CurrentLevel.name,Does.StartWith("Level11"));
            yield return new ExitPlayMode();
        }
        private static IEnumerator Settled(PrototypeGame game)
        { float until=Time.realtimeSinceStartup+12;while(game.Board.State==GameState.Resolving&&Time.realtimeSinceStartup<until)yield return null;Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving)); }
        private static void Tap(GridPosition position)
        { var p=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(x=>x.Data.Active&&x.Data.Type==PieceType.Normal&&x.Data.Position==position);p.GetComponentInChildren<Button>().onClick.Invoke(); }
        private static void Capture(string name)
        { Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/ux-foundation"));SprintPresentationTests.Capture("ux-foundation/"+name+".png"); }
    }
}
