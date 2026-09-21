using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    public static class Batch31Review
    {
        // Authored themes are human review hypotheses, not counts inferred from repeated graph branches.
        public static readonly string[][] DecisionThemes={
            new[]{"Prepare the other zone before redirecting its occupant", "Continue parking until the operator vacates the choke", "Keep the wrong color out of the delivery room"},
            new[]{"Leave the exit-facing junction to reverse approach", "Use temporary parking without treating it as final", "Clear the return landing before descending through it"},
            new[]{"Rendezvous rather than send the waiting target alone", "Vacate the shared crossing without reoccupying it", "Use the rear target to redirect and push the front one"},
            new[]{"Reject the direct rotator entry", "Lift the target into the outer arc", "Clear the return cell while transferring the target", "Predict the opposite rotator approach and lower three turns"},
            new[]{"Stage the separated targets in a firing row", "Align the launcher after its landing is ready", "Use the closed gate as a staging stop", "Change gate state before the shared trigger", "Leave the operator out of the exit approach"}
        };
        public static readonly GridPosition[] Slower33={new GridPosition(2,1),new GridPosition(1,1),new GridPosition(2,3),new GridPosition(1,2),new GridPosition(2,3),new GridPosition(3,3),new GridPosition(4,2)};
        [MenuItem("SHIFT/Analyze Levels 31-35")]
        public static void Analyze()
        {
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../Validation/batch31"));Directory.CreateDirectory(dir);
            foreach(var l in LevelValidation.AllLevels().Skip(30).Take(5))
            {
                var r=new PuzzleBatchAnalysis(l).Analyze(l.KnownSolution);
                File.WriteAllText(Path.Combine(dir,l.name+"-analysis.json"),JsonUtility.ToJson(r,true));
                Debug.Log($"{l.DisplayTitle}: minimum {r.minimum}, budget {l.MoveLimit}, openings {r.openings.Count}/{r.openings.Count(o=>o.winningPaths>0)} winning, paths {r.solutionPaths}, probability {r.randomSolveProbability:P6}, states {r.states}, final/route/global depth {r.finalDepth}/{r.routeMaxDepth}/{r.maxDepth}.");
            }
        }
    }
}
