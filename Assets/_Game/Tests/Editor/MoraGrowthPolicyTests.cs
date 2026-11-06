using NUnit.Framework;

namespace Shift.Game.Tests
{
    public sealed class MoraGrowthPolicyTests
    {
        [TestCase(-1, 40, 0, 1)]
        [TestCase(0, 40, 0, 1)]
        [TestCase(39, 40, 0, 1)]
        [TestCase(40, 41, 1, 1)]
        [TestCase(48, 60, 9, 1)]
        [TestCase(49, 60, 10, 2)]
        [TestCase(68, 90, 29, 2)]
        [TestCase(69, 90, 30, 3)]
        [TestCase(98, 110, 59, 3)]
        [TestCase(99, 110, 60, 4)]
        [TestCase(149, 200, 110, 4)]
        public void StageOnlyReflectsPostFortyCompletedCampaignLevels(
            int highestCompleted, int levelCount, int earned, int stage)
        {
            var snapshot = MoraGrowthPolicy.FromCampaign(highestCompleted, levelCount);
            Assert.That(snapshot.CompletedBeyondForty, Is.EqualTo(earned));
            Assert.That(snapshot.Stage, Is.EqualTo(stage));
        }

        [Test]
        public void CorruptOverlargeProgressIsClampedToAvailableLevelCount()
        {
            Assert.That(MoraGrowthPolicy.FromCampaign(1000, 40).CompletedBeyondForty, Is.Zero);
            Assert.That(MoraGrowthPolicy.FromCampaign(1000, 41).CompletedBeyondForty, Is.EqualTo(1));
            Assert.That(MoraGrowthPolicy.FromCampaign(-3, 0).CompletedBeyondForty, Is.Zero);
        }

        [Test]
        public void MilestonesAndProgressStayValid()
        {
            var first = MoraGrowthPolicy.FromCampaign(39, 40);
            Assert.That(first.HasPostFortyLevels, Is.False);
            Assert.That(first.NextMilestoneLevel, Is.EqualTo(50));
            Assert.That(first.Progress01, Is.EqualTo(0f));

            var middle = MoraGrowthPolicy.FromCampaign(44, 50);
            Assert.That(middle.Progress01, Is.EqualTo(.5f).Within(.0001f));

            var final = MoraGrowthPolicy.FromCampaign(99, 120);
            Assert.That(final.IsMaxStage, Is.True);
            Assert.That(final.Progress01, Is.EqualTo(1f));
            Assert.That(final.NextMilestoneLevel, Is.EqualTo(0));
        }

        [Test]
        public void EditorPreviewStagesMapToMilestonesWithoutAStorageDependency()
        {
            for (int stage = 1; stage <= 4; stage++)
                Assert.That(MoraGrowthPolicy.PreviewStage(stage).Stage, Is.EqualTo(stage));
        }

        [Test]
        public void GrowthNamesAreAvailableInBothLaunchLanguages()
        {
            Assert.That(MoraGrowthPolicy.StageName(1, true), Is.EqualTo("Filiz"));
            Assert.That(MoraGrowthPolicy.StageName(4, false), Is.EqualTo("Radiance"));
        }
    }
}
