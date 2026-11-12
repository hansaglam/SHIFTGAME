using NUnit.Framework;
using UnityEngine;

namespace Shift.Game.Tests
{
    // No screenshot assumptions: verify that the saved progression frontier,
    // not chapter access or visual animation, controls every restored node.
    public sealed class ChapterWorldBackdropTests
    {
        private const string Prefix = "SHIFT.Tests.ChapterWorldBackdrop.";

        [SetUp]
        public void SetUp() => new SaveService(Prefix).Reset();

        [TearDown]
        public void TearDown() => new SaveService(Prefix).Reset();

        private static LevelProgression Progress(
            int count, int unlocked, int completed)
        {
            var save = new SaveService(Prefix);
            save.Save(new ProgressData(unlocked,
                Mathf.Min(unlocked, count - 1), completed));
            return new LevelProgression(count, save);
        }

        [Test]
        public void ShippedFortyLevelCampaignHasNoWorldRestoration()
        {
            var root = new GameObject("World Graphic Test", typeof(RectTransform));
            try
            {
                var graphic = root.AddComponent<ChapterWorldBackdrop>();
                graphic.Show(Progress(40, 39, 39), 2, false);
                Assert.That(graphic.VisibleMilestones, Is.Zero);
                Assert.That(graphic.ChapterStart, Is.Zero);
                Assert.That(graphic.FinaleRestored, Is.False);
                Assert.That(graphic.raycastTarget, Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void UnlockingChapterFiveCannotRestoreAnyWorldState()
        {
            var root = new GameObject("World Graphic Test", typeof(RectTransform));
            try
            {
                var graphic = root.AddComponent<ChapterWorldBackdrop>();
                graphic.Show(Progress(100, 99, 79), 4, false);
                Assert.That(graphic.ChapterStart, Is.EqualTo(81));
                Assert.That(graphic.VisibleMilestones, Is.Zero);
                Assert.That(graphic.FinaleRestored, Is.False);
                Assert.That(graphic.raycastTarget, Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void RealSavedCompletionRestoresOnlyCompletedBeacons()
        {
            var root = new GameObject("World Graphic Test", typeof(RectTransform));
            try
            {
                var graphic = root.AddComponent<ChapterWorldBackdrop>();
                graphic.Show(Progress(100, 99, 83), 4, false);
                Assert.That(graphic.VisibleMilestones, Is.EqualTo(1));
                graphic.Show(Progress(100, 99, 84), 4, false);
                Assert.That(graphic.VisibleMilestones, Is.EqualTo(2));
                graphic.Show(Progress(100, 99, 94), 4, false);
                Assert.That(graphic.VisibleMilestones, Is.EqualTo(4));
                graphic.Show(Progress(100, 99, 98), 4, false);
                Assert.That(graphic.VisibleMilestones, Is.EqualTo(4));
                Assert.That(graphic.FinaleRestored, Is.False);
                graphic.Show(Progress(100, 99, 99), 4, true);
                Assert.That(graphic.VisibleMilestones, Is.EqualTo(5));
                Assert.That(graphic.FinaleRestored, Is.True);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void SwitchingWorldsAndResettingProgressNeverLeaksRestoration()
        {
            var root = new GameObject("World Graphic Test", typeof(RectTransform));
            try
            {
                var graphic = root.AddComponent<ChapterWorldBackdrop>();
                graphic.Show(Progress(100, 99, 99), 4, true);
                Assert.That(graphic.FinaleRestored, Is.True);
                graphic.Show(Progress(100, 99, 79), 4, true);
                Assert.That(graphic.VisibleMilestones, Is.Zero);
                Assert.That(graphic.FinaleRestored, Is.False);
                graphic.Show(Progress(100, 99, 58), 2, true);
                Assert.That(graphic.ChapterStart, Is.EqualTo(41));
                Assert.That(graphic.VisibleMilestones, Is.EqualTo(4));
                Assert.That(graphic.FinaleRestored, Is.False);
                graphic.Hide();
                Assert.That(graphic.VisibleMilestones, Is.Zero);
                Assert.That(graphic.FinaleRestored, Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
