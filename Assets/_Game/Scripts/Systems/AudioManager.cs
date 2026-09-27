using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shift.Game
{
    public enum AudioCue { Tap, Move, Push, DirectionChange, Exit, Blocked, Win, Lose, Rotate, ChapterComplete, UIButton, PanelOpen, PanelClose, SwitchActivate, GateOpen, GateClose, FinalExit, ChainStep, ChainEscalation, BigShift, MegaShift, Undo, Hint, Restart, Perfect, FirstPerfect, DailyComplete, DailyPerfect, ChapterMastered }

    [Serializable]
    public sealed class AudioClips
    {
        public AudioClip tap, move, push, directionChange, exit, blocked, win, lose, rotate, chapterComplete, uiButton, panelOpen, panelClose, switchActivate, gateOpen, gateClose;
        public AudioClip finalExit, chainStep, chainEscalation, bigShift, megaShift, undo, hint, restart, perfect, firstPerfect, dailyComplete, dailyPerfect, chapterMastered;
        [Range(0, 1)] public float volume = .65f;
        [Range(0, 1)] public float sfxVolume = 1;
        public AudioClip Get(AudioCue cue) => cue switch
        {
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
        private bool muted, paused, unfocused, outcomePlayed;
        private readonly float[] voiceGain = { 1, 1, 1, 1 };
        private readonly Dictionary<AudioCue, float> last = new Dictionary<AudioCue, float>();
        public event Action<AudioCue, float, float> VoiceStarted;
        public int StartedVoices { get; private set; }
        public bool Suspended => paused || unfocused;
        public static float ReactionPitch(int step) => 1 + .035f * Mathf.Clamp(step - 1, 0, 6);
        public static float CueGain(AudioCue cue) => cue switch
        {
            AudioCue.UIButton or AudioCue.PanelOpen or AudioCue.PanelClose or AudioCue.Restart => .28f,
            AudioCue.Blocked => .22f, AudioCue.Tap => .34f,
            AudioCue.Move or AudioCue.ChainStep => .42f,
            AudioCue.Hint or AudioCue.Undo => .45f,
            AudioCue.ChainEscalation => .25f,
            AudioCue.BigShift => .65f, AudioCue.MegaShift => .75f,
            AudioCue.Perfect or AudioCue.FirstPerfect or AudioCue.DailyPerfect or AudioCue.ChapterMastered => .85f,
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
            // ChainStep describes the pitched primary voice; it is not a second stacked sound.
            if (step >= 2) CuePlayed?.Invoke(AudioCue.ChainStep);
            if (step == 4) Play(AudioCue.ChainEscalation, 1.08f);
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
                voices[i].playOnAwake = false; voices[i].spatialBlend = 0; voices[i].loop = false;
            }
        }
        public void Play(AudioCue cue) => Play(cue, 1);
        private void Play(AudioCue cue, float pitch)
        {
            if (Suspended) return;
            CuePlayed?.Invoke(cue); // Semantic request, including missing assets/mute; not proof of audible output.
            var clip = clips?.Get(cue);
            if (clip == null || Muted || Gain <= 0) return;
            float now = Time.realtimeSinceStartup;
            if (last.TryGetValue(cue, out float previous) && now - previous < .045f) return;
            last[cue] = now;
            int index = nextVoice; nextVoice = (nextVoice + 1) % VoiceCount;
            var voice = voices[index]; if (voice == null) return;
            // At most four voices × .2 gain: conservative peak headroom even for full-scale source assets.
            voiceGain[index] = .2f * CueGain(cue);
            voice.Stop(); voice.clip = clip; voice.pitch = Mathf.Clamp(pitch, .9f, 1.21f); voice.volume = Gain * voiceGain[index]; voice.Play();
            StartedVoices++; VoiceStarted?.Invoke(cue, voice.pitch, voice.volume);
        }
        private void StopVoices() { foreach (var voice in voices) if (voice != null) voice.Stop(); }
        public void StopAll() { StopAllCoroutines(); StopVoices(); }
        private void OnDisable() => StopAll();
    }
}
