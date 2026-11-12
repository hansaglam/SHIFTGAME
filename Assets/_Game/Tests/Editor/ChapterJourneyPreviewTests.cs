using NUnit.Framework;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    // Data-only localization/progress checks. No runtime campaign unlocked.
    public sealed class ChapterJourneyPreviewTests
    {
        [TestCase(40, false)]
        [TestCase(41, true)]
        [TestCase(60, true)]
        [TestCase(61, true)]
        [TestCase(80, true)]
        [TestCase(81, true)]
        [TestCase(100, true)]
        [TestCase(101, false)]
        public void JourneyPrototypeOnlyCoversDraftLevels(int level, bool exists)
        {
            Assert.That(ChapterJourneyPrototype.ForLevel(level) != null, Is.EqualTo(exists));
        }

        [Test]
        public void FinaleMustNotCountAsCompletedBeforeWinning()
        {
            Assert.That(ChapterJourneyPrototype.PreviewProgress(41, false), Is.EqualTo(0f));
            Assert.That(ChapterJourneyPrototype.PreviewProgress(60, false),
                Is.EqualTo(19f / 20f).Within(.001f));
            Assert.That(ChapterJourneyPrototype.PreviewProgress(60, true), Is.EqualTo(1f));
            Assert.That(ChapterJourneyPrototype.PreviewProgress(61, false), Is.EqualTo(0f));
            Assert.That(ChapterJourneyPrototype.PreviewProgress(80, false),
                Is.LessThan(1f));
            Assert.That(ChapterJourneyPrototype.PreviewProgress(80, true), Is.EqualTo(1f));
            Assert.That(ChapterJourneyPrototype.PreviewProgress(81, false), Is.EqualTo(0f));
            Assert.That(ChapterJourneyPrototype.PreviewProgress(100, false),
                Is.EqualTo(19f / 20f).Within(.001f));
            Assert.That(ChapterJourneyPrototype.PreviewProgress(100, true), Is.EqualTo(1f));
            Assert.That(ChapterJourneyPrototype.IsBeatRestored(
                ChapterJourneyPrototype.Chapters[0],
                ChapterJourneyPrototype.Chapters[0].beats[4], 60, false), Is.False);
            Assert.That(ChapterJourneyPrototype.IsBeatRestored(
                ChapterJourneyPrototype.Chapters[0],
                ChapterJourneyPrototype.Chapters[0].beats[4], 60, true), Is.True);
            Assert.That(ChapterJourneyPrototype.IsBeatRestored(
                ChapterJourneyPrototype.Chapters[1],
                ChapterJourneyPrototype.Chapters[1].beats[4], 80, false), Is.False);
            Assert.That(ChapterJourneyPrototype.IsBeatRestored(
                ChapterJourneyPrototype.Chapters[1],
                ChapterJourneyPrototype.Chapters[1].beats[4], 80, true), Is.True);
            Assert.That(ChapterJourneyPrototype.IsBeatRestored(
                ChapterJourneyPrototype.Chapters[2],
                ChapterJourneyPrototype.Chapters[2].beats[4], 100, false), Is.False);
            Assert.That(ChapterJourneyPrototype.IsBeatRestored(
                ChapterJourneyPrototype.Chapters[2],
                ChapterJourneyPrototype.Chapters[2].beats[4], 100, true), Is.True);
        }

        [Test]
        public void EachChapterMustHaveFiveBilingualMilestonesAndAccessibleAlternatives()
        {
            Assert.That(ChapterJourneyPrototype.Chapters.Length, Is.EqualTo(3));
            foreach (var chapter in ChapterJourneyPrototype.Chapters)
            {
                Assert.That(chapter.endLevel - chapter.startLevel + 1, Is.EqualTo(20));
                Assert.That(chapter.beats.Length, Is.EqualTo(5));
                Assert.That(chapter.beats[0].level, Is.EqualTo(chapter.startLevel));
                Assert.That(chapter.beats[4].level, Is.EqualTo(chapter.endLevel));
                Assert.That(string.IsNullOrWhiteSpace(chapter.titleTr), Is.False);
                Assert.That(string.IsNullOrWhiteSpace(chapter.titleEn), Is.False);
                Assert.That(string.IsNullOrWhiteSpace(chapter.purposeTr), Is.False);
                Assert.That(string.IsNullOrWhiteSpace(chapter.purposeEn), Is.False);
                Assert.That(string.IsNullOrWhiteSpace(chapter.finaleTr), Is.False);
                Assert.That(string.IsNullOrWhiteSpace(chapter.finaleEn), Is.False);
                int previous = 0;
                foreach (var beat in chapter.beats)
                {
                    Assert.That(beat.level, Is.GreaterThan(previous));
                    previous = beat.level;
                    Assert.That(string.IsNullOrWhiteSpace(beat.nameTr), Is.False);
                    Assert.That(string.IsNullOrWhiteSpace(beat.nameEn), Is.False);
                    Assert.That(string.IsNullOrWhiteSpace(beat.worldTr), Is.False);
                    Assert.That(string.IsNullOrWhiteSpace(beat.worldEn), Is.False);
                    Assert.That(string.IsNullOrWhiteSpace(beat.motionTr), Is.False);
                    Assert.That(string.IsNullOrWhiteSpace(beat.motionEn), Is.False);
                    Assert.That(string.IsNullOrWhiteSpace(beat.stillTr), Is.False);
                    Assert.That(string.IsNullOrWhiteSpace(beat.stillEn), Is.False);
                    Assert.That(ChapterJourneyPrototype.MilestoneIndex(chapter, beat.level),
                        Is.GreaterThanOrEqualTo(0));
                }
            }
        }
    }
}
