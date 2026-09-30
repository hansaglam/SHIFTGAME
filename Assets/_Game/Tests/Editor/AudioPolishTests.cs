using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Shift.Game.Tests
{
    public sealed class AudioPolishTests
    {
        private const string Prefix = "SHIFT.Tests.AudioPolish.";
        private sealed class Output : IHapticOutput
        {
            public readonly List<HapticCue> Pulses = new List<HapticCue>();
            public Func<bool> Valid;
            public void Pulse(HapticCue cue, Func<bool> valid) { Pulses.Add(cue); Valid = valid; }
        }
        [TestCase(true,false,false,false,false,AudioCue.Win)]
        [TestCase(true,true,false,false,false,AudioCue.Perfect)]
        [TestCase(true,true,true,false,false,AudioCue.FirstPerfect)]
        [TestCase(true,false,false,true,false,AudioCue.DailyComplete)]
        [TestCase(true,true,true,true,false,AudioCue.DailyPerfect)]
        [TestCase(true,true,true,false,true,AudioCue.ChapterMastered)]
        [TestCase(false,false,false,false,false,AudioCue.Lose)]
        public void OutcomePriorityChoosesOneRestrainedAccent(bool won,bool perfect,bool first,bool daily,bool mastered,AudioCue expected)
        { Assert.That(AudioManager.Outcome(won,perfect,first,daily,mastered),Is.EqualTo(expected)); }

        [Test] public void PitchAndGainAreBoundedDeterministicAndQuiet()
        {
            for(int step=-2;step<10000;step++)
            {
                float pitch=AudioManager.ReactionPitch(step);
                Assert.That(pitch,Is.InRange(1f,1.21f)); Assert.That(pitch,Is.EqualTo(AudioManager.ReactionPitch(step)));
            }
            Assert.That(AudioManager.ReactionPitch(4),Is.GreaterThan(AudioManager.ReactionPitch(1)));
            foreach(AudioCue cue in Enum.GetValues(typeof(AudioCue)))Assert.That(AudioManager.CueGain(cue),Is.InRange(0f,1f));
            Assert.That(AudioManager.CueGain(AudioCue.Blocked),Is.LessThan(AudioManager.CueGain(AudioCue.Tap)));
            Assert.That(AudioManager.CueGain(AudioCue.UIButton),Is.LessThan(AudioManager.CueGain(AudioCue.Move)));
        }
        [TestCase(4,AudioCue.BigShift)] [TestCase(6,AudioCue.MegaShift)] [TestCase(100,AudioCue.MegaShift)]
        public void ChainPayoffUsesExistingVisualTier(int depth,AudioCue expected)
        {
            var go=new GameObject("Audio fixture");
            try { var audio=go.AddComponent<AudioManager>();audio.Initialize(new AudioClips());var cues=new List<AudioCue>();audio.CuePlayed+=cues.Add;audio.ChainPayoff(depth);Assert.That(cues,Is.EqualTo(new[]{expected})); }
            finally { Object.DestroyImmediate(go); }
        }
        [Test] public void LongChainHasAtMostThreeHapticsAndOnePerMilestone()
        {
            float time=0;var output=new Output();var feel=new GameFeelSettings{hapticsEnabled=true};var service=new HapticService(feel,output,()=>time);
            service.BeginReaction(false,100);
            for(int i=1;i<=100;i++){time+=.3f;service.Interaction();service.Step(i);}
            time+=.3f;service.Outcome(AudioCue.Perfect);
            Assert.That(output.Pulses,Is.EqualTo(new[]{HapticCue.Light,HapticCue.Medium,HapticCue.Strong}));
        }
        [Test] public void DisabledHapticsReducedMotionAndCooldownAreIndependent()
        {
            float time=0;var output=new Output();var feel=new GameFeelSettings{reducedMotion=true};var h=new HapticService(feel,output,()=>time);
            h.Light();Assert.That(output.Pulses,Is.Empty);feel.hapticsEnabled=true;h.Light();h.Medium();
            Assert.That(output.Pulses.Count,Is.EqualTo(1));time=1;h.Medium();Assert.That(output.Pulses.Last(),Is.EqualTo(HapticCue.Medium));
            feel.hapticsEnabled=false;Assert.That(output.Valid(),Is.False);time=2;h.Medium();Assert.That(output.Pulses.Count,Is.EqualTo(2));
        }
        [Test] public void PauseFocusAndRestartInvalidateQueuedHaptics()
        {
            float time=0;var o=new Output();var h=new HapticService(new GameFeelSettings{hapticsEnabled=true},o,()=>time);
            h.Light();var stale=o.Valid;h.Pause(true);Assert.That(stale(),Is.False);time=1;h.Medium();Assert.That(o.Pulses.Count,Is.EqualTo(1));
            h.Pause(false);h.Medium();stale=o.Valid;h.Focus(false);h.Pause(false);time=2;h.Light();Assert.That(o.Pulses.Count,Is.EqualTo(2));
            Assert.That(stale(),Is.False);h.Focus(true);h.Light();stale=o.Valid;h.Cancel();Assert.That(stale(),Is.False);
        }
        private sealed class Unsupported : IHapticOutput { public void Pulse(HapticCue cue,Func<bool> valid) => throw new NotSupportedException(); }
        [Test] public void UnsupportedHapticsFailSilently()
        { var h=new HapticService(new GameFeelSettings{hapticsEnabled=true},new Unsupported(),()=>0);Assert.DoesNotThrow(h.Medium); }
        [Test] public void ProductionAssetsAndSceneMappingAreComplete()
        {
            var bank=new AudioClips();foreach(AudioCue cue in Enum.GetValues(typeof(AudioCue)))Assert.That(bank.Get(cue),Is.Null);
            Assert.That(Directory.GetFiles(Path.Combine(Application.dataPath,"_Game/Audio/SFX"),"*.wav"),Has.Length.EqualTo(17));
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);
            var serialized = new UnityEditor.SerializedObject(Object.FindFirstObjectByType<PrototypeGame>());
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.tap").objectReferenceValue).name,Is.EqualTo("shift_tap_move"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.move").objectReferenceValue).name,Is.EqualTo("shift_tap_move"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.push").objectReferenceValue).name,Is.EqualTo("shift_push"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.directionChange").objectReferenceValue).name,Is.EqualTo("shift_direction"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.exit").objectReferenceValue).name,Is.EqualTo("shift_exit"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.blocked").objectReferenceValue).name,Is.EqualTo("shift_blocked"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.win").objectReferenceValue).name,Is.EqualTo("shift_win"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.rotate").objectReferenceValue).name,Is.EqualTo("shift_rotator"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.switchActivate").objectReferenceValue).name,Is.EqualTo("shift_switch"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.gateOpen").objectReferenceValue).name,Is.EqualTo("shift_gate_open"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.gateClose").objectReferenceValue).name,Is.EqualTo("shift_gate_close"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.finalExit").objectReferenceValue).name,Is.EqualTo("shift_exit"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.chainStep").objectReferenceValue).name,Is.EqualTo("shift_chain_pulse"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.chainEscalation").objectReferenceValue).name,Is.EqualTo("shift_chain_pulse"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.bigShift").objectReferenceValue).name,Is.EqualTo("shift_big_shift"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.megaShift").objectReferenceValue).name,Is.EqualTo("shift_mega_shift"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.undo").objectReferenceValue).name,Is.EqualTo("shift_undo"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.hint").objectReferenceValue).name,Is.EqualTo("shift_hint"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.perfect").objectReferenceValue).name,Is.EqualTo("shift_perfect"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.firstPerfect").objectReferenceValue).name,Is.EqualTo("shift_perfect"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.dailyComplete").objectReferenceValue).name,Is.EqualTo("shift_win"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.dailyPerfect").objectReferenceValue).name,Is.EqualTo("shift_perfect"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.chapterMastered").objectReferenceValue).name,Is.EqualTo("shift_perfect"));
            Assert.That(((AudioClip)serialized.FindProperty("audioClips.campaignComplete").objectReferenceValue).name,Is.EqualTo("shift_campaign_complete"));
            foreach (var path in Directory.GetFiles(Path.Combine(Application.dataPath,"_Game/Audio/SFX"),"*.wav"))
            {
                var importer=(UnityEditor.AudioImporter)UnityEditor.AssetImporter.GetAtPath("Assets/_Game/Audio/SFX/"+Path.GetFileName(path));
                Assert.That(importer.defaultSampleSettings.loadType,Is.EqualTo(AudioClipLoadType.DecompressOnLoad));
                Assert.That(importer.defaultSampleSettings.compressionFormat,Is.EqualTo(AudioCompressionFormat.PCM));
                Assert.That(importer.loadInBackground,Is.False);Assert.That(importer.ambisonic,Is.False);
            }
        }
        [Test] public void SceneOnlyChangesAudioReferences()
        {
            string text=File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Scenes/Prototype.unity"));
            int start=text.IndexOf("  audioClips:",StringComparison.Ordinal);
            int end=text.IndexOf("  analyticsEnabled:",start,StringComparison.Ordinal);
            string bank=System.Text.RegularExpressions.Regex.Replace(text.Substring(start,end-start),@"\{fileID: 8300000, guid: [0-9a-f]+, type: 3\}","{fileID: 0}");
            bank=bank.Replace("    campaignComplete: {fileID: 0}\n","");
            text=text.Substring(0,start)+bank+text.Substring(end);
            foreach(string field in new[]{"finalExit","chainStep","chainEscalation","bigShift","megaShift","undo","hint","restart","perfect","firstPerfect","dailyComplete","dailyPerfect","chapterMastered"})
                text=text.Replace("    "+field+": {fileID: 0}\n","");
            using var sha=System.Security.Cryptography.SHA256.Create();
            // Original Unity scene has LF line endings and no BOM.
            Assert.That(BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text))).Replace("-",""),Is.EqualTo("CB411AE21A0A0C4F6C42F5DA5DFA3387B141673D666B97C95BC45352265B56A3"));
        }
        [Test] public void AllProtectedProjectFilesRemainByteIdentical()
        {
            using var sha=System.Security.Cryptography.SHA256.Create();
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/AudioProtectedFiles.txt")))
            {var p=line.Split('|');Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,"..",p[0])))).Replace("-",""),Is.EqualTo(p[1]),p[0]);}
        }
        // Validation-only procedural signal: never assigned to a scene, bank asset or production Resources.
        private static AudioClip Tone()
        {
            var clip=AudioClip.Create("VALIDATION ONLY - not production audio",4410,1,44100,false);var samples=new float[4410];
            for(int i=0;i<samples.Length;i++){float t=(float)i/44100;samples[i]=.25f*Mathf.Sin(2*Mathf.PI*420*t)*Mathf.Sin(Mathf.PI*i/samples.Length);}
            clip.SetData(samples,0);return clip;
        }
        [UnityTest] public IEnumerator FourVoiceMixMuteLifecycleAndOutcomeOnce()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return null;
            var go=new GameObject("Audio validation fixture");var audio=go.AddComponent<AudioManager>();var clip=Tone();var bank=new AudioClips{volume=1,sfxVolume=1};
            foreach(var f in typeof(AudioClips).GetFields().Where(f=>f.FieldType==typeof(AudioClip)))f.SetValue(bank,clip);
            audio.Initialize(bank);var cues=new List<AudioCue>();audio.CuePlayed+=cues.Add;
            var h=new HapticService(new GameFeelSettings{hapticsEnabled=true},new Output());
            foreach(AudioCue cue in Enum.GetValues(typeof(AudioCue)))audio.Play(cue);
            Assert.That(go.GetComponents<AudioSource>().Length,Is.EqualTo(4));
            Assert.That(go.GetComponents<AudioSource>().Sum(s=>s.volume),Is.LessThanOrEqualTo(.8f));
            int count=audio.StartedVoices;audio.Muted=true;audio.Play(AudioCue.Move);Assert.That(audio.StartedVoices,Is.EqualTo(count));
            foreach(var source in go.GetComponents<AudioSource>())Assert.That(source.isPlaying,Is.False);
            audio.Muted=false;audio.BeginLevel();cues.Clear();audio.PlayOutcome(AudioCue.Perfect,h);audio.PlayOutcome(AudioCue.Perfect,h);
            yield return new WaitForSecondsRealtime(.12f);Assert.That(cues.Count(c=>c==AudioCue.Perfect),Is.EqualTo(1));
            audio.BeginLevel();cues.Clear();audio.PlayOutcome(AudioCue.Perfect,h);audio.Pause(true);audio.Focus(false);audio.Pause(false);
            audio.Play(AudioCue.Move);yield return new WaitForSecondsRealtime(.12f);Assert.That(cues,Is.Empty);
            audio.Focus(true);audio.PlayOutcome(AudioCue.Perfect,h);yield return new WaitForSecondsRealtime(.12f);Assert.That(cues,Is.Empty,"No stale replay on ad resume");
            audio.BeginLevel();audio.PlayOutcome(AudioCue.Win,h);audio.BeginLevel();yield return new WaitForSecondsRealtime(.12f);Assert.That(cues,Is.Empty,"Restart cancels scheduled outcome");
            audio.BeginLevel();audio.PlayOutcome(AudioCue.Lose,h);yield return new WaitForSecondsRealtime(.12f);
            audio.RearmOutcome();audio.PlayOutcome(AudioCue.Win,h);yield return new WaitForSecondsRealtime(.12f);
            Assert.That(cues.Count(c=>c==AudioCue.Lose),Is.EqualTo(1));Assert.That(cues.Count(c=>c==AudioCue.Win),Is.EqualTo(1));
            Object.Destroy(go);Object.Destroy(clip);yield return new ExitPlayMode();
        }
        private sealed class RewardAd : IRewardedAdService
        {
            public bool IsRewardedAdAvailable => true;
            public Action<bool> Done;
            public void ShowRewardedAd(RewardReason reason,Action<bool> done) { Done=done; }
        }
        [UnityTest] public IEnumerator EarnedUtilityRewardsDoNotImpersonateUndoOrDuplicateFeedback()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return Ready();
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.UnlockAllLevels();game.SelectLevel(9);yield return Ready();
            var audio=game.GetComponent<AudioManager>();var cues=new List<AudioCue>();audio.CuePlayed+=cues.Add;var ads=new RewardAd();game.RewardedAds=ads;
            for(int i=0;i<3;i++)game.Allowances.TryConsumeHint();
            game.OpenHint();game.RequestRewardedHint();game.SendMessage("OnApplicationPause",true);game.SendMessage("OnApplicationPause",false);
            ads.Done(true);ads.Done(true);Assert.That(cues.Count(c=>c==AudioCue.Hint),Is.EqualTo(1));Assert.That(cues,Has.No.Member(AudioCue.Win));
            for(int i=0;i<5;i++)game.Allowances.TryUndo(()=>true);
            Tap(game.CurrentLevel.KnownSolution[0]);yield return Settled(game);cues.Clear();Assert.That(game.Undo(),Is.False);game.RequestRewardedHint();ads.Done(true);ads.Done(true);
            Assert.That(cues,Has.No.Member(AudioCue.Undo));Assert.That(game.Undo(),Is.True);Assert.That(cues.Count(c=>c==AudioCue.Undo),Is.EqualTo(1));
            Assert.That(game.Undo(),Is.False);Assert.That(cues.Count(c=>c==AudioCue.Undo),Is.EqualTo(1));
            game.Restart();yield return Ready();Assert.That(game.GetComponents<AudioSource>().Length,Is.EqualTo(4));
            UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene,UnityEngine.SceneManagement.LoadSceneMode> loaded=(_,__)=>
            {
                var reloaded=Object.FindFirstObjectByType<PrototypeGame>();
                typeof(PrototypeGame).GetField("saveKeyPrefix",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(reloaded,Prefix);
                typeof(PrototypeGame).GetField("settingsKeyPrefix",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(reloaded,Prefix+"Settings.");
            };
            UnityEngine.SceneManagement.SceneManager.sceneLoaded+=loaded;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Prototype");yield return Ready();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded-=loaded;
            Assert.That(Object.FindObjectsByType<AudioManager>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length,Is.EqualTo(4));
            yield return new ExitPlayMode();
        }
        private static HapticService Haptics(PrototypeGame game) => (HapticService)typeof(PrototypeGame).GetField("haptics",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(game);
        private static void Tap(GridPosition p) => Object.FindObjectsByType<Piece>(FindObjectsSortMode.None).Single(x=>x.Data.Active&&x.Data.Type==PieceType.Normal&&x.Data.Position==p).GetComponentInChildren<Button>().onClick.Invoke();
        private static IEnumerator Settled(PrototypeGame game)
        {float end=Time.realtimeSinceStartup+15;while(game.Board.State==GameState.Resolving&&Time.realtimeSinceStartup<end)yield return null;Assert.That(game.Board.State,Is.Not.EqualTo(GameState.Resolving));yield return new WaitForSecondsRealtime(.11f);}
        private static IEnumerator Ready(){yield return new WaitForSecondsRealtime(.35f);}
        private static void Capture(string name){Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/audio"));SprintPresentationTests.Capture("audio/"+name+".png");}
        private static AttemptMetrics Certified(LevelData level,int index)
        {
            var b=new BoardManager();b.Load(level);var t=new TelemetryTracker(new NullAnalyticsService(),()=>0);t.StartSession();t.StartLevel(level,index,level.VerifiedOptimalMoveCount);
            foreach(var tap in level.KnownSolution){b.RequestMove(tap);t.AcceptedTap(b.ReactionDepth,b.MovesRemaining,b.LastReactionWasCancelled);b.CompleteResolution();}t.Finish(true);return t.Result;
        }
        [UnityTest] public IEnumerator RepresentativeGameplaySemanticRoutingAndEvidence()
        {
            EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
            yield return new EnterPlayMode();yield return Ready();
            var game=Object.FindFirstObjectByType<PrototypeGame>();game.UnlockAllLevels();game.SelectLevel(9);yield return Ready();
            var audio=game.GetComponent<AudioManager>();audio.Focus(true);audio.Pause(false);
            var cues=new List<AudioCue>();var pulses=new List<HapticCue>();var rows=new List<string>{"scenario,event,audio_cue,haptic_cue,trigger_count"};string scenario="utilities";
            audio.CuePlayed+=cue=>cues.Add(cue);Haptics(game).PulseSent+=cue=>pulses.Add(cue);
            void Flush(){foreach(var g in cues.GroupBy(x=>x))rows.Add($"{scenario},semantic_request,{g.Key},none,{g.Count()}");foreach(var g in pulses.GroupBy(x=>x))rows.Add($"{scenario},haptic_output,none,{g.Key},{g.Count()}");cues.Clear();pulses.Clear();}
            game.OpenSettings();GameObject.Find("Haptics Setting").GetComponent<Button>().onClick.Invoke();yield return Ready();Capture("02-settings");game.CloseSettings();yield return Ready();
            Capture("07-normal-gameplay");cues.Clear();Assert.That(game.Undo(),Is.False);Assert.That(cues,Has.No.Member(AudioCue.Undo));
            game.OpenHint();Assert.That(cues.Count(c=>c==AudioCue.Hint),Is.EqualTo(1));
            Tap(game.CurrentLevel.KnownSolution[0]);yield return Settled(game);Assert.That(game.Undo(),Is.True);Assert.That(cues.Count(c=>c==AudioCue.Undo),Is.EqualTo(1));
            Assert.That(game.Undo(),Is.False);Assert.That(cues.Count(c=>c==AudioCue.Undo),Is.EqualTo(1));
            foreach(var p in game.CurrentLevel.KnownSolution){Tap(p);yield return Settled(game);}
            Assert.That(cues,Does.Contain(AudioCue.Win));Assert.That(cues,Has.No.Member(AudioCue.FirstPerfect));Flush();
            var seen=new HashSet<AudioCue>();
            foreach(int index in new[]{0,9,20,25,29,34,35,37,39})
            {
                scenario="campaign-"+(index+1);game.SelectLevel(index);yield return Ready();
                // Find a truly blocked tap using an isolated authoritative board, then test the actual presentation.
                foreach(var candidate in game.CurrentLevel.Placements.Where(p=>p.type==PieceType.Normal))
                {
                    var check=new BoardManager();check.Load(game.CurrentLevel);check.RequestMove(candidate.position);
                    if(check.Actions.Count!=0)continue;
                    Tap(candidate.position);yield return Settled(game);Assert.That(cues,Does.Contain(AudioCue.Blocked));break;
                }
                foreach(var p in game.CurrentLevel.KnownSolution)
                {
                    Tap(p);
                    if(index==39 && game.Board.Actions.Count>=6){yield return new WaitForSecondsRealtime(.45f);Capture("03-chain");}
                    yield return Settled(game);
                }
                Assert.That(game.Board.State,Is.EqualTo(GameState.Won));Assert.That(cues,Does.Contain(AudioCue.FinalExit));
                if(index==39){Capture("04-perfect");}
                foreach(var c in cues)seen.Add(c);Flush();
            }
            foreach(var cue in new[]{AudioCue.Tap,AudioCue.Move,AudioCue.Push,AudioCue.DirectionChange,AudioCue.Rotate,AudioCue.SwitchActivate,AudioCue.GateOpen,AudioCue.GateClose,AudioCue.Blocked,AudioCue.BigShift,AudioCue.Exit,AudioCue.FinalExit,AudioCue.ChainStep,AudioCue.ChainEscalation,AudioCue.MegaShift,AudioCue.FirstPerfect})Assert.That(seen,Does.Contain(cue));
            scenario="perfect-replay";game.Restart();yield return Ready();foreach(var p in game.CurrentLevel.KnownSolution){Tap(p);yield return Settled(game);}Assert.That(cues,Does.Contain(AudioCue.Perfect));Flush();
            scenario="daily-perfect";game.LocalNow=()=>new DateTime(2026,10,5);game.StartDaily(game.LocalNow());yield return Ready();foreach(var p in game.CurrentLevel.KnownSolution){Tap(p);yield return Settled(game);}Assert.That(cues,Does.Contain(AudioCue.DailyPerfect));Capture("05-daily");Flush();
            scenario="daily-complete";game.Restart();yield return Ready();Tap(game.CurrentLevel.KnownSolution[0]);yield return Settled(game);Assert.That(game.Undo(),Is.True);
            foreach(var p in game.CurrentLevel.KnownSolution){Tap(p);yield return Settled(game);}Assert.That(cues,Does.Contain(AudioCue.DailyComplete));Capture("05-daily");Flush();
            scenario="fail";game.SelectLevel(9);yield return Ready();
            for(int i=0;i<game.CurrentLevel.MoveLimit;i++){var blue=game.Board.Pieces.First(p=>p.Active&&p.Type==PieceType.Normal&&p.Color==PieceColor.Blue);Tap(blue.Position);yield return Settled(game);}
            Assert.That(game.Board.State,Is.EqualTo(GameState.Lost));Assert.That(cues,Does.Contain(AudioCue.Lose));Flush();
            scenario="chapter-mastered";var levels=LevelValidation.AllLevels();
            for(int i=20;i<40;i++){game.Progress.Select(i);game.Progress.Complete(i);if(i!=38)game.Progress.RecordPerfect(i,Certified(levels[i],i));}
            game.SelectLevel(38);yield return Ready();foreach(var p in game.CurrentLevel.KnownSolution){Tap(p);yield return Settled(game);}
            Assert.That(cues,Does.Contain(AudioCue.ChapterMastered));Assert.That(game.Progress.ChapterSummary(1).Mastered,Is.True);
            game.OpenChapter();yield return Ready();Object.FindFirstObjectByType<ChapterSelect>().ShowPage(1);yield return Ready();Capture("06-mastered");game.CloseChapter();Flush();
            game.SelectLevel(39);yield return Ready();
            // Overlay exists only in this editor test; not in runtime code/scenes/builds.
            var safe=GameObject.Find("Safe Area").transform;
            var overlay=PlaceholderVisuals.Label("VALIDATION ONLY semantic overlay",safe,Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),"VALIDATION ONLY · semantic routing\n4 voices · pitch ≤ 1.21 · pulse cooldown 100 ms\n17 production SFX assets installed",23,VisualTheme.Ink,new Vector2(.1f,.735f),new Vector2(.9f,.795f));
            overlay.raycastTarget=false;yield return Ready();Capture("01-semantic-overlay");Object.Destroy(overlay.gameObject);
            Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/audio"));File.WriteAllLines(Path.Combine(Application.dataPath,"../Validation/audio/semantic-events.csv"),rows);
            yield return new ExitPlayMode();
        }
    }
}
