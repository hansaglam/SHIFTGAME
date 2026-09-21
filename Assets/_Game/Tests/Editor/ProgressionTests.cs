using NUnit.Framework;
using UnityEngine;

namespace Shift.Game.Tests
{
    public sealed class ProgressionTests
    {
        private const string Prefix = "SHIFT.Tests.Sprint2.Unit.";
        private SaveService saves;
        [SetUp] public void Setup() { saves = new SaveService(Prefix); saves.Reset(); }
        [TearDown] public void Cleanup() => saves.Reset();
        [Test] public void FirstInstallStartsAtOneAndLockedSelectionIsRejected()
        {
            var p = new LevelProgression(20,saves);
            Assert.That(p.Current,Is.Zero); Assert.That(p.HighestUnlocked,Is.Zero);
            Assert.That(p.HighestCompleted,Is.EqualTo(-1)); Assert.That(p.ChapterComplete,Is.False);
            Assert.That(p.Select(1),Is.False); Assert.That(p.Select(-1),Is.False); Assert.That(p.Next(),Is.False);
        }
        [Test] public void CompletionUnlocksNextAndRelaunchResumesIt()
        {
            var p = new LevelProgression(20,saves); Assert.That(p.Complete(0),Is.True);
            Assert.That(p.HighestUnlocked,Is.EqualTo(1)); Assert.That(p.Current,Is.Zero);
            Assert.That(new LevelProgression(20,saves).Current,Is.EqualTo(1));
            Assert.That(p.Next(),Is.True); Assert.That(p.Current,Is.EqualTo(1));
        }
        [Test] public void ReplayNeverReducesUnlocksAndPersistsSelection()
        {
            var p = new LevelProgression(20,saves);
            for(int i=0;i<5;i++) { p.Complete(i); p.Next(); }
            p.Select(0); p.Complete(0); p.Select(0);
            var loaded = new LevelProgression(20,saves);
            Assert.That(loaded.HighestUnlocked,Is.EqualTo(5)); Assert.That(loaded.HighestCompleted,Is.EqualTo(4));
            Assert.That(loaded.Current,Is.Zero); Assert.That(loaded.Recommended,Is.EqualTo(5));
        }
        [TestCase(-100,-20,-9,0,0,-1)]
        [TestCase(999,999,999,19,19,19)]
        [TestCase(4,15,2,4,4,2)]
        public void CorruptIndexesClamp(int unlocked,int current,int completed,int expectedUnlocked,int expectedCurrent,int expectedCompleted)
        {
            PlayerPrefs.SetInt(Prefix+"Unlocked",unlocked); PlayerPrefs.SetInt(Prefix+"Current",current); PlayerPrefs.SetInt(Prefix+"Completed",completed);
            var p = new LevelProgression(20,saves);
            Assert.That(p.HighestUnlocked,Is.EqualTo(expectedUnlocked)); Assert.That(p.Current,Is.EqualTo(expectedCurrent));
            Assert.That(p.HighestCompleted,Is.EqualTo(expectedCompleted));
        }
        [Test] public void FinalLevelCompletesChapterWithoutOverflow()
        {
            var p = new LevelProgression(20,saves);
            for(int i=0;i<20;i++) { Assert.That(p.Complete(i),Is.True); if(i<19) Assert.That(p.Next(),Is.True); }
            Assert.That(p.ChapterComplete,Is.True); Assert.That(p.Next(),Is.False);
            Assert.That(p.HighestUnlocked,Is.EqualTo(19)); Assert.That(new LevelProgression(20,saves).ChapterComplete,Is.True);
        }
        [Test] public void ResetOnlyDeletesOwnedKeys()
        {
            PlayerPrefs.SetInt(Prefix+"Unrelated",42);
            var p = new LevelProgression(20,saves); p.UnlockAll(); p.Select(19); p.Complete(19); p.Reset();
            Assert.That(p.Current,Is.Zero); Assert.That(p.HighestUnlocked,Is.Zero); Assert.That(p.HighestCompleted,Is.EqualTo(-1));
            Assert.That(saves.Load(20).Current,Is.Zero); Assert.That(PlayerPrefs.GetInt(Prefix+"Unrelated"),Is.EqualTo(42));
            PlayerPrefs.DeleteKey(Prefix+"Unrelated");
        }
        [Test] public void CannotCompleteAnotherOrLockedLevel()
        {
            var p = new LevelProgression(20,saves); Assert.That(p.Complete(19),Is.False);
            Assert.That(p.HighestUnlocked,Is.Zero);
        }
    }
}
