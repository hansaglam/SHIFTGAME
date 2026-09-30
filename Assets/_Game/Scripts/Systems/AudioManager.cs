using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shift.Game
{
    public enum AudioCue { Tap, Move, Push, DirectionChange, Exit, Blocked, Win, Lose, Rotate, ChapterComplete, UIButton, PanelOpen, PanelClose, SwitchActivate, GateOpen, GateClose, FinalExit, ChainStep, ChainEscalation, BigShift, MegaShift, Undo, Hint, Restart, Perfect, FirstPerfect, DailyComplete, DailyPerfect, ChapterMastered, CampaignComplete }

    [Serializable]
    public sealed class AudioClips
    {
        public AudioClip tap, move, push, directionChange, exit, blocked, win, lose, rotate, chapterComplete, uiButton, panelOpen, panelClose, switchActivate, gateOpen, gateClose;
        public AudioClip campaignComplete;
        public AudioClip finalExit, chainStep, chainEscalation, bigShift, megaShift, undo, hint, restart, perfect, firstPerfect, dailyComplete, dailyPerfect, chapterMastered;
        [Range(0, 1)] public float volume = .65f;
        [Range(0, 1)] public float sfxVolume = 1;
        public AudioClip Get(AudioCue cue) => cue switch
        {
            AudioCue.CampaignComplete => campaignComplete,
            AudioCue.FinalExit => finalExit ?? exit, AudioCue.ChainStep => chainStep ?? move,
            AudioCue.ChainEscalation => chainEscalation ?? directionChange, AudioCue.BigShift => bigShift ?? push,
            AudioCue.MegaShift => megaShift ?? bigShift ?? push, AudioCue.Undo => undo, AudioCue.Hint => hint,
            AudioCue.Restart => restart ?? uiButton, AudioCue.Perfect => perfect ?? win,
            AudioCue.FirstPerfect => firstPerfect ?? perfect ?? win, AudioCue.DailyComplete => dailyComplete ?? win,
            AudioCue.DailyPerfect => dailyPerfect ?? perfect ?? win, AudioCue.ChapterMastered => chapterMastered ?? perfect ?? win,
            AudioCue.SwitchActivate => switchActivate, AudioCue.GateOpen => gateOpen, AudioCue.GateClose => gateClose,
            AudioCue.UIButton => uiButton, AudioCue.PanelOpen => panelOpen, AudioCue.PanelClose => panelClose,
            AudioCue.Tap => tap, AudioCue.Move => move, AudioCue.Push => push,
            AudioCue.DirectionChange => directionChange, AudioCue.Exit => exit,
            AudioCue.Rotate => rotate, AudioCue.ChapterComplete => chapterComplete,
            AudioCue.Blocked => blocked, AudioCue.Win => win, AudioCue.Lose => lose, _ => null
        };
    }

    public sealed class AudioManager : MonoBehaviour
    {
        public event Action<AudioCue> CuePlayed;
        private const int VoiceCount = 4;
        private readonly AudioSource[] voices = new AudioSource[VoiceCount];
        private AudioClips clips;
        private int nextVoice;
        private readonly int[] voicePriority = new int[VoiceCount];
        private bool muted, paused, unfocused, outcomePlayed;
        private readonly float[] voiceGain = new float[VoiceCount];
        private readonly Dictionary<AudioCue, float> last = new Dictionary<AudioCue, float>();
        public event Action<AudioCue, float, float> VoiceStarted;
        public int StartedVoices { get; private set; }
        public bool Suspended => paused || unfocused;
        public static float ReactionPitch(int step) => 1 + .035f * Mathf.Clamp(step - 1, 0, 6);
        public static float CueGain(AudioCue cue) => cue switch
        {
            AudioCue.UIButton or AudioCue.PanelOpen or AudioCue.PanelClose or AudioCue.Restart => .28f,
            AudioCue.Blocked => .35f, AudioCue.Tap => .50f,
            AudioCue.Move => .50f, AudioCue.Push => .48f,
            AudioCue.DirectionChange or AudioCue.Rotate => .55f,
            AudioCue.SwitchActivate => .55f, AudioCue.GateOpen => .45f, AudioCue.GateClose => .60f,
            AudioCue.Exit or AudioCue.FinalExit => .50f,
            AudioCue.ChainStep or AudioCue.ChainEscalation => .28f,
            AudioCue.Hint => .60f, AudioCue.Undo => .40f,
            AudioCue.BigShift => .70f, AudioCue.MegaShift => .60f,
            AudioCue.Perfect or AudioCue.FirstPerfect or AudioCue.DailyPerfect or AudioCue.ChapterMastered => .65f,
            AudioCue.CampaignComplete => .65f, AudioCue.Win or AudioCue.DailyComplete => .75f,
            AudioCue.Lose => .38f, _ => .55f
        };
        public static AudioCue Outcome(bool won, bool perfect, bool first, bool daily, bool mastered) =>
            !won ? AudioCue.Lose : mastered ? AudioCue.ChapterMastered : daily ? (perfect ? AudioCue.DailyPerfect : AudioCue.DailyComplete) :
            perfect ? (first ? AudioCue.FirstPerfect : AudioCue.Perfect) : AudioCue.Win;
        public void RearmOutcome() { StopAllCoroutines(); outcomePlayed = false; }
        public void BeginLevel() { StopAll(); RearmOutcome(); last.Clear(); }
        public void Pause(bool value) { paused = value; if (Suspended) StopAll(); }
        public void Focus(bool value) { unfocused = !value; if (Suspended) StopAll(); }
        public void PlayOutcome(AudioCue cue, HapticService haptics)
        {
            if (outcomePlayed) return; outcomePlayed = true;
            if (Suspended) return;
            StartCoroutine(OutcomeAfterBreath(cue, haptics));
        }
        private IEnumerator OutcomeAfterBreath(AudioCue cue, HapticService haptics)
        {
            // Presentation-only breath, never delays board resolution/progression or input.
            yield return new WaitForSecondsRealtime(.075f);
            if (Suspended) yield break;
            Play(cue); haptics.Outcome(cue);
        }
        public void Reaction(AudioCue cue, int step)
        {
            if (Suspended) return;
            Play(cue, ReactionPitch(step));
            // One restrained pulse per eligible step; shared-clip cooldown prevents milestone duplication.
            if (step >= 2) Play(AudioCue.ChainStep, ReactionPitch(step));
            if (step == 4) CuePlayed?.Invoke(AudioCue.ChainEscalation);
        }
        public void ChainPayoff(int depth)
        {
            var tier = ReactionPresentation.Classify(depth);
            if (tier == ReactionTier.BigShift) Play(AudioCue.BigShift);
            else if (tier == ReactionTier.MegaShift) Play(AudioCue.MegaShift);
        }
        public bool Muted { get => muted; set { muted = value; if (muted) StopVoices(); } }
        public float MasterVolume { get => clips?.volume ?? 0; set { if (clips != null) clips.volume = Mathf.Clamp01(value); RefreshVolume(); } }
        public float SfxVolume { get => clips?.sfxVolume ?? 0; set { if (clips != null) clips.sfxVolume = Mathf.Clamp01(value); RefreshVolume(); } }
        private float Gain => clips == null ? 0 : Mathf.Clamp01(clips.volume) * Mathf.Clamp01(clips.sfxVolume);
        private void RefreshVolume() { for (int i = 0; i < voices.Length; i++) if (voices[i] != null) voices[i].volume = Gain * voiceGain[i]; }
        public void Initialize(AudioClips bank)
        {
            clips = bank;
            for (int i = 0; i < voices.Length; i++)
            {
                if (voices[i] == null) voices[i] = gameObject.AddComponent<AudioSource>();
                voices[i].volume = Gain * voiceGain[i]; voices[i].playOnAwake = false; voices[i].spatialBlend = 0; voices[i].loop = false;
            }
        }
        public void Play(AudioCue cue) => Play(cue, 1);
        private void Play(AudioCue cue, float pitch)
        {
            if (Suspended) return;
            CuePlayed?.Invoke(cue); // Semantic request, including missing assets/mute; not proof of audible output.
            var clip = clips?.Get(cue);
            if (clip == null || Muted || Gain <= 0) return;
            if (cue == AudioCue.Tap) return; // Committed Move/Push supplies the audible input body.
            float now = Time.realtimeSinceStartup;
            if (last.TryGetValue(cue, out float previous) && now - previous < (cue == AudioCue.ChainStep ? .10f : .045f)) return;
            foreach (var pair in last)
                if (pair.Key != cue && clips.Get(pair.Key) == clip && now - pair.Value < .10f) return;
            int priority = Priority(cue);
            if (priority == 5) StopVoices();
            int index = -1;
            for (int i = 0; i < VoiceCount; i++)
            {
                int candidate = (nextVoice + i) % VoiceCount;
                if (!voices[candidate].isPlaying) { index = candidate; break; }
                if (voicePriority[candidate] <= priority && (index < 0 || voicePriority[candidate] < voicePriority[index])) index = candidate;
            }
            if (index < 0) return;
            last[cue] = now;
            nextVoice = (index + 1) % VoiceCount; voicePriority[index] = priority;
            var voice = voices[index]; if (voice == null) return;
            // At most four voices × .2 gain: conservative peak headroom even for full-scale source assets.
            voiceGain[index] = .2f * CueGain(cue);
            voice.Stop(); voice.clip = clip; voice.pitch = Mathf.Clamp(pitch, .9f, 1.21f); voice.volume = Gain * voiceGain[index]; voice.Play();
            StartedVoices++; VoiceStarted?.Invoke(cue, voice.pitch, voice.volume);
        }
        private static int Priority(AudioCue cue) => cue switch
        {
            AudioCue.CampaignComplete or AudioCue.Win or AudioCue.Perfect or AudioCue.FirstPerfect or AudioCue.DailyComplete or AudioCue.DailyPerfect or AudioCue.ChapterMastered => 5,
            AudioCue.BigShift or AudioCue.MegaShift => 4,
            AudioCue.Exit or AudioCue.FinalExit or AudioCue.GateOpen or AudioCue.GateClose or AudioCue.SwitchActivate or AudioCue.Rotate or AudioCue.Push => 3,
            AudioCue.ChainStep or AudioCue.ChainEscalation => 1,
            AudioCue.Blocked => 0, _ => 2
        };
        private void StopVoices() { foreach (var voice in voices) if (voice != null) voice.Stop(); }
        public void StopAll() { StopAllCoroutines(); StopVoices(); }
        private void OnDisable() => StopAll();
    }
}
