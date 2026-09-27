using System.IO;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    public static class Batch36Review
    {
        public static readonly string[][] DecisionThemes = {
            new[]{"Vacate receiving shelf", "Preserve shared turning point", "Predict color-specific destination"},
            new[]{"Stage Blue behind Red", "Redirect Red before yielding", "Yield upward from shared bay", "Preserve Blue's color route"},
            new[]{"Yellow lifts Red", "Red restores Yellow access", "Preserve the launcher before early delivery", "Keep each color in its accepting route"},
            new[]{"Stage below before crossing", "Shared lift versus separate delivery", "Use the vacated upper landing"},
            new[]{"Stage separated targets", "Align launcher", "Use closed gate as staging stop", "Preserve control access", "Divert waiting Red after Blue delivery"}
        };
        public static readonly GridPosition[] Normal39 = {new GridPosition(2,3),new GridPosition(0,2),new GridPosition(1,2),new GridPosition(3,3),new GridPosition(1,3),new GridPosition(2,2),new GridPosition(2,3)};
        [MenuItem("SHIFT/Analyze Levels 36–40")]
        public static void Analyze()
        {
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Validation/batch36"));Directory.CreateDirectory(dir);
            var levels=LevelValidation.AllLevels();
            for(int i=35;i<40;i++)File.WriteAllText(Path.Combine(dir,levels[i].name+"-analysis.json"),JsonUtility.ToJson(new PuzzleBatchAnalysis(levels[i]).Analyze(levels[i].KnownSolution),true));
        }
    }
}
