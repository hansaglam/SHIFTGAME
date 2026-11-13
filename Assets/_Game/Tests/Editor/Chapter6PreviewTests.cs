using System.Linq;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Tests
{
    // Pure editor checks: never creates the isolated scene or writes player saves.
    public sealed class Chapter6PreviewTests
    {
        [Test]
        public void AllOriginalHundredLevelAssetReferencesArePreserved()
        {
            var original = ChapterJourneyScenePreview.Load100();
            var draft = Chapter6TetherPrototypes.Load104();
            Assert.That(original.Length, Is.EqualTo(100));
            Assert.That(draft.Length, Is.EqualTo(104));
            for (int i = 0; i < 100; i++)
                Assert.That(draft[i], Is.SameAs(original[i]),
                    "Prototype must not replace released/draft Level " + (i + 1));
            for (int n = 101; n <= 104; n++)
                Assert.That(draft[n - 1], Is.SameAs(
                    Chapter6TetherPrototypes.Get(n)));
        }

        [Test]
        public void TetherPreviewMustBeExcludedFromProductionBuildAndSaves()
        {
            Assert.That(Chapter6TetherPrototypes.PreviewScene,
                Is.Not.EqualTo(PrototypeSetup.ScenePath));
            Assert.That(Chapter6TetherPrototypes.PreviewScene,
                Is.Not.EqualTo(ChapterJourneyScenePreview.Preview100ScenePath));
            foreach (var scene in EditorBuildSettings.scenes)
                Assert.That(scene.path,
                    Is.Not.EqualTo(Chapter6TetherPrototypes.PreviewScene));
            Assert.That(Chapter6TetherPrototypes.ProgressPrefix,
                Does.StartWith("SHIFT.Preview."));
            Assert.That(Chapter6TetherPrototypes.SettingsPrefix,
                Does.StartWith("SHIFT.Preview."));
            Assert.That(Chapter6TetherPrototypes.ProgressPrefix,
                Is.Not.EqualTo(ChapterJourneyScenePreview.Preview100ProgressPrefix));
        }

        [TestCase(GameLanguage.English)]
        [TestCase(GameLanguage.Turkish)]
        public void FourPuzzleConceptsHaveSpecificTranslatedHints(GameLanguage language)
        {
            for (int n = 101; n <= 104; n++)
            {
                var level = Chapter6TetherPrototypes.Get(n);
                Assert.That(level, Is.Not.Null);
                Assert.That(TetherPrototypeLocalization.TryNumber(
                    level, out int number), Is.True);
                Assert.That(number, Is.EqualTo(n));
                Assert.That(TetherPrototypeLocalization.TryTitle(
                    level, language, out string title), Is.True);
                Assert.That(TetherPrototypeLocalization.TryInstruction(
                    level, language, out string hint), Is.True);
                Assert.That(title, Does.StartWith(n + " - "));
                Assert.That(hint.Length, Is.GreaterThan(30));
                Assert.That(title, Does.Not.Contain("SHIFT"));
            }
        }

        [Test]
        public void EditorCanRecoverMissingPilotAssetReferencesWithoutChangingFirstHundred()
        {
            var original = Chapter6TetherPrototypes.Load104();
            var copied = (LevelData[])original.Clone();
            for (int i = 100; i < 104; i++)
                copied[i] = null;
            Assert.That(TetherPreviewAssetRepair.RestoreMissing(copied,
                TetherPreviewAssetRepair.PreviewSavePrefix),
                Is.EqualTo(4));
            for (int i = 0; i < copied.Length; i++)
                Assert.That(copied[i], Is.SameAs(original[i]),
                    "Only missing BAĞ pilot reference " + (i + 1) +
                    " may be restored.");
            Assert.That(TetherPreviewAssetRepair.RestoreMissing(copied,
                TetherPreviewAssetRepair.PreviewSavePrefix), Is.Zero,
                "Repairing an already intact preview must be a no-op.");
        }

        [Test]
        public void PreviewRepairCannotAffectPublishedOrOtherDraftCampaigns()
        {
            var original = Chapter6TetherPrototypes.Load104();
            var test = (LevelData[])original.Clone();
            test[100] = null;
            Assert.That(TetherPreviewAssetRepair.RestoreMissing(test,
                "SHIFT.Progress.v1."), Is.Zero);
            Assert.That(test[100], Is.Null);
            Assert.That(TetherPreviewAssetRepair.RestoreMissing(test,
                ChapterJourneyScenePreview.Preview100ProgressPrefix),
                Is.Zero);
            Assert.That(test[100], Is.Null);
            Assert.Throws<System.InvalidOperationException>(() =>
                TetherPreviewAssetRepair.RestoreMissing(
                    new LevelData[100],
                    TetherPreviewAssetRepair.PreviewSavePrefix),
                "A malformed BAĞ preview must not silently mutate a 100-level scene.");
        }

        [Test]
        public void Level101To104AreDistinctMechanicProblemsNotClonedBoards()
        {
            var sizes = new System.Collections.Generic.HashSet<string>();
            var routes = new System.Collections.Generic.HashSet<string>();
            for (int n = 101; n <= 104; n++)
            {
                var level = Chapter6TetherPrototypes.Get(n);
                Assert.That(level, Is.Not.Null);
                Assert.That(MechanicGuideCatalog.Contains(level,
                    MechanicTopic.Tether), Is.True);
                Assert.That(level.Placements.Count(p =>
                    p.type == PieceType.Normal && p.tetherId == 1),
                    Is.EqualTo(2));
                var signature = level.Width + "x" + level.Height + ":";
                foreach (var p in level.Placements)
                    signature += p.type + "," + p.color + "," +
                        p.direction + "," + p.position + "," +
                        p.channel + "," + p.tetherId + ";";
                Assert.That(sizes.Add(signature), Is.True,
                    "Do not publish duplicated BAĞ boards.");
                var solution = string.Join("|", level.KnownSolution
                    .Select(p => p.ToString()));
                Assert.That(routes.Add(solution), Is.True,
                    "Do not simply copy the same tap solution.");
                Assert.That(level.KnownSolutionLength, Is.InRange(3, 5));
            }
        }
    }
}
