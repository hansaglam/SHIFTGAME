using NUnit.Framework;
using Shift.Game;

namespace Shift.Game.Tests
{
    // Verifies saved chapter completion, not unlocked levels or preview sliders,
    // is the ONLY source for visual restoration and chapter-finale state.
    public sealed class ChapterJourneyRuntimeTests
    {
        private const string Prefix = "SHIFT.Tests.ChapterJourney.Runtime.";

        [SetUp]
        public void Before() { new SaveService(Prefix).Reset(); }

        [TearDown]
        public void After() { new SaveService(Prefix).Reset(); }

        private static LevelProgression Saved(int count, int highestUnlocked,
            int highestCompleted, int current = 0)
        {
            var save = new SaveService(Prefix);
            save.Save(new ProgressData(highestUnlocked, current, highestCompleted));
            return new LevelProgression(count, save);
        }

        [Test]
        public void OriginalFortyLevelCampaignDoesNotExposeFutureWorlds()
        {
            var legacy = Saved(40, 39, 39, 39);
            Assert.That(ChapterJourneyCatalog.Available(legacy, 2), Is.False);
            Assert.That(ChapterJourneyCatalog.Available(legacy, 3), Is.False);
            Assert.That(ChapterJourneyCatalog.ForChapter(0), Is.Null);
            Assert.That(ChapterJourneyCatalog.ForChapter(1), Is.Null);
            Assert.That(ChapterJourneyCatalog.ForChapter(2).startLevel, Is.EqualTo(41));
            Assert.That(ChapterJourneyCatalog.ForChapter(3).startLevel, Is.EqualTo(61));
        }

        [Test]
        public void UnlockingAllFutureLevelsDoesNotRestoreAnyWorldLines()
        {
            // This is intentionally the isolated preview-save frontier:
            // 1–40 are recorded, 41–80 merely unlocked.
            var preview = Saved(80, 79, 39, 40);
            var third = ChapterJourneyCatalog.ForChapter(2);
            var fourth = ChapterJourneyCatalog.ForChapter(3);
            Assert.That(ChapterJourneyCatalog.Available(preview, 2), Is.True);
            Assert.That(ChapterJourneyCatalog.Available(preview, 3), Is.True);
            Assert.That(ChapterJourneyCatalog.Completed(third, preview.HighestCompleted), Is.Zero);
            Assert.That(ChapterJourneyCatalog.Completed(fourth, preview.HighestCompleted), Is.Zero);
            Assert.That(ChapterJourneyCatalog.LastRestoredBeat(third,
                preview.HighestCompleted), Is.EqualTo(-1));
            Assert.That(ChapterJourneyCatalog.Restored(third.beats[0],
                preview.HighestCompleted), Is.False);
            Assert.That(ChapterJourneyCatalog.Restored(fourth.beats[4],
                preview.HighestCompleted), Is.False);
        }

        [Test]
        public void ChapterThreeFinaleRequiresLevelSixtyActuallyCompleted()
        {
            var before = Saved(80, 79, 58, 59);
            var third = ChapterJourneyCatalog.ForChapter(2);
            Assert.That(ChapterJourneyCatalog.Completed(third,
                before.HighestCompleted), Is.EqualTo(19));
            Assert.That(ChapterJourneyCatalog.Progress01(third,
                before.HighestCompleted), Is.EqualTo(.95f).Within(.001f));
            Assert.That(ChapterJourneyCatalog.Restored(third.beats[4],
                before.HighestCompleted), Is.False);

            var after = Saved(80, 79, 59, 60);
            Assert.That(ChapterJourneyCatalog.Completed(third,
                after.HighestCompleted), Is.EqualTo(20));
            Assert.That(ChapterJourneyCatalog.Progress01(third,
                after.HighestCompleted), Is.EqualTo(1f));
            Assert.That(ChapterJourneyCatalog.Restored(third.beats[4],
                after.HighestCompleted), Is.True);
            Assert.That(ChapterJourneyCatalog.Completed(
                ChapterJourneyCatalog.ForChapter(3), after.HighestCompleted), Is.Zero);
        }

        [Test]
        public void ChapterFourFinaleRequiresLevelEightyActuallyCompleted()
        {
            var fourth = ChapterJourneyCatalog.ForChapter(3);
            var before = Saved(80, 79, 78, 79);
            Assert.That(ChapterJourneyCatalog.Completed(fourth,
                before.HighestCompleted), Is.EqualTo(19));
            Assert.That(ChapterJourneyCatalog.Restored(fourth.beats[4],
                before.HighestCompleted), Is.False);
            var after = Saved(80, 79, 79, 79);
            Assert.That(ChapterJourneyCatalog.Completed(fourth,
                after.HighestCompleted), Is.EqualTo(20));
            Assert.That(ChapterJourneyCatalog.LastRestoredBeat(fourth,
                after.HighestCompleted), Is.EqualTo(4));
        }

        [Test]
        public void PartialChapterMustNotDisplayACompleteWorld()
        {
            var progress = Saved(60, 59, 39, 40);
            Assert.That(ChapterJourneyCatalog.Available(progress, 2), Is.True);
            Assert.That(ChapterJourneyCatalog.Available(progress, 3), Is.False);
        }

        [Test]
        public void ReturningPlayerShowsIdenticalProgressWithoutNewSaveKeys()
        {
            var save = new SaveService(Prefix);
            save.Save(new ProgressData(79, 64, 63));
            var first = new LevelProgression(80, save);
            var restored = new LevelProgression(80, save);
            var third = ChapterJourneyCatalog.ForChapter(2);
            var fourth = ChapterJourneyCatalog.ForChapter(3);
            Assert.That(ChapterJourneyCatalog.Completed(third,
                restored.HighestCompleted), Is.EqualTo(20));
            Assert.That(ChapterJourneyCatalog.Completed(fourth,
                restored.HighestCompleted), Is.EqualTo(4));
            Assert.That(ChapterJourneyCatalog.Completed(fourth,
                restored.HighestCompleted), Is.EqualTo(
                    ChapterJourneyCatalog.Completed(fourth, first.HighestCompleted)));
            Assert.That(ChapterJourneyCatalog.Restored(fourth.beats[1],
                restored.HighestCompleted), Is.False);
        }
    }
}
