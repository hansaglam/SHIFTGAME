using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEngine;

namespace Shift.Game.Tests
{
    public sealed class LevelDesignTests
    {
        [Test] public void ShippedLevelsOneThroughTwentyNineAndSimulationAreByteIdentical()
        {
            var lines = File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/Sprint5FrozenFiles.txt"));
            Assert.That(lines.Length, Is.EqualTo(30));
            foreach (var line in lines)
            {
                var pair = line.Split('|'); using var sha = SHA256.Create();
                string hash = System.BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,pair[0])))).Replace("-","");
                Assert.That(hash, Is.EqualTo(pair[1]), pair[0]);
            }
        }
        [Test] public void AllFortyDesignsAndCreativeTagsValidate()
        {
            Assert.That(DesignValidation.ValidateCatalog(out string error), Is.True, error);
            CollectionAssert.AreEquivalent(System.Enum.GetValues(typeof(PuzzleArchetype)), LevelDesignCatalog.Current.Entries.Select(e => e.primary).Distinct());
            Assert.That(LevelDesignCatalog.Current.Entries.Count(e => e.creativeCandidate), Is.InRange(4,6));
            Assert.That(LevelValidation.AllLevels()[38].Design.primary, Is.EqualTo(PuzzleArchetype.Optimization));
        }
        [TestCase(29,8,7)] [TestCase(33,8,5)] [TestCase(37,6,7)] [TestCase(39,14,7)]
        public void FinalTapHasAuthoredPayoff(int index, int depth, int taps)
        {
            var level = LevelValidation.AllLevels()[index]; var b = new BoardManager(); b.Load(level);
            foreach (var tap in level.KnownSolution) { Assert.That(b.RequestMove(tap), Is.True); b.CompleteResolution(); }
            Assert.That(b.State, Is.EqualTo(GameState.Won)); Assert.That(b.ReactionDepth, Is.GreaterThanOrEqualTo(depth));
            Assert.That(level.KnownSolutionLength, Is.EqualTo(taps)); Assert.That(level.Design.expectedPayoffDepth, Is.EqualTo(depth));
        }
        [TestCase(29,7)] [TestCase(33,5)] [TestCase(36,6)] [TestCase(38,5)] [TestCase(39,7)]
        public void ProveRedesignedMinimum(int index, int expected)
        {
            var level = LevelValidation.AllLevels()[index]; int nodes = 0;
            Assert.That(LevelValidation.VerifySolution(level, out string error, out _), Is.True, error);
            Assert.That(level.KnownSolutionLength, Is.EqualTo(expected));
            Assert.That(DesignValidation.CanWin(level, new List<GridPosition>(), expected - 1, ref nodes), Is.False, "Shorter route exists.");
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Validation/proofs")); Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, level.name + ".txt"), VerifiedOptimality.Fingerprint(level) + "," + expected + "," + nodes);
        }
        [Test] public void OppositeRoomsMustFinishBeforeSecondToggle()
        {
            // Retain the shipped parity regression as a fixture while campaign Level 32 now tests position reversal.
            var level = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>("Assets/_Game/Tests/Editor/Fixtures/LegacyOppositeRooms.asset"); var b = new BoardManager(); b.Load(level); int switches = 0;
            foreach (var tap in level.KnownSolution) { b.RequestMove(tap); switches += b.Actions.Count(a => a.Type == BoardActionType.SwitchActivated); b.CompleteResolution(); }
            Assert.That(switches, Is.EqualTo(2));
            var wrong = new List<GridPosition> { new GridPosition(0,3),new GridPosition(1,3),new GridPosition(0,1),new GridPosition(1,1) };
            int nodes = 0; Assert.That(DesignValidation.CanWin(level, wrong, level.MoveLimit, ref nodes), Is.False);
        }
        [Test] public void FinaleUpwardBoxDecoyCannotCompleteWithinBudget()
        {
            var level = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>("Assets/_Game/Tests/Editor/Fixtures/LegacyLevel40.asset"); int nodes = 0;
            Assert.That(DesignValidation.CanWin(level, new List<GridPosition> { new GridPosition(1,0) }, level.MoveLimit, ref nodes), Is.False);
        }
        [Test] public void OptimizationHasTwoMeaningfulRoutesAndOnlyEfficientRouteIsPerfect()
        {
            var level = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>("Assets/_Game/Tests/Editor/Fixtures/LegacyLevel39.asset");
            var longer = new[] { new GridPosition(0,0), new GridPosition(1,2), new GridPosition(2,2), new GridPosition(0,2), new GridPosition(1,2), new GridPosition(2,2) };
            foreach (var route in new IEnumerable<GridPosition>[] { level.KnownSolution, longer })
            {
                var b = new BoardManager(); b.Load(level); var t = new TelemetryTracker(null, () => 0); t.StartLevel(level, 38, level.VerifiedOptimalMoveCount);
                foreach (var tap in route) { b.RequestMove(tap); Assert.That(b.Actions.Count, Is.GreaterThan(0)); t.AcceptedTap(b.ReactionDepth,b.MovesRemaining,false); b.CompleteResolution(); }
                Assert.That(b.State, Is.EqualTo(GameState.Won)); t.Finish(true);
                Assert.That(t.Result.perfectShift, Is.EqualTo(route.Count() == 4));
            }
        }
        [TestCase(0,ReactionTier.Normal)] [TestCase(1,ReactionTier.Normal)] [TestCase(2,ReactionTier.Chain)]
        [TestCase(3,ReactionTier.Chain)] [TestCase(4,ReactionTier.BigShift)] [TestCase(5,ReactionTier.BigShift)] [TestCase(6,ReactionTier.MegaShift)] [TestCase(256,ReactionTier.MegaShift)]
        public void TierBoundaries(int depth, ReactionTier expected) => Assert.That(ReactionPresentation.Classify(depth), Is.EqualTo(expected));
        [Test] public void MetadataTelemetrySnapshotsPayoffAndNoOpRemainIndependent()
        {
            var level = LevelValidation.AllLevels()[39]; var provider = new LocalAnalyticsService(); var t = new TelemetryTracker(provider, () => 12);
            t.StartLevel(level,39,null); var start = provider.Events.Last().attempt;
            Assert.That(start.archetype, Is.EqualTo("Rooms")); Assert.That(start.difficultyBand, Is.EqualTo("Finale"));
            Assert.That(start.creativeCandidate, Is.True); Assert.That(start.hasPayoffMove, Is.True); Assert.That(start.payoffReached, Is.False);
            t.AcceptedTap(8,6,true); Assert.That(t.Snapshot().payoffReached, Is.False);
            t.AcceptedTap(4,5,false); Assert.That(t.Snapshot().maxReactionTier, Is.EqualTo("BigShift")); Assert.That(t.Snapshot().payoffReached, Is.False);
            t.AcceptedTap(14,4,false); Assert.That(t.Snapshot().payoffReached, Is.True); Assert.That(t.Snapshot().maxReactionTier, Is.EqualTo("MegaShift"));
            Assert.That(start.payoffReached, Is.False); t.StartLevel(level,39,null,true); Assert.That(t.Snapshot().payoffReached, Is.False); Assert.That(t.Snapshot().restarts, Is.EqualTo(1));
            var noOp = new TelemetryTracker(new NullAnalyticsService(), () => 0); noOp.StartLevel(level,39,null); noOp.AcceptedTap(14,5,false); noOp.Finish(true);
            Assert.That(noOp.Result.payoffReached, Is.True); Assert.That(noOp.Result.perfectShift, Is.False);
        }
        [Test] public void PuzzleMutationInvalidatesCertificateWhileDesignLabelsDoNot()
        {
            var level = LevelValidation.AllLevels()[39]; var clone = Object.Instantiate(level);
            try
            {
                Assert.That(clone.VerifiedOptimalMoveCount, Is.EqualTo(level.VerifiedOptimalMoveCount));
                clone.Configure(level.Width,level.Height,level.MoveLimit+1,level.TargetColor,level.Placements);
                clone.ConfigureTargetColors(level.ResolvedTargetColors);
                Assert.That(clone.VerifiedOptimalMoveCount, Is.Null);
                var d = level.Design; var prior = d.difficultyBand;
                try { d.difficultyBand = DifficultyBand.Intro; Assert.That(level.VerifiedOptimalMoveCount, Is.Not.Null); }
                finally { d.difficultyBand = prior; }
            }
            finally { Object.DestroyImmediate(clone); }
        }
    }
}
