using System.Collections.Generic;
using NUnit.Framework;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    public sealed class ChapterLevelTests
    {
        [Test] public void LevelThirteenUsesTwoDistinctChains()
        {
            var level=LevelValidation.ChapterLevels()[12]; var board=new BoardManager(); board.Load(level);
            board.RequestMove(level.KnownSolution[0]); Assert.That(board.ReactionDepth,Is.InRange(3,5));
            board.CompleteResolution(); Assert.That(board.State,Is.EqualTo(GameState.Playing));
            Assert.That(board.GetOccupant(new GridPosition(3,1)).Color,Is.EqualTo(PieceColor.Red));
            board.RequestMove(level.KnownSolution[1]); Assert.That(board.ReactionDepth,Is.InRange(3,5));
            board.CompleteResolution(); Assert.That(board.State,Is.EqualTo(GameState.Won));
        }
        [Test] public void TwentyLevelsAreWiredAndEverySolutionWins()
        {
            var levels = LevelValidation.ChapterLevels(); Assert.That(levels.Length,Is.EqualTo(20));
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);
            var game = UnityEngine.Object.FindFirstObjectByType<PrototypeGame>();
            var list = new UnityEditor.SerializedObject(game).FindProperty("levels"); Assert.That(list.arraySize,Is.EqualTo(40));
            for(int i=0;i<levels.Length;i++)
            {
                Assert.That(list.GetArrayElementAtIndex(i).objectReferenceValue,Is.EqualTo(levels[i]));
                Assert.That(levels[i].Width,Is.InRange(4,6)); Assert.That(levels[i].Height,Is.InRange(4,6));
                Assert.That(LevelValidation.VerifySolution(levels[i],out string error,out _),Is.True,levels[i].name+": "+error);
                var board = new BoardManager(); board.Load(levels[i]); Assert.That(board.State,Is.EqualTo(GameState.Playing));
            }
        }
        [TestCase(14,5)] [TestCase(17,6)] [TestCase(19,6)]
        public void HardLevelsMeetMinimumMoveBudget(int index,int minimum)
        {
            var level = LevelValidation.ChapterLevels()[index];
            Assert.That(level.KnownSolution.Count,Is.EqualTo(minimum));
            int nodes = 0;
            Assert.That(CanWin(level,new List<GridPosition>(),minimum-1,ref nodes),Is.False,$"{level.name} has a shorter solution");
            Assert.That(LevelValidation.VerifySolution(level,out string error,out int depth),Is.True,error);
            if(index==19) Assert.That(depth,Is.GreaterThanOrEqualTo(4));
        }
        private static bool CanWin(LevelData level,List<GridPosition> path,int budget,ref int nodes)
        {
            Assert.That(++nodes,Is.LessThan(100000),"Bounded search limit reached; minimum not verified.");
            var board = new BoardManager(); board.Load(level);
            foreach(var tap in path)
            {
                if(!board.RequestMove(tap)||board.Actions.Count==0) return false;
                board.CompleteResolution();
            }
            if(board.State==GameState.Won) return true;
            if(path.Count>=budget||board.State!=GameState.Playing) return false;
            foreach(var piece in board.Pieces)
            {
                if(!piece.Active||piece.Type!=PieceType.Normal) continue;
                path.Add(piece.Position); bool win=CanWin(level,path,budget,ref nodes); path.RemoveAt(path.Count-1);
                if(win) return true;
            }
            return false;
        }
        [Test] public void LevelTwelveWrongOrderTrapsBlueButRestartRestores()
        {
            var level=LevelValidation.ChapterLevels()[11]; var board=new BoardManager(); board.Load(level);
            board.RequestMove(new GridPosition(0,1)); board.CompleteResolution();
            Assert.That(board.GetOccupant(new GridPosition(2,1)).Color,Is.EqualTo(PieceColor.Blue));
            board.RequestMove(new GridPosition(2,1)); Assert.That(board.Actions,Is.Empty);
            board.Load(level); Assert.That(board.GetOccupant(new GridPosition(1,1)).Color,Is.EqualTo(PieceColor.Blue));
        }
    }
}
