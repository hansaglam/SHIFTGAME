using System;
using UnityEngine;

namespace Shift.Game
{
    public enum AudioCue { Tap, Move, Push, DirectionChange, Exit, Blocked, Win, Lose, Rotate, ChapterComplete, UIButton, PanelOpen, PanelClose, SwitchActivate, GateOpen, GateClose }

    [Serializable]
    public sealed class AudioClips
    {
        public AudioClip tap, move, push, directionChange, exit, blocked, win, lose, rotate, chapterComplete, uiButton, panelOpen, panelClose, switchActivate, gateOpen, gateClose;
        [Range(0, 1)] public float volume = .65f;
        [Range(0, 1)] public float sfxVolume = 1;
        public AudioClip Get(AudioCue cue) => cue switch
        {
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
        private bool muted;
        public bool Muted { get => muted; set { muted = value; if (muted) StopAll(); } }
        public float MasterVolume { get => clips?.volume ?? 0; set { if (clips != null) clips.volume = Mathf.Clamp01(value); RefreshVolume(); } }
        public float SfxVolume { get => clips?.sfxVolume ?? 0; set { if (clips != null) clips.sfxVolume = Mathf.Clamp01(value); RefreshVolume(); } }
        private float Gain => clips == null ? 0 : Mathf.Clamp01(clips.volume) * Mathf.Clamp01(clips.sfxVolume);
        private void RefreshVolume() { foreach (var voice in voices) if (voice != null) voice.volume = Gain; }
        public void Initialize(AudioClips bank)
        {
            clips = bank;
            for (int i = 0; i < voices.Length; i++)
            {
                if (voices[i] == null) voices[i] = gameObject.AddComponent<AudioSource>();
                voices[i].playOnAwake = false; voices[i].spatialBlend = 0;
            }
        }
        public void Play(AudioCue cue)
        {
            CuePlayed?.Invoke(cue);
            var clip = clips?.Get(cue);
            if (clip == null || Muted) return;
            var voice = voices[nextVoice]; nextVoice = (nextVoice + 1) % VoiceCount;
            voice.Stop(); voice.clip = clip; voice.volume = Gain; voice.Play();
        }
        public void StopAll() { foreach (var voice in voices) if (voice != null) voice.Stop(); }
    }
}
