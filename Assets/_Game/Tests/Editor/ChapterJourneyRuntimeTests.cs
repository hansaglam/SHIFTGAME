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
        public void MilestoneToastRequiresANewlyRecordedWinNotJustAnUnlock()
        {
            var before = Saved(80, 79, 43, 44);
            Assert.That(ChapterJourneyMilestoneNotice.NewMilestone(
                before, 44, 43), Is.Null);
            var after = Saved(80, 79, 44, 45);
            var beat = ChapterJourneyMilestoneNotice.NewMilestone(after, 44, 43);
            Assert.That(beat, Is.Not.Null);
            Assert.That(beat.level, Is.EqualTo(45));
            Assert.That(ChapterJourneyMilestoneNotice.NewMilestone(
                after, 44, 44), Is.Null, "Replay must not re-award milestone.");
            Assert.That(ChapterJourneyMilestoneNotice.NewMilestone(
                after, 43, 42), Is.Null, "Stale level cannot emit a new cue.");
            Assert.That(ChapterJourneyMilestoneNotice.NewMilestone(
                after, 46, 44), Is.Null, "Opening future level is not a win.");
        }

        [Test]
        public void FinalMilestoneToastCannotBeTriggeredByViewingFinalLevel()
        {
            var third = Saved(80, 79, 58, 59);
            Assert.That(ChapterJourneyMilestoneNotice.NewMilestone(
                third, 59, 58), Is.Null);
            var thirdWon = Saved(80, 79, 59, 60);
            Assert.That(ChapterJourneyMilestoneNotice.NewMilestone(
                thirdWon, 59, 58)?.level, Is.EqualTo(60));
            var fourthWon = Saved(80, 79, 79, 79);
            Assert.That(ChapterJourneyMilestoneNotice.NewMilestone(
                fourthWon, 79, 78)?.level, Is.EqualTo(80));
            Assert.That(ChapterJourneyMilestoneNotice.NewMilestone(
                fourthWon, 79, 79), Is.Null);
        }

        [Test]
        public void ReturningFortyLevelCompleterCanEnterLevelFortyOneWithoutReplay()
        {
            var original = Saved(40, 39, 39, 39);
            Assert.That(original.HighestUnlocked, Is.EqualTo(39));
            Assert.That(original.HighestCompleted, Is.EqualTo(39));

            var extended = new LevelProgression(100, new SaveService(Prefix));
            Assert.That(extended.HighestUnlocked, Is.EqualTo(40));
            Assert.That(extended.Current, Is.EqualTo(40));
            Assert.That(extended.HighestCompleted, Is.EqualTo(39));
            Assert.That(extended.IsUnlocked(40), Is.True);
            Assert.That(extended.Recommended, Is.EqualTo(40));

            var reopened = new LevelProgression(100, new SaveService(Prefix));
            Assert.That(reopened.HighestUnlocked, Is.EqualTo(40));
            Assert.That(reopened.Current, Is.EqualTo(40));
            Assert.That(reopened.HighestCompleted, Is.EqualTo(39));
            Assert.That(ChapterJourneyCatalog.Completed(
                ChapterJourneyCatalog.ForChapter(2),
                reopened.HighestCompleted), Is.Zero);
        }

        [Test]
        public void FortyLevelMigrationPreservesReplayChoiceAndIncompletePlayers()
        {
            var save = new SaveService(Prefix);
            save.Save(new ProgressData(39, 11, 39));
            var replaying = new LevelProgression(100, save);
            Assert.That(replaying.Current, Is.EqualTo(11));
            Assert.That(replaying.HighestUnlocked, Is.EqualTo(40));
            Assert.That(replaying.HighestCompleted, Is.EqualTo(39));

            save.Save(new ProgressData(39, 39, 38));
            var unfinished = new LevelProgression(100, save);
            Assert.That(unfinished.HighestUnlocked, Is.EqualTo(39));
            Assert.That(unfinished.Current, Is.EqualTo(39));
            Assert.That(unfinished.HighestCompleted, Is.EqualTo(38));
        }

        [Test]
        public void HundredLevelPreviewUnlocksButDoesNotRestoreChapterFive()
        {
            var preview = Saved(100, 99, 79, 80);
            var fifth = ChapterJourneyCatalog.ForChapter(4);
            Assert.That(fifth, Is.Not.Null);
            Assert.That(fifth.startLevel, Is.EqualTo(81));
            Assert.That(fifth.endLevel, Is.EqualTo(100));
            Assert.That(ChapterJourneyCatalog.Available(preview, 4), Is.True);
            Assert.That(ChapterJourneyCatalog.Completed(fifth,
                preview.HighestCompleted), Is.EqualTo(0));
            Assert.That(ChapterJourneyCatalog.LastRestoredBeat(fifth,
                preview.HighestCompleted), Is.EqualTo(-1));
            Assert.That(ChapterJourneyCatalog.Restored(fifth.beats[0],
                preview.HighestCompleted), Is.False);
            Assert.That(ChapterJourneyCatalog.Restored(fifth.beats[4],
                preview.HighestCompleted), Is.False);
        }

        [Test]
        public void LevelHundredVictoryIsRequiredForTheFinalHorizon()
        {
            var fifth = ChapterJourneyCatalog.ForChapter(4);
            var before = Saved(100, 99, 98, 99);
            Assert.That(ChapterJourneyCatalog.Completed(fifth,
                before.HighestCompleted), Is.EqualTo(19));
            Assert.That(ChapterJourneyCatalog.Restored(fifth.beats[4],
                before.HighestCompleted), Is.False);
            Assert.That(ChapterJourneyMilestoneNotice.NewMilestone(
                before, 99, 98), Is.Null);

            var won = Saved(100, 99, 99, 99);
            Assert.That(ChapterJourneyCatalog.Completed(fifth,
                won.HighestCompleted), Is.EqualTo(20));
            Assert.That(ChapterJourneyCatalog.Progress01(fifth,
                won.HighestCompleted), Is.EqualTo(1f));
            Assert.That(ChapterJourneyCatalog.Restored(fifth.beats[4],
                won.HighestCompleted), Is.True);
            Assert.That(ChapterJourneyMilestoneNotice.NewMilestone(
                won, 99, 98)?.level, Is.EqualTo(100));
            Assert.That(ChapterJourneyMilestoneNotice.NewMilestone(
                won, 99, 99), Is.Null);
            Assert.That(ChapterJourneyMilestoneNotice.IsFinalConvergence(
                ChapterJourneyMilestoneNotice.NewMilestone(
                    won, 99, 98)), Is.True,
                "The Level 100 convergence is a one-time win cue.");
            Assert.That(ChapterJourneyMilestoneNotice.IsFinalConvergence(
                ChapterJourneyMilestoneNotice.NewMilestone(
                    won, 99, 99)), Is.False,
                "Replay must not emit a second finale.");
            Assert.That(ChapterJourneyMilestoneNotice.IsFinalConvergence(
                ChapterJourneyCatalog.ForChapter(3).beats[4]), Is.False,
                "Level 80 keeps its own quieter finale experience.");
        }

        [Test]
        public void LevelEightyCampaignDoesNotExposeFifthWorld()
        {
            var old = Saved(80, 79, 79, 79);
            Assert.That(ChapterJourneyCatalog.Available(old, 4), Is.False);
            Assert.That(ChapterJourneyCatalog.ForChapter(4).startLevel, Is.EqualTo(81));
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
