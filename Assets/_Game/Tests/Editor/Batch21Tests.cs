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
using Object = UnityEngine.Object;

namespace Shift.Game.Tests
{
    public sealed class Batch21Tests
    {
        private const string Prefix="SHIFT.Tests.Batch21.";
        [TearDown] public void Cleanup() { new SaveService(Prefix).Reset();new SettingsService(Prefix+"Settings.").Reset(); }
        [TestCase(20,3,2,1,1,.125,4)]
        [TestCase(21,4,2,1,1,.0625,2)]
        [TestCase(22,5,2,1,1,1d/72,2)]
        [TestCase(23,5,2,1,1,1d/384,4)]
        [TestCase(24,6,3,2,2,1d/48,4)]
        public void ExhaustiveMetricsKnownSolutionsAndIndependentMinimumProof(int index,int minimum,int openings,int useful,int paths,double probability,int finalDepth)
        {
            var level=LevelValidation.AllLevels()[index];
            Assert.That(LevelValidation.VerifySolution(level,out string error,out _),Is.True,error);
            foreach(var gate in level.Placements.Where(p=>p.type==PieceType.Gate))
                Assert.That(level.Placements.Any(p=>(p.type==PieceType.Normal || p.type==PieceType.PushBlock) && p.position==gate.position),Is.False,"Initial gate state must remain visible.");
            var result=new PuzzleBatchAnalysis(level).Analyze(level.KnownSolution);
            Assert.That(result.minimum,Is.EqualTo(minimum));Assert.That(level.VerifiedOptimalMoveCount,Is.EqualTo(minimum));
            Assert.That(level.KnownSolutionLength,Is.EqualTo(minimum));Assert.That(level.MoveLimit,Is.EqualTo(minimum));
            Assert.That(result.openings.Count,Is.EqualTo(openings));Assert.That(result.openings.Count(o=>o.winningPaths>0),Is.EqualTo(useful));
            Assert.That(result.solutionPaths,Is.EqualTo(paths));Assert.That(result.randomSolveProbability,Is.EqualTo(probability).Within(1e-12));
            Assert.That(result.meaningfulDecisions,Is.GreaterThanOrEqualTo(index<22?2:3));
            Assert.That(result.finalDepth,Is.EqualTo(finalDepth));
            if(index>=22) Assert.That(result.randomSolveProbability,Is.LessThan(.05));
            int nodes=0;
            Assert.That(DesignValidation.CanWin(level,new List<GridPosition>(),minimum-1,ref nodes),Is.False,"Independent exhaustive shorter-route check.");
            nodes=0;Assert.That(DesignValidation.CanWin(level,new List<GridPosition>(),2,ref nodes),Is.False);
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Validation/batch21"));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,level.name+"-analysis.json"),JsonUtility.ToJson(result,true));
        }
        [Test] public void AnalysisBoundNeverProducesAPartialCertificate()
        { Assert.Throws<InvalidOperationException>(()=>new PuzzleBatchAnalysis(LevelValidation.AllLevels()[24],1).Analyze()); }
        [Test] public void SwitchDiscoveryHasOnePadOneGateAndWrongColorBlocksTheExitApproach()
        {
            var l=LevelValidation.AllLevels()[20];Assert.That(l.Placements.Count(p=>p.type==PieceType.Switch),Is.EqualTo(1));
            Assert.That(l.Placements.Count(p=>p.type==PieceType.Gate),Is.EqualTo(1));
            var b=Run(l,new GridPosition(2,0));
            Assert.That(b.GetOccupant(new GridPosition(2,1)).Color,Is.EqualTo(PieceColor.Yellow));
            Assert.That(b.GetOccupant(new GridPosition(2,1)).Direction,Is.EqualTo(Direction.Up));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(new[]{new GridPosition(2,0)}),Is.Zero);
        }
        [Test] public void PrematureTargetPushRedirectsTheOnlyGateOperatorAwayFromItsPad()
        {
            var l=LevelValidation.AllLevels()[21];var wrong=new[]{new GridPosition(0,2)};var b=Run(l,wrong);
            Assert.That(b.GetOccupant(new GridPosition(2,2)).Color,Is.EqualTo(PieceColor.Blue));
            Assert.That(b.GetOccupant(new GridPosition(2,2)).Direction,Is.EqualTo(Direction.Right));
            Assert.That(b.GetTerrain(new GridPosition(3,2)).GateOpen,Is.False);
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
        }
        [Test] public void SecondPressMustWaitUntilTheTargetHasCrossedTheFirstGate()
        {
            var l=LevelValidation.AllLevels()[22];var b=Run(l,new GridPosition(4,1));
            Assert.That(b.GetTerrain(new GridPosition(1,2)).GateOpen,Is.True);
            Assert.That(b.GetTerrain(new GridPosition(3,2)).GateOpen,Is.False);
            var wrong=new[]{new GridPosition(4,1),new GridPosition(3,1)};b=Run(l,wrong);
            Assert.That(b.GetTerrain(new GridPosition(1,2)).GateOpen,Is.False);
            Assert.That(b.GetTerrain(new GridPosition(3,2)).GateOpen,Is.True);
            Assert.That(b.GetOccupant(new GridPosition(0,2)).Color,Is.EqualTo(PieceColor.Red));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
            var correct=Run(l,l.KnownSolution.ToArray());
            Assert.That(correct.State,Is.EqualTo(GameState.Won));
        }
        [Test] public void FrontTargetAloneWastesTheSharedPushOpportunity()
        {
            var l=LevelValidation.AllLevels()[23];var wrong=new[]{new GridPosition(2,2),new GridPosition(2,3)};
            var b=Run(l,wrong);Assert.That(b.GetOccupant(new GridPosition(2,2)).Color,Is.EqualTo(PieceColor.Red));
            Assert.That(b.GetOccupant(new GridPosition(2,4)).Color,Is.EqualTo(PieceColor.Red));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
            var extended=Object.Instantiate(l);
            try
            {
                extended.Configure(l.Width,l.Height,l.MoveLimit+1,l.TargetColor,l.Placements);
                Assert.That(new PuzzleBatchAnalysis(extended).WinningContinuations(wrong),Is.GreaterThan(0),"Bad order costs a move, not a hidden deadlock.");
            }
            finally { Object.DestroyImmediate(extended); }
        }
        [Test] public void LiftingTheBoxIsAVisibleButLosingSetupWithinBudget()
        {
            var l=LevelValidation.AllLevels()[24];var wrong=new[]{new GridPosition(1,0)};var b=Run(l,wrong);
            Assert.That(b.GetOccupant(new GridPosition(1,2)).Type,Is.EqualTo(PieceType.PushBlock));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
            b=Run(l,new GridPosition(3,1));Assert.That(b.GetOccupant(new GridPosition(3,0)).Color,Is.EqualTo(PieceColor.Blue));
            Assert.That(b.GetOccupant(new GridPosition(0,1)).Color,Is.EqualTo(PieceColor.Red));
        }
        [Test] public void ProtectedLevelsSimulationPresentationAndSaveFilesRemainByteIdentical()
        {
            var lines=File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Batch21FrozenFiles.txt"));
            Assert.That(lines.Count(l=>l.Contains("/Data/Levels/Level")),Is.EqualTo(35));
            foreach(var line in lines)
            {
                var pair=line.Split('|');using var sha=SHA256.Create();
                Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,pair[0])))).Replace("-",""),Is.EqualTo(pair[1]),pair[0]);
            }
            // Metadata for all 35 out-of-scope levels is frozen independently of the five edited entries.
            var catalog=File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Resources/LevelDesignCatalog.asset"));
            var entries=catalog.Split(new[]{"  - level: "},StringSplitOptions.None);
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Batch21CatalogFrozen.txt")))
            {
                var pair=line.Split('|');using var sha=SHA256.Create();
                string hash=BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(entries[int.Parse(pair[0])]))).Replace("-","");
                Assert.That(hash,Is.EqualTo(pair[1]),"Catalog entry "+pair[0]);
            }
        }
        [UnityTest] public IEnumerator FiveScreensAndBothBatchBoundaryTransitionsUseRealControls()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            new SaveService(Prefix).Save(new ProgressData(19,19,18));
            yield return new EnterPlayMode();yield return new WaitForSecondsRealtime(.25f);
            var game=Object.FindFirstObjectByType<PrototypeGame>();
            yield return Solve(game);GameObject.Find("Next Level").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level21"));
            for(int n=21;n<=25;n++)
            {
                yield return new WaitForSecondsRealtime(.25f);
                Assert.That(game.CurrentLevel.name,Does.StartWith("Level"+n));
                Assert.That(GameObject.Find("Alpine Lake"),Is.Not.Null);
                Assert.That(GameObject.Find("Status").GetComponent<Text>().text,Is.EqualTo(game.CurrentLevel.Hint));
                SprintPresentationTests.Capture($"batch21-level{n}.png");
                yield return Solve(game);
                Assert.That(game.Telemetry.Result.perfectShift,Is.True);
                GameObject.Find("Next Level").GetComponent<Button>().onClick.Invoke();yield return null;
            }
            Assert.That(game.CurrentLevel.name,Does.StartWith("Level26"));
            Assert.That(game.Progress.HighestUnlocked,Is.EqualTo(25));
            yield return new ExitPlayMode();
        }
        private static BoardManager Run(LevelData l,params GridPosition[] taps)
        {
            var b=new BoardManager();b.Load(l);
            foreach(var p in taps) { Assert.That(b.RequestMove(p),Is.True);Assert.That(b.ReactionDepth,Is.GreaterThan(0));b.CompleteResolution(); }
            return b;
        }
        private static IEnumerator Solve(PrototypeGame game)
        {
            foreach(var tap in game.CurrentLevel.KnownSolution)
            {
                var p=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(p=>p.Data.Type==PieceType.Normal && p.Data.Position==tap);
                p.GetComponentInChildren<Button>().onClick.Invoke();double deadline=Time.realtimeSinceStartupAsDouble+5;
                while(game.Board.State==GameState.Resolving && Time.realtimeSinceStartupAsDouble<deadline) yield return null;
                Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
            }
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won));
        }
    }
}
