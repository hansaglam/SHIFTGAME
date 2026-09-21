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
    public sealed class Batch31Tests
    {
        private const string Prefix="SHIFT.Tests.Batch31.";
        [TearDown] public void Cleanup(){new SaveService(Prefix).Reset();new SettingsService(Prefix+"Settings.").Reset();}
        [TestCase(30,6,1,1d/216,52,6,6,6,1)]
        [TestCase(31,6,2,0.011574074074074072,78,4,5,13,1)]
        [TestCase(32,6,2,1d/81,94,8,11,12,2)]
        [TestCase(33,5,6,2d/81,77,8,8,15,2)]
        [TestCase(34,8,102,0.00682789994855967,412,8,15,18,3)]
        public void ExactGraphsKnownSolutionsAndIndependentMinimum(int index,int minimum,int paths,double probability,int states,int final,int routeMax,int globalMax,int winningStarts)
        {
            var l=LevelValidation.AllLevels()[index];var r=new PuzzleBatchAnalysis(l).Analyze(l.KnownSolution);
            Assert.That(LevelValidation.VerifySolution(l,out string error,out int depth),Is.True,error);
            Assert.That(l.VerifiedOptimalMoveCount,Is.EqualTo(minimum));Assert.That(r.minimum,Is.EqualTo(minimum));
            Assert.That(l.MoveLimit,Is.EqualTo(minimum));Assert.That(l.KnownSolutionLength,Is.EqualTo(minimum));
            Assert.That(r.solutionPaths,Is.EqualTo(paths));Assert.That(r.randomSolveProbability,Is.EqualTo(probability).Within(1e-12));
            Assert.That(r.optimalDiscoveryProbability,Is.EqualTo(probability).Within(1e-12));
            Assert.That(r.states,Is.EqualTo(states));Assert.That(r.finalDepth,Is.EqualTo(final));
            Assert.That(r.routeMaxDepth,Is.EqualTo(routeMax));Assert.That(depth,Is.EqualTo(routeMax));Assert.That(r.maxDepth,Is.EqualTo(globalMax));
            Assert.That(r.openings.Count,Is.EqualTo(index==34?4:3));Assert.That(r.openings.Count(o=>o.winningPaths>0),Is.EqualTo(winningStarts));
            Assert.That(r.openings.All(o=>o.depth>0),Is.True);
            int themes=index>=33?4:3;Assert.That(Batch31Review.DecisionThemes[index-30].Distinct().Count(),Is.GreaterThanOrEqualTo(themes));
            Assert.That(r.meaningfulDecisions,Is.GreaterThanOrEqualTo(themes));
            int nodes=0;Assert.That(DesignValidation.CanWin(l,new List<GridPosition>(),minimum-1,ref nodes),Is.False);
            nodes=0;Assert.That(DesignValidation.CanWin(l,new List<GridPosition>(),2,ref nodes),Is.False);
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Validation/batch31"));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,l.name+"-analysis.json"),JsonUtility.ToJson(r,true));
        }
        [Test] public void WorkshopMustParkThenVacateBeforeDeliveryRoomCanProceed()
        {
            var l=LevelValidation.AllLevels()[30];var b=Run(l,new GridPosition(2,2));
            Assert.That(b.GetOccupant(new GridPosition(3,2)).Color,Is.EqualTo(PieceColor.Yellow));
            Assert.That(b.GetOccupant(new GridPosition(4,2)).Type,Is.EqualTo(PieceType.PushBlock));
            b=Run(l,new GridPosition(2,2),new GridPosition(3,2),new GridPosition(3,1));
            Assert.That(b.GetOccupant(new GridPosition(5,2)).Type,Is.EqualTo(PieceType.PushBlock));
            Assert.That(b.GetOccupant(new GridPosition(3,1)),Is.Null);
            var wrong=new[]{new GridPosition(4,1)};b=Run(l,wrong);
            Assert.That(b.GetOccupant(new GridPosition(2,1)).Color,Is.EqualTo(PieceColor.Blue));
            Assert.That(b.GetOccupant(new GridPosition(2,1)).Direction,Is.EqualTo(Direction.Left));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
            Assert.That(l.Placements.Any(p=>p.type==PieceType.Switch || p.type==PieceType.Gate),Is.False);
        }
        [Test] public void ReturnToTheStartingJunctionReversesDirectionAndNeedsVacatedParking()
        {
            var l=LevelValidation.AllLevels()[31];var b=Run(l,new GridPosition(1,2),new GridPosition(2,1));
            Assert.That(b.GetOccupant(new GridPosition(1,2)).Color,Is.EqualTo(PieceColor.Red));
            Assert.That(b.GetOccupant(new GridPosition(2,2)).Color,Is.EqualTo(PieceColor.Blue));
            var wrong=new[]{new GridPosition(1,2),new GridPosition(2,1),new GridPosition(1,2)};
            b=Run(l,wrong);Assert.That(b.GetOccupant(new GridPosition(3,1)).Color,Is.EqualTo(PieceColor.Blue));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
            b=Run(l,l.KnownSolution.Take(5).ToArray());
            Assert.That(b.Actions.Any(a=>a.Type==BoardActionType.Move && a.To==new GridPosition(2,1)),Is.True);
            Assert.That(b.GetOccupant(new GridPosition(3,1)).Direction,Is.EqualTo(Direction.Right));
            Assert.That(l.Placements.Single(p=>p.type==PieceType.Normal && p.color==PieceColor.Red).direction,Is.EqualTo(Direction.Left));
        }
        [Test] public void PerpendicularTargetsBecomeOneSharedReaction()
        {
            var l=LevelValidation.AllLevels()[32];var b=Run(l,l.KnownSolution.Take(4).ToArray());
            var targets=b.Pieces.Where(p=>p.Type==PieceType.Normal && p.Color==PieceColor.Red).Select(p=>p.Id).ToArray();
            Assert.That(targets.Length,Is.EqualTo(2));
            foreach(int id in targets)Assert.That(b.Actions.Any(a=>a.PieceId==id && a.Type==BoardActionType.Move),Is.True);
            Assert.That(b.GetOccupant(new GridPosition(4,2)).Direction,Is.EqualTo(Direction.Down));
            Assert.That(b.GetOccupant(new GridPosition(3,3)).Direction,Is.EqualTo(Direction.Right));
            Assert.That(b.ReactionDepth,Is.GreaterThanOrEqualTo(5));
        }
        [Test] public void CrossingMistakeCanRecoverInSevenButNotSixMoves()
        {
            var l=LevelValidation.AllLevels()[32];var prefix=new[]{new GridPosition(2,1)};
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(prefix),Is.Zero);
            var extended=Object.Instantiate(l);
            try
            {
                extended.Configure(l.Width,l.Height,7,l.TargetColor,l.Placements);
                var b=Run(extended,Batch31Review.Slower33);Assert.That(b.State,Is.EqualTo(GameState.Won));
                Assert.That(new PuzzleBatchAnalysis(extended).WinningContinuations(prefix),Is.GreaterThan(0));
            }
            finally{Object.DestroyImmediate(extended);}
        }
        [Test] public void SameRotatorHasDifferentOutcomeWhenEnteredFromAbove()
        {
            var l=LevelValidation.AllLevels()[33];var wrong=new[]{new GridPosition(2,2)};var b=Run(l,wrong);
            Assert.That(b.GetOccupant(new GridPosition(3,1)).Color,Is.EqualTo(PieceColor.Red));
            Assert.That(b.GetOccupant(new GridPosition(3,1)).Direction,Is.EqualTo(Direction.Down));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
            b=Run(l,l.KnownSolution.Take(4).ToArray());
            Assert.That(b.GetOccupant(new GridPosition(2,2)).Color,Is.EqualTo(PieceColor.Red));
            Assert.That(b.GetOccupant(new GridPosition(2,2)).Direction,Is.EqualTo(Direction.Left));
            Assert.That(b.GetOccupant(new GridPosition(2,3)).Color,Is.EqualTo(PieceColor.Blue));
        }
        [Test] public void FinalChainMustBeAssembledAndLauncherRepositioned()
        {
            var l=LevelValidation.AllLevels()[34];var initial=new BoardManager();initial.Load(l);
            Assert.That(initial.Pieces.Where(p=>p.Color==PieceColor.Red).All(p=>p.Position.y==3),Is.True);
            var b=Run(l,l.KnownSolution.Take(4).ToArray());
            Assert.That(b.GetOccupant(new GridPosition(1,1)).Color,Is.EqualTo(PieceColor.Red));
            Assert.That(b.GetOccupant(new GridPosition(3,1)).Color,Is.EqualTo(PieceColor.Red));
            b=Run(l,l.KnownSolution.Take(5).ToArray());
            Assert.That(b.GetOccupant(new GridPosition(1,1)).Color,Is.EqualTo(PieceColor.Blue));
            Assert.That(b.GetOccupant(new GridPosition(1,1)).Direction,Is.EqualTo(Direction.Right));
            Assert.That(b.GetTerrain(new GridPosition(4,1)).GateOpen,Is.False);
            b=Run(l,l.KnownSolution.Take(6).ToArray());Assert.That(b.GetTerrain(new GridPosition(4,1)).GateOpen,Is.True);
            b=Run(l,l.KnownSolution.Take(7).ToArray());
            Assert.That(b.ReactionDepth,Is.EqualTo(15));Assert.That(b.Actions.Count(a=>a.Type==BoardActionType.Deliver),Is.EqualTo(1));
            b=Run(l,l.KnownSolution.ToArray());Assert.That(b.State,Is.EqualTo(GameState.Won));Assert.That(b.ReactionDepth,Is.EqualTo(8));
        }
        [Test] public void ProtectedLevelsReferencesAndRuntimeRemainByteIdentical()
        {
            var lines=File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Batch31FrozenFiles.txt"));
            Assert.That(lines.Count(s=>s.Contains("/Data/Levels/Level")),Is.EqualTo(35));
            foreach(var line in lines){var p=line.Split('|');Assert.That(Hash(File.ReadAllBytes(Path.Combine(Application.dataPath,p[0]))),Is.EqualTo(p[1]),p[0]);}
            var catalog=File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Resources/LevelDesignCatalog.asset")).Split(new[]{"  - level: "},StringSplitOptions.None);
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Batch31CatalogFrozen.txt")))
            {var p=line.Split('|');Assert.That(Hash(System.Text.Encoding.UTF8.GetBytes(catalog[int.Parse(p[0])])),Is.EqualTo(p[1]),p[0]);}
        }
        [UnityTest] public IEnumerator ActualBoundaryProgressionFiveStartsAndFinaleCapture()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            new SaveService(Prefix).Save(new ProgressData(29,29,28));
            yield return new EnterPlayMode();yield return new WaitForSecondsRealtime(.25f);
            var game=Object.FindFirstObjectByType<PrototypeGame>();yield return Solve(game);
            GameObject.Find("Next Level").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level31"));
            for(int n=31;n<=35;n++)
            {
                yield return new WaitForSecondsRealtime(.25f);Assert.That(game.CurrentLevel.name,Does.StartWith("Level"+n));
                Assert.That(GameObject.Find("Alpine Lake"),Is.Not.Null);
                Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Is.EqualTo(game.CurrentLevel.Hint));
                SprintPresentationTests.Capture($"batch31-level{n}-start.png");yield return Solve(game);
                Assert.That(game.Telemetry.Result.perfectShift,Is.True);
                if(n==35)
                {
                    Assert.That(GameObject.Find("Chain").GetComponent<Text>().text,Does.StartWith("MEGA SHIFT"));
                    SprintPresentationTests.Capture("batch31-level35-payoff.png");
                }
                GameObject.Find("Next Level").GetComponent<Button>().onClick.Invoke();yield return null;
            }
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level36"));Assert.That(game.Progress.HighestUnlocked,Is.EqualTo(35));
            yield return new ExitPlayMode();
        }
        private static string Hash(byte[] bytes){using var sha=SHA256.Create();return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","");}
        private static BoardManager Run(LevelData l,params GridPosition[] taps)
        {
            var b=new BoardManager();b.Load(l);
            foreach(var tap in taps){Assert.That(b.RequestMove(tap),Is.True);Assert.That(b.ReactionDepth,Is.GreaterThan(0));b.CompleteResolution();}return b;
        }
        private static IEnumerator Solve(PrototypeGame game)
        {
            foreach(var tap in game.CurrentLevel.KnownSolution)
            {
                var p=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(p=>p.Data.Type==PieceType.Normal && p.Data.Position==tap);
                p.GetComponentInChildren<Button>().onClick.Invoke();double deadline=Time.realtimeSinceStartupAsDouble+5;
                while(game.Board.State==GameState.Resolving && Time.realtimeSinceStartupAsDouble<deadline)yield return null;
                Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
            }
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won));
        }
    }
}
