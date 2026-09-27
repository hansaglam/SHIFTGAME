using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed class Batch36Tests
    {
        private const string Prefix="SHIFT.Tests.Batch36.";
        [TearDown] public void Cleanup(){new SaveService(Prefix).Reset();new SettingsService(Prefix+"Settings.").Reset();}
        [TestCase(35,6,6,3,1d/18,1d/18,30,3,4,4,6,1)]
        [TestCase(36,6,6,12,7d/243,7d/243,98,5,4,4,5,1)]
        [TestCase(37,7,7,7,1d/48,1d/48,230,5,6,8,12,1)]
        [TestCase(38,5,8,19,1d/9,1d/27,142,2,6,6,8,3)]
        [TestCase(39,7,8,71,37d/3456,0.006751543209876542,659,6,14,14,18,3)]
        public void ExactGraphsMinimumProofsAndPayoffs(int index,int minimum,int budget,int paths,double probability,double optimumProbability,int states,int mixed,int final,int routeMax,int globalMax,int winningStarts)
        {
            var l=LevelValidation.AllLevels()[index];var r=new PuzzleBatchAnalysis(l).Analyze(l.KnownSolution);
            Assert.That(l.ValidatePlayable(out string error),Is.True,error);
            Assert.That(LevelValidation.VerifySolution(l,out error,out int depth),Is.True,error);
            Assert.That(l.VerifiedOptimalMoveCount,Is.EqualTo(minimum));Assert.That(r.minimum,Is.EqualTo(minimum));
            Assert.That(l.MoveLimit,Is.EqualTo(budget));Assert.That(l.KnownSolutionLength,Is.EqualTo(minimum));
            Assert.That(r.solutionPaths,Is.EqualTo(paths));Assert.That(r.randomSolveProbability,Is.EqualTo(probability).Within(1e-12));
            Assert.That(r.optimalDiscoveryProbability,Is.EqualTo(optimumProbability).Within(1e-12));
            Assert.That(r.states,Is.EqualTo(states));Assert.That(r.meaningfulDecisions,Is.EqualTo(mixed));
            Assert.That(r.finalDepth,Is.EqualTo(final));Assert.That(r.routeMaxDepth,Is.EqualTo(routeMax));
            Assert.That(depth,Is.EqualTo(routeMax));Assert.That(r.maxDepth,Is.EqualTo(globalMax));
            Assert.That(r.openings.Count,Is.EqualTo(index==39||index==37?4:3));Assert.That(r.openings.Count(o=>o.winningPaths>0),Is.EqualTo(winningStarts));
            Assert.That(l.ResolvedTargetColors,Is.EqualTo(index==37?new[]{PieceColor.Red,PieceColor.Yellow}:new[]{PieceColor.Red,PieceColor.Blue}));
            Assert.That(Batch36Review.DecisionThemes[index-35].Distinct().Count(),Is.GreaterThanOrEqualTo(index==39?5:index==36||index==37?4:3));
            int nodes=0;Assert.That(DesignValidation.CanWin(l,new List<GridPosition>(),minimum-1,ref nodes),Is.False);
            nodes=0;Assert.That(DesignValidation.CanWin(l,new List<GridPosition>(),2,ref nodes),Is.False);
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Validation/batch36"));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,l.name+"-analysis.json"),JsonUtility.ToJson(r,true));
        }
        [Test] public void SharedTurnRequiresVacatingShelfAndBothColoredDestinations()
        {
            var l=LevelValidation.AllLevels()[35];
            foreach(var tap in new[]{new GridPosition(2,1),new GridPosition(1,2)})Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(new[]{tap}),Is.Zero);
            var b=Run(l,l.KnownSolution.Take(4).ToArray());
            Assert.That(b.Pieces.Single(p=>p.Type==PieceType.Normal&&p.Color==PieceColor.Red).Active,Is.False);
            Assert.That(b.Pieces.Single(p=>p.Type==PieceType.Normal&&p.Color==PieceColor.Blue).Active,Is.True);
            Assert.That(b.State,Is.EqualTo(GameState.Playing));
            b=Run(l,new GridPosition(1,2));
            Assert.That(b.GetOccupant(new GridPosition(2,0)).Color,Is.EqualTo(PieceColor.Red));
            Assert.That(b.GetTerrain(new GridPosition(1,0)).Color,Is.EqualTo(PieceColor.Blue));
        }
        [Test] public void SharedBayRequiresBlueToYieldAfterRedirectingRed()
        {
            var l=LevelValidation.AllLevels()[36];var b=Run(l,new GridPosition(0,2),new GridPosition(1,2));
            Assert.That(b.GetOccupant(new GridPosition(2,2)).Color,Is.EqualTo(PieceColor.Blue));
            Assert.That(b.GetOccupant(new GridPosition(3,2)).Color,Is.EqualTo(PieceColor.Red));
            Assert.That(b.GetOccupant(new GridPosition(3,2)).Direction,Is.EqualTo(Direction.Right));
            b=Run(l,new GridPosition(0,2),new GridPosition(1,2),new GridPosition(2,1));
            Assert.That(b.GetOccupant(new GridPosition(2,3)).Color,Is.EqualTo(PieceColor.Blue));
            var wrong=new[]{new GridPosition(0,2),new GridPosition(1,2),new GridPosition(2,2)};
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
            b=Run(l,wrong);Assert.That(b.GetOccupant(new GridPosition(3,2)).Color,Is.EqualTo(PieceColor.Blue));
        }
        [Test] public void YellowLaunchesRedAndRedReopensAccessForTheOtherYellow()
        {
            var l=LevelValidation.AllLevels()[37];var b=Run(l,l.KnownSolution.Take(1).ToArray());
            Assert.That(b.GetOccupant(new GridPosition(3,3)).Color,Is.EqualTo(PieceColor.Red));
            Assert.That(b.GetTerrain(new GridPosition(1,2)).GateOpen,Is.False);
            b=Run(l,l.KnownSolution.Take(2).ToArray());
            Assert.That(b.Actions.Count(a=>a.Type==BoardActionType.Deliver),Is.EqualTo(1));
            Assert.That(b.GetTerrain(new GridPosition(1,2)).GateOpen,Is.True);
            Assert.That(b.State,Is.EqualTo(GameState.Playing));
            b=Run(l,l.KnownSolution.Take(5).ToArray());
            Assert.That(b.Pieces.Where(p=>p.Type==PieceType.Normal&&p.Color==PieceColor.Red).All(p=>!p.Active),Is.True);
            Assert.That(b.State,Is.EqualTo(GameState.Playing));
            foreach(var tap in new[]{new GridPosition(1,4),new GridPosition(4,3),new GridPosition(2,2)})
                Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(new[]{tap}),Is.Zero);
        }
        [Test] public void OptimizationHasFiveAndSevenMoveCompletionsAndOnlyFiveIsPerfect()
        {
            var l=LevelValidation.AllLevels()[38];
            foreach(var route in new[]{l.KnownSolution.ToArray(),Batch36Review.Normal39})
            {
                var b=new BoardManager();b.Load(l);var telemetry=new TelemetryTracker(null,()=>0);telemetry.StartLevel(l,38,l.VerifiedOptimalMoveCount);
                foreach(var tap in route){Assert.That(b.RequestMove(tap),Is.True);Assert.That(b.ReactionDepth,Is.GreaterThan(0));telemetry.AcceptedTap(b.ReactionDepth,b.MovesRemaining,false);b.CompleteResolution();}
                Assert.That(b.State,Is.EqualTo(GameState.Won));telemetry.Finish(true);Assert.That(telemetry.Result.perfectShift,Is.EqualTo(route.Length==5));
            }
            var shared=Run(l,l.KnownSolution.Take(4).ToArray());
            foreach(var c in l.ResolvedTargetColors)Assert.That(shared.Actions.Any(a=>a.Type==BoardActionType.Move&&shared.Pieces[a.PieceId].Color==c),Is.True);
        }
        [Test] public void FinaleDeliversBlueThenDivertsRedAndAllowsAnEightMoveRecovery()
        {
            var l=LevelValidation.AllLevels()[39];var b=Run(l,l.KnownSolution.Take(6).ToArray());
            Assert.That(b.ReactionDepth,Is.EqualTo(13));Assert.That(b.Pieces.Single(p=>p.Type==PieceType.Normal&&p.Color==PieceColor.Blue).Active,Is.False);
            Assert.That(b.GetOccupant(new GridPosition(4,1)).Color,Is.EqualTo(PieceColor.Red));Assert.That(b.State,Is.EqualTo(GameState.Playing));
            b=Run(l,l.KnownSolution.ToArray());Assert.That(b.ReactionDepth,Is.EqualTo(14));Assert.That(b.State,Is.EqualTo(GameState.Won));
            var slower=l.KnownSolution.Take(5).Concat(new[]{new GridPosition(2,1),new GridPosition(3,1),new GridPosition(3,0)}).ToArray();
            b=Run(l,slower);Assert.That(b.State,Is.EqualTo(GameState.Won));Assert.That(b.MovesRemaining,Is.Zero);
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(new[]{new GridPosition(0,2)}),Is.Zero);
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(new[]{new GridPosition(2,0),new GridPosition(3,0)}),Is.Zero);
        }
        [Test] public void ProtectedContentAndOnlyThreeBoardManagerSubstitutions()
        {
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Batch36FrozenFiles.txt")))
            {var p=line.Split('|');Assert.That(Hash(File.ReadAllBytes(Path.Combine(Application.dataPath,p[0]))),Is.EqualTo(p[1]),p[0]);}
            var catalog=File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Resources/LevelDesignCatalog.asset")).Split(new[]{"  - level: "},StringSplitOptions.None);
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Batch36CatalogFrozen.txt")))
            {var p=line.Split('|');Assert.That(Hash(System.Text.Encoding.UTF8.GetBytes(catalog[int.Parse(p[0])])),Is.EqualTo(p[1]),p[0]);}
            var old=File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Fixtures/LegacyBoardManager.txt")).Replace("\r\n","\n");
            var expected=old.Replace("private PieceColor targetColor;","private HashSet<PieceColor> targetColors;").Replace("targetColor = level.TargetColor;","targetColors = new HashSet<PieceColor>(level.ResolvedTargetColors);").Replace("p.Color == targetColor","targetColors.Contains(p.Color)");
            var current=File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Scripts/Board/BoardManager.cs")).Replace("\r\n","\n");
            // Explicit Undo storage/API is additive. Retain the exact legacy simulation comparison.
            int begin=current.IndexOf("        private Snapshot[] undoSnapshot;",StringComparison.Ordinal);
            int end=current.IndexOf("        private bool[] moving;",StringComparison.Ordinal);
            Assert.That(begin,Is.GreaterThan(0));Assert.That(end,Is.GreaterThan(begin));
            current=current.Remove(begin,end-begin).Replace("            ClearUndo();\n", "");
            current=current.Replace("            if (actions.Count > 0)\n            {\n                undoSnapshot = (Snapshot[])snapshots.Clone(); undoMoves = MovesRemaining;\n                MovesRemaining--;\n            }", "            if (actions.Count > 0) MovesRemaining--;");
            Assert.That(current,Is.EqualTo(expected));
        }
        [UnityTest] public IEnumerator Actual35BoundaryCampaignCompletionAndPortraitCaptures()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            new SaveService(Prefix).Save(new ProgressData(34,34,33));
            yield return new EnterPlayMode();yield return new WaitForSecondsRealtime(.25f);
            var game=Object.FindFirstObjectByType<PrototypeGame>();yield return Solve(game,game.CurrentLevel.KnownSolution);
            GameObject.Find("Next Level").GetComponent<Button>().onClick.Invoke();yield return null;
            for(int n=36;n<=40;n++)
            {
                yield return new WaitForSecondsRealtime(.25f);Assert.That(game.CurrentLevel.name,Does.StartWith("Level"+n));
                Assert.That(GameObject.Find("Goal").GetComponent<Text>().text,Is.EqualTo("Clear Both"));
                Assert.That(GameObject.Find("Target Color 1"),Is.Not.Null);Assert.That(GameObject.Find("Alpine Lake"),Is.Not.Null);
                SprintPresentationTests.Capture($"batch36-level{n}-start.png");
                if(n==36)SprintPresentationTests.Capture("batch36-multicolor-hud.png");
                if(n==39)
                {
                    yield return Solve(game,Batch36Review.Normal39);Assert.That(game.Telemetry.Result.perfectShift,Is.False);
                    SprintPresentationTests.Capture("batch36-level39-normal.png");GameObject.Find("Restart").GetComponent<Button>().onClick.Invoke();yield return null;
                }
                yield return Solve(game,game.CurrentLevel.KnownSolution);Assert.That(game.Telemetry.Result.perfectShift,Is.True);
                if(n==39)SprintPresentationTests.Capture("batch36-level39-perfect.png");
                if(n==40)
                {
                    Assert.That(GameObject.Find("Chain").GetComponent<Text>().text,Is.EqualTo("MEGA SHIFT x14"));
                    Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Does.Contain("CHAPTER 2 COMPLETE"));
                    Assert.That(game.Progress.IsChapterComplete(1),Is.True);Assert.That(game.NextLevel(),Is.False);
                    Assert.That(game.Progress.HighestCompleted,Is.EqualTo(39));SprintPresentationTests.Capture("batch36-level40-payoff.png");
                }
                else {GameObject.Find("Next Level").GetComponent<Button>().onClick.Invoke();yield return null;}
            }
            yield return new ExitPlayMode();
        }
        private static string Hash(byte[] bytes){using var sha=SHA256.Create();return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","");}
        private static BoardManager Run(LevelData l,params GridPosition[] taps)
        {var b=new BoardManager();b.Load(l);foreach(var tap in taps){Assert.That(b.RequestMove(tap),Is.True);Assert.That(b.ReactionDepth,Is.GreaterThan(0));b.CompleteResolution();}return b;}
        private static IEnumerator Solve(PrototypeGame game,IReadOnlyList<GridPosition> route)
        {
            foreach(var tap in route)
            {
                var p=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(p=>p.Data.Type==PieceType.Normal&&p.Data.Position==tap);
                p.GetComponentInChildren<Button>().onClick.Invoke();double deadline=Time.realtimeSinceStartupAsDouble+5;
                while(game.Board.State==GameState.Resolving&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
                Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
            }
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won));
        }
    }
}
