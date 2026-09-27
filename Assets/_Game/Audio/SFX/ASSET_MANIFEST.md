# SHIFT SFX delivery manifest — assets NOT finalized

No production SFX or music files are installed. Existing scene references remain null. The semantic system is wired, but release audio sign-off is blocked on creation/licensing, import, assignment and listening review of the final sound set.

Only the editor test creates a 100 ms mono procedural validation tone in memory, then destroys it. It is not a production sound, is not persisted, is not enabled by a runtime fallback, and is not shipped. No third-party audio downloaded. No music added.

| Suggested original WAV | Duration target | Character | AudioClips slots / reuse |
|---|---|---|---|
| input_tap.wav | 35–70 ms | soft dry tactile tap | tap; uiButton quieter |
| blocked_tick.wav | 25–50 ms | muted, low-energy tick | blocked |
| move_slide.wav | 60–120 ms | light mechanical slide | move; chainStep is primary voice metadata |
| push_snap.wav | 60–130 ms | compact weight/contact | push |
| route_click.wav | 40–90 ms | restrained directional click | directionChange; rotate with subtle variation |
| switch_toggle.wav | 50–100 ms | tactile toggle | switchActivate |
| gate_latch.wav | 60–140 ms | soft latch | gateOpen, gateClose distinct variants |
| exit_release.wav | 80–160 ms | clean release | exit; finalExit slightly fuller |
| chain_accent.wav | 60–150 ms | low-volume harmonic body | chainEscalation, bigShift, megaShift; no aggressive brightness |
| undo_snap.wav | 60–130 ms | short reverse/snap | undo |
| hint_reveal.wav | 100–200 ms | soft reveal, no victory jingle | hint |
| complete.wav | 150–300 ms | restrained resolution | win, dailyComplete |
| perfect.wav | 180–400 ms | clean premium accent | perfect, firstPerfect, dailyPerfect, chapterMastered; restrained richer variants |
| fail.wav | 80–160 ms | subdued negative tick | lose |

PanelOpen/PanelClose/Restart may reuse a quiet input variant; ChapterComplete is retained for compatibility but current outcome routing chooses one Win/Perfect/Daily/Mastered cue. Explicit slot fallbacks are in AudioClips.Get. Unassigned clips are silent, never synthesized automatically.

Deliver mono 44.1/48 kHz WAV source, minimal leading silence, short fades/tails, no clipping, conservative peak headroom (e.g. ≤ -3 dBFS), no heavy limiting. Small short clips: Unity Force To Mono, 2D, Decompress On Load, Preload Audio Data; evaluate PCM versus ADPCM per asset and device. Do not stream these tiny effects. Verify final waveforms/import settings after assets exist. These are delivery targets, not claims of already configured importers.

Assign clips in PrototypeGame → Audio Clips on Prototype.unity. Master .65 / SFX 1 preserved. Cue hierarchy and four-voice headroom live in AudioManager. Each active voice is at most master × SFX × .2 × cue gain; pitch stays 1.00–1.21 for chains. A repeat of the same audible cue within 45 ms is suppressed. Chain step metadata does not layer another click.

Listening approval needed on headphones and phone speaker: softness of repeated taps, audibility at low volume, gate/route differentiation, large-chain mix, muted blocked taps, and Perfect/Mastered payoff. Screenshots and semantic logs cannot establish audio quality.
