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
    public sealed class Batch26Tests
    {
        private const string Prefix="SHIFT.Tests.Batch26.";
        [TearDown] public void Cleanup() { new SaveService(Prefix).Reset();new SettingsService(Prefix+"Settings.").Reset(); }
        [TestCase(25,4,4,1,1d/24,6,8,11)]
        [TestCase(26,5,5,2,1d/36,4,7,63)]
        [TestCase(27,5,5,3,1d/81,8,9,24)]
        [TestCase(28,5,8,116,0.37019890260631,4,18,146)]
        [TestCase(29,7,7,1,1d/1536,8,11,646)]
        public void ExactGraphKnownSolutionIndependentMinimumAndPayoff(int index,int minimum,int budget,int paths,double probability,int final,int maximum,int states)
        {
            var l=LevelValidation.AllLevels()[index];var r=new PuzzleBatchAnalysis(l).Analyze(l.KnownSolution);
            Assert.That(LevelValidation.VerifySolution(l,out string error,out int routeMax),Is.True,error);
            Assert.That(l.VerifiedOptimalMoveCount,Is.EqualTo(minimum));Assert.That(r.minimum,Is.EqualTo(minimum));
            Assert.That(l.KnownSolutionLength,Is.EqualTo(minimum));Assert.That(l.MoveLimit,Is.EqualTo(budget));
            Assert.That(r.solutionPaths,Is.EqualTo(paths));Assert.That(r.randomSolveProbability,Is.EqualTo(probability).Within(1e-12));
            Assert.That(r.states,Is.EqualTo(states));Assert.That(r.finalDepth,Is.EqualTo(final));Assert.That(r.maxDepth,Is.EqualTo(maximum));
            Assert.That(r.routeMaxDepth,Is.EqualTo(routeMax));Assert.That(r.openings.Count,Is.EqualTo(index==29?4:3));
            Assert.That(r.openings.Count(o=>o.winningPaths>0),Is.EqualTo(index==28?3:1));
            Assert.That(r.openings.All(o=>o.depth>0),Is.True);
            int target=index==25?2:index==29?4:3;
            Assert.That(Batch26Review.DecisionThemes[index-25].Distinct().Count(),Is.GreaterThanOrEqualTo(target));
            Assert.That(index==28?r.efficiencyDecisions:r.meaningfulDecisions,Is.GreaterThanOrEqualTo(target));
            Assert.That(r.optimalDiscoveryProbability,Is.LessThan(index==25?.1:index==29?.02:.05));
            int nodes=0;Assert.That(DesignValidation.CanWin(l,new List<GridPosition>(),minimum-1,ref nodes),Is.False);
            nodes=0;Assert.That(DesignValidation.CanWin(l,new List<GridPosition>(),2,ref nodes),Is.False);
            var shorter=Object.Instantiate(l);
            try
            {
                shorter.Configure(l.Width,l.Height,minimum,l.TargetColor,l.Placements);
                Assert.That(new PuzzleBatchAnalysis(shorter).Analyze().randomSolveProbability,Is.EqualTo(r.optimalDiscoveryProbability).Within(1e-12),"Independent budget-limited optimal discovery check.");
            }
            finally { Object.DestroyImmediate(shorter); }
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Validation/batch26"));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,l.name+"-analysis.json"),JsonUtility.ToJson(r,true));
        }
        [Test] public void EarlyTargetRedirectsWaitingOccupantIntoTheOnlyLane()
        {
            var l=LevelValidation.AllLevels()[25];var wrong=new[]{new GridPosition(0,1)};var b=Run(l,wrong);
            Assert.That(b.GetOccupant(new GridPosition(2,2)).Color,Is.EqualTo(PieceColor.Blue));
            Assert.That(b.GetOccupant(new GridPosition(2,2)).Direction,Is.EqualTo(Direction.Right));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
        }
        [Test] public void SharedCrossingNeedsBothBoxParkingAndOperatorDeparture()
        {
            var l=LevelValidation.AllLevels()[26];var wrong=new[]{new GridPosition(0,2)};var b=Run(l,wrong);
            Assert.That(b.GetOccupant(new GridPosition(2,1)).Type,Is.EqualTo(PieceType.PushBlock));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
            b=Run(l,new GridPosition(1,1));Assert.That(b.GetOccupant(new GridPosition(1,2)).Color,Is.EqualTo(PieceColor.Blue));
            b=Run(l,new GridPosition(1,1),new GridPosition(1,2));Assert.That(b.GetOccupant(new GridPosition(1,2)),Is.Null);
            Assert.That(b.GetOccupant(new GridPosition(1,4)).Type,Is.EqualTo(PieceType.PushBlock));
        }
        [Test] public void WrongColorFollowingTheLowerTurnsBlocksDelivery()
        {
            var l=LevelValidation.AllLevels()[27];var wrong=new[]{new GridPosition(2,0)};var b=Run(l,wrong);
            Assert.That(b.GetOccupant(new GridPosition(3,0)).Color,Is.EqualTo(PieceColor.Yellow));
            Assert.That(b.GetOccupant(new GridPosition(3,0)).Direction,Is.EqualTo(Direction.Right));
            Assert.That(b.Actions.Count(a=>a.Type==BoardActionType.Turn),Is.GreaterThanOrEqualTo(2));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
        }
        [Test] public void OptimizationHasFiveSevenAndEightMoveWinsOnlyFiveIsPerfect()
        {
            var l=LevelValidation.AllLevels()[28];
            foreach(var route in new IReadOnlyList<GridPosition>[] {l.KnownSolution,Batch26Review.Merged29,Batch26Review.Longer29})
            {
                var b=new BoardManager();b.Load(l);var telemetry=new TelemetryTracker(null,()=>0);telemetry.StartLevel(l,28,l.VerifiedOptimalMoveCount);
                foreach(var tap in route)
                {
                    Assert.That(b.RequestMove(tap),Is.True);Assert.That(b.ReactionDepth,Is.GreaterThan(0));
                    telemetry.AcceptedTap(b.ReactionDepth,b.MovesRemaining,false);b.CompleteResolution();
                }
                Assert.That(b.State,Is.EqualTo(GameState.Won));telemetry.Finish(true);
                Assert.That(telemetry.Result.perfectShift,Is.EqualTo(route.Count==5));
            }
            var analysis=new PuzzleBatchAnalysis(l).Analyze();
            Assert.That(analysis.optimalDiscoveryProbability,Is.EqualTo(1d/54).Within(1e-12));
            Assert.That(analysis.efficiencyDecisions,Is.EqualTo(4));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(new[]{new GridPosition(2,1)}),Is.GreaterThan(0),"The tempting loop is a real alternative, not a trap.");
        }
        [Test] public void FinaleNeedsParkingThenVacatingThenRestoringGateState()
        {
            var l=LevelValidation.AllLevels()[29];var first=new GridPosition(1,1);var second=new GridPosition(2,1);var third=new GridPosition(2,2);
            var b=Run(l,first);Assert.That(b.GetTerrain(new GridPosition(3,2)).GateOpen,Is.False);
            Assert.That(b.GetOccupant(new GridPosition(3,1)).Type,Is.EqualTo(PieceType.PushBlock));
            b=Run(l,first,second);Assert.That(b.GetOccupant(new GridPosition(2,1)),Is.Null);
            b=Run(l,first,second,third);Assert.That(b.GetTerrain(new GridPosition(3,2)).GateOpen,Is.True);
            Assert.That(b.GetOccupant(new GridPosition(2,2)),Is.Null);
            var wrong=new[]{new GridPosition(0,2)};b=Run(l,wrong);
            Assert.That(b.GetOccupant(new GridPosition(3,2)).Color,Is.EqualTo(PieceColor.Blue));
            Assert.That(b.GetOccupant(new GridPosition(3,2)).Direction,Is.EqualTo(Direction.Right));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
        }
        [Test] public void AllOtherLevelsRulesVisualIdentityAndCatalogEntriesAreFrozen()
        {
            var lines=File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Batch26FrozenFiles.txt"));
            Assert.That(lines.Count(s=>s.Contains("/Data/Levels/Level")),Is.EqualTo(35));
            foreach(var line in lines)
            {
                var pair=line.Split('|');Assert.That(Hash(File.ReadAllBytes(Path.Combine(Application.dataPath,pair[0]))),Is.EqualTo(pair[1]),pair[0]);
            }
            var entries=File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Resources/LevelDesignCatalog.asset")).Split(new[]{"  - level: "},StringSplitOptions.None);
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Batch26CatalogFrozen.txt")))
            {
                var pair=line.Split('|');Assert.That(Hash(System.Text.Encoding.UTF8.GetBytes(entries[int.Parse(pair[0])])),Is.EqualTo(pair[1]),"Catalog "+pair[0]);
            }
        }
        private static string Hash(byte[] bytes) { using var sha=SHA256.Create();return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-",""); }
        [UnityTest] public IEnumerator BatchScreensRealProgressionNormalPerfectAndReducedMotion()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            new SaveService(Prefix).Save(new ProgressData(24,24,23));
            yield return new EnterPlayMode();yield return new WaitForSecondsRealtime(.25f);
            var game=Object.FindFirstObjectByType<PrototypeGame>();yield return Solve(game,game.CurrentLevel.KnownSolution);
            GameObject.Find("Next Level").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level26"));
            for(int n=26;n<=30;n++)
            {
                yield return new WaitForSecondsRealtime(.25f);Assert.That(game.CurrentLevel.name,Does.StartWith("Level"+n));
                Assert.That(GameObject.Find("Alpine Lake"),Is.Not.Null);
                foreach(var icon in Object.FindObjectsByType<IdentityIcon>(FindObjectsSortMode.None)) Assert.That(icon.raycastTarget,Is.False);
                SprintPresentationTests.Capture($"batch26-level{n}.png");
                if(n==29)
                {
                    yield return Solve(game,Batch26Review.Longer29);Assert.That(game.Telemetry.Result.perfectShift,Is.False);
                    Assert.That(Object.FindFirstObjectByType<GameHud>().MasteryVisible,Is.False);
                    SprintPresentationTests.Capture("batch26-level29-normal.png");
                    GameObject.Find("Restart").GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.2f);
                }
                if(n==30)
                {
                    game.OpenSettings();GameObject.Find("Reduced Motion Setting").GetComponent<Button>().onClick.Invoke();game.CloseSettings();
                    yield return new WaitForSecondsRealtime(.05f);Assert.That(game.Settings.ReducedMotion,Is.True);
                }
                yield return Solve(game,game.CurrentLevel.KnownSolution);Assert.That(game.Telemetry.Result.perfectShift,Is.True);
                if(n==29) SprintPresentationTests.Capture("batch26-level29-perfect.png");
                if(n==30)
                {
                    Assert.That(GameObject.Find("Chain").GetComponent<Text>().text,Does.StartWith("MEGA SHIFT"));
                    Assert.That(GameObject.Find("Board").transform.localScale,Is.EqualTo(Vector3.one));
                    SprintPresentationTests.Capture("batch26-level30-payoff.png");
                }
                GameObject.Find("Next Level").GetComponent<Button>().onClick.Invoke();yield return null;
            }
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level31"));Assert.That(game.Progress.HighestUnlocked,Is.EqualTo(30));
            yield return new ExitPlayMode();
        }
        private static BoardManager Run(LevelData l,params GridPosition[] taps)
        {
            var b=new BoardManager();b.Load(l);
            foreach(var tap in taps) { Assert.That(b.RequestMove(tap),Is.True);Assert.That(b.ReactionDepth,Is.GreaterThan(0));b.CompleteResolution(); }
            return b;
        }
        private static IEnumerator Solve(PrototypeGame game,IReadOnlyList<GridPosition> route)
        {
            foreach(var tap in route)
            {
                var piece=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(p=>p.Data.Type==PieceType.Normal && p.Data.Position==tap);
                piece.GetComponentInChildren<Button>().onClick.Invoke();double deadline=Time.realtimeSinceStartupAsDouble+5;
                while(game.Board.State==GameState.Resolving && Time.realtimeSinceStartupAsDouble<deadline) yield return null;
                Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
            }
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won));
        }
    }
}
