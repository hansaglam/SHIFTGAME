using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEngine;

namespace Shift.Game.Tests
{
    public sealed class TelemetryTests
    {
        private double time;
        private LocalAnalyticsService provider;
        private TelemetryTracker tracker;
        private LevelData level;
        [SetUp] public void Setup()
        {
            time = 100; provider = new LocalAnalyticsService(); tracker = new TelemetryTracker(provider, () => time);
            level = LevelValidation.ChapterLevels()[6]; tracker.StartSession(); tracker.StartLevel(level, 6, 3);
        }
        [Test] public void StartIncludesLevelMetadata()
        {
            var a = tracker.Snapshot();
            Assert.That(a.levelNumber, Is.EqualTo(7)); Assert.That(a.title, Is.EqualTo(level.DisplayTitle));
            Assert.That(a.width, Is.EqualTo(4)); Assert.That(a.height, Is.EqualTo(4));
            Assert.That(a.budget, Is.EqualTo(3)); Assert.That(a.targetColor, Is.EqualTo("Red"));
            Assert.That(a.attemptNumber, Is.EqualTo(1)); Assert.That(a.duration, Is.Zero);
            Assert.That(provider.Events.Select(e => e.name), Is.EqualTo(new[] { "session_start", "level_start" }));
        }
        [Test] public void AcceptedTapMetricsSeparateBlockedCancelledAndSuccessful()
        {
            tracker.AcceptedTap(4, 2, false); tracker.AcceptedTap(0, 2, false); tracker.AcceptedTap(0, 2, true);
            var a = tracker.Snapshot(); Assert.That(a.successfulMoves, Is.EqualTo(1)); Assert.That(a.taps, Is.EqualTo(3));
            Assert.That(a.blockedTaps, Is.EqualTo(1)); Assert.That(a.cancelledTaps, Is.EqualTo(1));
            Assert.That(a.blockedRatio, Is.EqualTo(1d/3).Within(.00001)); Assert.That(a.strongChains, Is.EqualTo(1));
            Assert.That(provider.Events.Last(e => e.name == "chain").reactionClass, Is.EqualTo("strong"));
        }
        [Test] public void CompletionHasDeterministicDurationAndOptimalSummaryOnce()
        {
            tracker.AcceptedTap(1, 2, false); tracker.AcceptedTap(2, 1, false); tracker.AcceptedTap(6, 0, false);
            time += 12; tracker.Finish(true); tracker.Finish(true); time += 100;
            var a = tracker.Result; Assert.That(a.duration, Is.EqualTo(12)); Assert.That(a.successfulMoves, Is.EqualTo(3));
            Assert.That(a.totalDepth, Is.EqualTo(9)); Assert.That(a.maxDepth, Is.EqualTo(6)); Assert.That(a.averageDepth, Is.EqualTo(3));
            Assert.That(a.chains, Is.EqualTo(2)); Assert.That(a.perfectShift, Is.True); Assert.That(a.efficiency, Is.EqualTo(1));
            Assert.That(provider.Events.Count(e => e.name == "level_complete"), Is.EqualTo(1));
        }
        [Test] public void UnknownOptimalAndFailureNeverAwardPerfectShift()
        {
            tracker.StartLevel(level, 6, null); tracker.AcceptedTap(1, 2, false); tracker.Finish(true);
            Assert.That(tracker.Result.hasOptimal, Is.False); Assert.That(tracker.Result.perfectShift, Is.False);
            tracker.StartLevel(level, 6, 1); tracker.AcceptedTap(1, 0, false); tracker.Finish(false);
            Assert.That(tracker.Result.perfectShift, Is.False); Assert.That(provider.Events.Last().reason, Is.EqualTo("move_exhaustion"));
        }
        [Test] public void RestartSnapshotsPreviousAttemptAndCarriesVisitCount()
        {
            tracker.AcceptedTap(1, 2, false); time += 8;
            tracker.StartLevel(level, 6, 3, true); var prior = provider.Events.Last(e => e.name == "level_restart").attempt;
            Assert.That(prior.duration, Is.EqualTo(8)); Assert.That(prior.successfulMoves, Is.EqualTo(1));
            Assert.That(prior.restarts, Is.EqualTo(1)); Assert.That(tracker.Snapshot().attemptNumber, Is.EqualTo(2));
            Assert.That(tracker.Snapshot().successfulMoves, Is.Zero); Assert.That(tracker.Snapshot().restarts, Is.EqualTo(1));
            tracker.StartLevel(level, 6, 3, false); Assert.That(tracker.Snapshot().restarts, Is.Zero);
        }
        [Test] public void SessionPauseEndAndNewSessionAreIdempotent()
        {
            tracker.AcceptedTap(4, 2, false); time += 5; tracker.Pause(true); time += 200; tracker.Pause(false); time += 2;
            Assert.That(tracker.Snapshot().duration, Is.EqualTo(7)); tracker.EndSession(); tracker.EndSession(); time += 50;
            Assert.That(tracker.SessionSnapshot().duration, Is.EqualTo(7));
            Assert.That(tracker.SessionSnapshot().successfulMoves, Is.EqualTo(1)); Assert.That(tracker.SessionSnapshot().chains, Is.EqualTo(1));
            Assert.That(provider.Events.Count(e => e.name == "session_end"), Is.EqualTo(1));
            tracker.StartSession(); Assert.That(tracker.SessionSnapshot().successfulMoves, Is.Zero);
            tracker.StartLevel(level, 6, 3); Assert.That(tracker.Snapshot().attemptNumber, Is.EqualTo(1));
        }
        private sealed class BrokenProvider : IAnalyticsService { public void Track(AnalyticsEvent value) => throw new Exception("Provider offline"); }
        [Test] public void ProviderExceptionsAndDisabledProviderCannotBreakMetrics()
        {
            foreach (var service in new IAnalyticsService[] { new BrokenProvider(), new NullAnalyticsService() })
            {
                var t = new TelemetryTracker(service, () => time);
                Assert.DoesNotThrow(() => { t.StartSession(); t.StartLevel(level, 6, 3); t.AcceptedTap(2, 2, false); t.Finish(true); t.EndSession(); });
                Assert.That(t.Result.successfulMoves, Is.EqualTo(1));
                if (service is BrokenProvider) Assert.That(t.ProviderFailures, Is.GreaterThan(0));
            }
        }
        [Test] public void LocalProviderIsBoundedAndSnapshotsDoNotMutate()
        {
            var first = provider.Events.Last();
            for (int i = 0; i < 200; i++) tracker.AcceptedTap(1, 2, false);
            Assert.That(provider.Events.Count(), Is.EqualTo(128)); Assert.That(first.attempt.successfulMoves, Is.Zero);
        }
        [Test] public void OnlySearchVerifiedUnmodifiedPuzzlesExposeOptimalMoves()
        {
            var expected = new Dictionary<int,int> { {0,1}, {1,3}, {2,3}, {3,1}, {4,2}, {5,2}, {6,3}, {7,2}, {8,2}, {9,4}, {10,4}, {11,3}, {12,2}, {13,4}, {14,5}, {15,5}, {16,5}, {17,6}, {18,4}, {19,6} };
            var levels = LevelValidation.ChapterLevels();
            for (int i=0; i<20; i++) Assert.That(levels[i].VerifiedOptimalMoveCount, Is.EqualTo(expected.TryGetValue(i,out int n) ? (int?)n : null), levels[i].name);
            var clone = UnityEngine.Object.Instantiate(level);
            try { clone.Configure(5, level.Height, level.MoveLimit, level.TargetColor, level.Placements); Assert.That(clone.VerifiedOptimalMoveCount, Is.Null); }
            finally { UnityEngine.Object.DestroyImmediate(clone); }
        }
        [Test] public void BoardManagerRemainsAnalyticsFree()
        {
            var source = File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Scripts/Board/BoardManager.cs"));
            Assert.That(source, Does.Not.Contain("Analytics")); Assert.That(source, Does.Not.Contain("Telemetry"));
        }
        [TestCase(true,false,true)] [TestCase(false,true,false)]
        public void SettingsPersistIndependentlyOfProgress(bool sound, bool haptics, bool motion)
        {
            const string prefix = "SHIFT.Tests.Settings."; var settings = new SettingsService(prefix); settings.Reset();
            try
            {
                settings.Save(sound,haptics,motion); var loaded = new SettingsService(prefix);
                Assert.That(loaded.Sound, Is.EqualTo(sound)); Assert.That(loaded.Haptics, Is.EqualTo(haptics)); Assert.That(loaded.ReducedMotion, Is.EqualTo(motion));
                new SaveService(prefix+"Progress.").Reset(); Assert.That(new SettingsService(prefix).Sound, Is.EqualTo(sound));
            }
            finally { settings.Reset(); }
        }
        [Test] public void SettingsDefaultsAndAudioGainMuteAndPoolAreSafe()
        {
            var settings = new SettingsService("SHIFT.Tests.AudioSettings."); settings.Reset(); settings = new SettingsService("SHIFT.Tests.AudioSettings.");
            Assert.That(settings.Sound, Is.True); Assert.That(settings.Haptics, Is.False); Assert.That(settings.ReducedMotion, Is.False);
            var go = new GameObject("Audio settings test");
            try
            {
                var audio = go.AddComponent<AudioManager>(); audio.Initialize(new AudioClips()); audio.Initialize(new AudioClips());
                audio.MasterVolume = .5f; audio.SfxVolume = .4f;
                Assert.That(go.GetComponents<AudioSource>().Length, Is.EqualTo(4));
                Assert.That(audio.MasterVolume, Is.EqualTo(.5f));
                Assert.That(audio.SfxVolume, Is.EqualTo(.4f));
                foreach (var source in go.GetComponents<AudioSource>()) Assert.That(source.volume, Is.Zero, "Unused pooled voices stay silent");
                settings.Save(false,true,true); var feel = new GameFeelSettings(); settings.Apply(feel,audio);
                Assert.That(audio.Muted, Is.True); Assert.That(feel.hapticsEnabled, Is.True); Assert.That(feel.reducedMotion, Is.True);
                foreach (AudioCue cue in Enum.GetValues(typeof(AudioCue))) Assert.DoesNotThrow(() => audio.Play(cue));
            }
            finally { UnityEngine.Object.DestroyImmediate(go); settings.Reset(); }
        }
    }
}
