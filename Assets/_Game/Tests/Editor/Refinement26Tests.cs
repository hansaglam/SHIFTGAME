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
using Object=UnityEngine.Object;

namespace Shift.Game.Tests
{
    public sealed class Refinement26Tests
    {
        private const string Prefix="SHIFT.Tests.Refinement26.";
        [TearDown] public void Cleanup(){new SaveService(Prefix).Reset();new SettingsService(Prefix+"Settings.").Reset();}
        [Test] public void UpperLandingMustBeVacatedBeforeTheObviousSupportMoves()
        {
            var l=LevelValidation.AllLevels()[25];var wrong=new[]{new GridPosition(1,1)};
            var b=Run(l,wrong);
            Assert.That(b.GetOccupant(new GridPosition(2,2)).Color,Is.EqualTo(PieceColor.Yellow));
            Assert.That(b.GetOccupant(new GridPosition(2,2)).Direction,Is.EqualTo(Direction.Right));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrong),Is.Zero);
            b=Run(l,new GridPosition(1,2));Assert.That(b.GetOccupant(new GridPosition(0,2)).Color,Is.EqualTo(PieceColor.Yellow));
            Assert.That(b.GetOccupant(new GridPosition(1,2)),Is.Null);
            b=Run(l,new GridPosition(1,2),new GridPosition(1,1));
            Assert.That(b.GetOccupant(new GridPosition(1,1)),Is.Null);
            Assert.That(b.GetOccupant(new GridPosition(1,2)).Color,Is.EqualTo(PieceColor.Blue));
            Assert.That(new PuzzleBatchAnalysis(l).Analyze().openings.Count,Is.EqualTo(3));
        }
        [Test] public void BothBoxParkingDirectionsCommitButOnlyNorthPreservesTheReturn()
        {
            var l=LevelValidation.AllLevels()[26];var north=Run(l,new GridPosition(1,1));var east=Run(l,new GridPosition(0,2));
            Assert.That(north.GetOccupant(new GridPosition(1,3)).Type,Is.EqualTo(PieceType.PushBlock));
            Assert.That(east.GetOccupant(new GridPosition(2,1)).Type,Is.EqualTo(PieceType.PushBlock));
            var wrongOrder=new[]{new GridPosition(1,1),new GridPosition(0,2)};var b=Run(l,wrongOrder);
            Assert.That(b.GetOccupant(new GridPosition(2,1)).Color,Is.EqualTo(PieceColor.Blue));
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(wrongOrder),Is.Zero);
            b=Run(l,new GridPosition(1,1),new GridPosition(1,2));
            Assert.That(b.GetOccupant(new GridPosition(1,2)),Is.Null);
            Assert.That(b.GetOccupant(new GridPosition(1,4)).Type,Is.EqualTo(PieceType.PushBlock));
        }
        [TestCase(false,8)] [TestCase(true,9)]
        public void WrongFinalePreparationIsRecoverableButCostsVisibleMoves(bool afterOpening,int recoveryBudget)
        {
            var l=LevelValidation.AllLevels()[29];
            var prefix=afterOpening?new[]{new GridPosition(1,1),new GridPosition(2,1),new GridPosition(2,2),new GridPosition(3,0)}:new[]{new GridPosition(3,0)};
            var b=Run(l,prefix);
            if(afterOpening)
            {
                Assert.That(b.GetTerrain(new GridPosition(3,2)).GateOpen,Is.True);
                Assert.That(b.GetOccupant(new GridPosition(3,2)).Color,Is.EqualTo(PieceColor.Yellow),"An open gate is still unusable when occupied.");
                Assert.That(b.GetOccupant(new GridPosition(3,2)).Direction,Is.EqualTo(Direction.Up));
            }
            else Assert.That(b.GetOccupant(new GridPosition(3,1)).Color,Is.EqualTo(PieceColor.Yellow),"Premature access setup occupies box staging.");
            Assert.That(new PuzzleBatchAnalysis(l).WinningContinuations(prefix),Is.Zero);
            var extended=Object.Instantiate(l);
            try
            {
                extended.Configure(l.Width,l.Height,recoveryBudget-1,l.TargetColor,l.Placements);
                Assert.That(new PuzzleBatchAnalysis(extended).WinningContinuations(prefix),Is.Zero);
                extended.Configure(l.Width,l.Height,recoveryBudget,l.TargetColor,l.Placements);
                var recovery=afterOpening?
                    prefix.Concat(new[]{new GridPosition(3,2)}).Concat(l.KnownSolution.Skip(3)).ToArray():
                    prefix.Concat(l.KnownSolution).ToArray();
                Assert.That(recovery.Length,Is.EqualTo(recoveryBudget));b=Run(extended,recovery);
                Assert.That(b.State,Is.EqualTo(GameState.Won));
                if(afterOpening) Assert.That(b.GetOccupant(new GridPosition(3,3)).Color,Is.EqualTo(PieceColor.Yellow));
                else Assert.That(b.GetOccupant(new GridPosition(5,1)).Color,Is.EqualTo(PieceColor.Yellow));
            }
            finally {Object.DestroyImmediate(extended);}
        }
        [Test] public void AllThirtySevenOtherPuzzlesAndLockedSystemsStayByteIdentical()
        {
            var lines=File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Refinement26FrozenFiles.txt"));
            Assert.That(lines.Count(s=>s.Contains("/Data/Levels/Level")),Is.EqualTo(37));
            foreach(var line in lines){var p=line.Split('|');Assert.That(Hash(File.ReadAllBytes(Path.Combine(Application.dataPath,p[0]))),Is.EqualTo(p[1]),p[0]);}
            var catalog=File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Resources/LevelDesignCatalog.asset")).Split(new[]{"  - level: "},StringSplitOptions.None);
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Refinement26CatalogFrozen.txt")))
            {var p=line.Split('|');Assert.That(Hash(System.Text.Encoding.UTF8.GetBytes(catalog[int.Parse(p[0])])),Is.EqualTo(p[1]),p[0]);}
        }
        [UnityTest] public IEnumerator ThreeRevisedStartsAndTheFinalPayoffUseCurrentIdentity()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return null;
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.UnlockAllLevels();
            foreach(int n in new[]{26,27,30})
            {
                Assert.That(game.SelectLevel(n-1),Is.True);yield return new WaitForSecondsRealtime(.25f);
                Assert.That(GameObject.Find("Alpine Lake"),Is.Not.Null);
                SprintPresentationTests.Capture($"refine26-level{n}-start.png");
            }
            foreach(var tap in game.CurrentLevel.KnownSolution)
            {
                var p=Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(p=>p.Data.Type==PieceType.Normal && p.Data.Position==tap);
                p.GetComponentInChildren<Button>().onClick.Invoke();double deadline=Time.realtimeSinceStartupAsDouble+5;
                while(game.Board.State==GameState.Resolving && Time.realtimeSinceStartupAsDouble<deadline)yield return null;
                Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));
            }
            Assert.That(game.Board.State,Is.EqualTo(GameState.Won));Assert.That(game.Telemetry.Result.perfectShift,Is.True);
            Assert.That(GameObject.Find("Chain").GetComponent<Text>().text,Does.StartWith("MEGA SHIFT"));
            SprintPresentationTests.Capture("refine26-level30-payoff.png");yield return new ExitPlayMode();
        }
        private static string Hash(byte[] bytes){using var sha=SHA256.Create();return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","");}
        private static BoardManager Run(LevelData l,params GridPosition[] taps)
        {
            var b=new BoardManager();b.Load(l);
            foreach(var tap in taps){Assert.That(b.RequestMove(tap),Is.True);Assert.That(b.ReactionDepth,Is.GreaterThan(0));b.CompleteResolution();}
            return b;
        }
    }
}
