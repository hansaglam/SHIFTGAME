using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    public sealed class ChapterTwoTests
    {
        [Test] public void ChapterTwoSolutionsValidateAndAreDeterministic([Range(20,39)] int index)
        {
            var level=LevelValidation.AllLevels()[index];
            Assert.That(level.ValidatePlayable(out string error),Is.True,error);
            Assert.That(level.Width,Is.LessThanOrEqualTo(6)); Assert.That(level.Height,Is.LessThanOrEqualTo(6));
            Assert.That(LevelValidation.VerifySolution(level,out error,out int max),Is.True,level.name+": "+error);
            var first=Run(level); CollectionAssert.AreEqual(first,Run(level));
            if(index==39)
            {
                Assert.That(max,Is.GreaterThanOrEqualTo(6)); Assert.That(level.KnownSolutionLength,Is.InRange(6,8));
                Assert.That(level.ResolvedTargetColors.Count,Is.GreaterThanOrEqualTo(2));
            }
        }
        private static List<BoardAction> Run(LevelData level)
        {
            var b=new BoardManager(); b.Load(level); var result=new List<BoardAction>();
            foreach(var tap in level.KnownSolution) { b.RequestMove(tap); result.AddRange(b.Actions); b.CompleteResolution(); }
            Assert.That(b.State,Is.EqualTo(GameState.Won)); return result;
        }
        [TestCase(29,7)] [TestCase(33,5)] [TestCase(36,6)] [TestCase(38,5)] [TestCase(39,7)]
        public void SelectedPuzzlesHaveVerifiedMinimum(int index,int expected)
        {
            var level=LevelValidation.AllLevels()[index]; int nodes=0;
            Assert.That(level.VerifiedOptimalMoveCount,Is.EqualTo(expected));
            Assert.That(level.KnownSolutionLength,Is.EqualTo(expected));
            Assert.That(LevelValidation.VerifySolution(level,out _,out _),Is.True);
            Assert.That(CanWin(level,new List<GridPosition>(),expected-1,ref nodes),Is.False);
        }
        private static bool CanWin(LevelData level,List<GridPosition> path,int budget,ref int nodes)
        {
            Assert.That(++nodes,Is.LessThan(200000),"Bound reached; optimum not verified.");
            var b=new BoardManager(); b.Load(level);
            foreach(var tap in path) { if(!b.RequestMove(tap)||b.Actions.Count==0) return false; b.CompleteResolution(); }
            if(b.State==GameState.Won) return true;
            if(path.Count>=budget || b.State!=GameState.Playing) return false;
            foreach(var p in b.Pieces)
            {
                if(!p.Active || p.Type!=PieceType.Normal) continue;
                path.Add(p.Position); bool win=CanWin(level,path,budget,ref nodes); path.RemoveAt(path.Count-1); if(win) return true;
            }
            return false;
        }
        [Test] public void LevelThirtySixWrongSwitchOrderIsADeadEnd()
        {
            var level=UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>("Assets/_Game/Tests/Editor/Fixtures/LegacyLevel36.asset"); int nodes=0;
            Assert.That(CanWin(level,new List<GridPosition>{new GridPosition(0,0),new GridPosition(1,0)},level.MoveLimit,ref nodes),Is.False);
        }
        [Test] public void OriginalFinaleStillUsesTwoDistinctSwitchActivations()
        {
            var level=UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>("Assets/_Game/Tests/Editor/Fixtures/LegacyLevel40.asset");
            Assert.That(Run(level).Count(a=>a.Type==BoardActionType.SwitchActivated),Is.GreaterThanOrEqualTo(2));
        }
        [TestCase(19,20)] [TestCase(4,4)]
        public void OldCompletedChapterMigratesWithoutResettingReplay(int oldCurrent,int expectedCurrent)
        {
            var save=new SaveService("SHIFT.Tests.Migration."); save.Reset();
            try
            {
                save.Save(new ProgressData(19,oldCurrent,19)); var p=new LevelProgression(40,save);
                Assert.That(p.HighestUnlocked,Is.EqualTo(20)); Assert.That(p.Current,Is.EqualTo(expectedCurrent)); Assert.That(p.HighestCompleted,Is.EqualTo(19));
                Assert.That(new LevelProgression(40,save).Current,Is.EqualTo(expectedCurrent));
            }
            finally { save.Reset(); }
        }
        [Test] public void IncompleteOldSaveDoesNotUnlockChapterTwo()
        {
            var save=new SaveService("SHIFT.Tests.Migration."); save.Reset();
            try { save.Save(new ProgressData(19,19,18)); Assert.That(new LevelProgression(40,save).IsUnlocked(20),Is.False); }
            finally { save.Reset(); }
        }
    }
}
