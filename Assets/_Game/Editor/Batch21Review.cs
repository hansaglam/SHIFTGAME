using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    public static class Batch21Review
    {
        [MenuItem("SHIFT/Analyze Levels 21-25")]
        public static void Analyze()
        {
            string directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../Validation/batch21"));
            Directory.CreateDirectory(directory);
            foreach(var level in LevelValidation.AllLevels().Skip(20).Take(5))
            {
                var result=new PuzzleBatchAnalysis(level).Analyze(level.KnownSolution);
                File.WriteAllText(Path.Combine(directory,level.name+"-analysis.json"),JsonUtility.ToJson(result,true));
                Debug.Log($"{level.DisplayTitle}: proven minimum {result.minimum}, {result.openings.Count} valid openings, " +
                    $"{result.openings.Count(o=>o.winningPaths>0)} viable openings, {result.solutionPaths} solution paths, " +
                    $"exact uniform-valid-action solve probability {result.randomSolveProbability:P4}, final event depth {result.finalDepth}.");
            }
        }
    }
}
