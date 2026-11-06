# SHIFT × MORA Habitat v0.6.2 — Complete single-sprint implementation

Player request: perform all six changes before asking for one combined Unity test.
Development branch: `feature/mora-visual-pilot-v01`. Flag **Enable Mora Pilot** remains off by default. Do not merge to main before Unity/device validation.

## Six changes implemented
1. **Rapid taps:** a .28s short cooldown prevents restart/flash; a .22s crossfade moves between facial sprites. The base image always stays visible; no hide/show on tap.
2. **Persistent presence:** MORA stays instantiated and visible in the habitat; Idle/Happy/Curious states, gentle scale response. Only Back to Chapters closes the habitat.
3. **Natural layout:** smaller title and subtitles, larger character, a slimmer speech ribbon, simplified footer. Puzzle HUD is not modified.
4. **Habitat framing:** loads optional original `Resources/Mora/Mora_Habitat.png` transparent moss-and-stone island under the character, with the existing AlpineLake environment behind it. Falls back to tinted natural shapes when art is absent. No other dependency.
5. **Dialogue:** short localized greeting text, alternating happy/curious states, smaller non-blocking ribbon and a small honest preview note.
6. **Quiet ambient life:** few fixed-location fireflies plus two water glints, low-amplitude movement only in habitat. Reduced Motion makes both MORA and ambient static.

## Asset delivery (one-time manual import)
Copy the supplied transparent `Mora_Habitat.png` to
`D:\UnityProjects\SHIFT\SHIFT-MORA\Assets\_Game\Resources\Mora\Mora_Habitat.png`
Set Texture Type **Sprite (2D and UI)**, Sprite Mode **Single**, Alpha Is Transparency **On**. Set max size 2048 and compression off for quality inspection. Do not replace the five already imported expressions. No sprite binaries are committed to the repo.

## One combined Unity test after all six tasks
- Close Unity; `git status`, `git pull --ff-only` in the SHIFT-MORA worktree. Preserve locally changed `Prototype.unity`, `PackageManagerSettings.asset` and untracked MORA sprite files.
- Open Prototype with Enable Mora Pilot ON; open Chapters > MORA.
- Check the new mossy island scenery and avatar at standard 9:16 and tall aspect ratio.
- Tap quickly 10+ times: MORA must stay visible continuously, only accepted taps should alternate Happy/Curious, and messages should be stable.
- Leave idle for several seconds: sprite returns to Idle, gently breathes; ambient glints move subtly.
- Toggle Reduced Motion: no breathing/bouncing/ambient drift, facial changes and taps still work.
- Return to Chapters, reopen MORA, verify clean reset. Then play a level with chain, normal complete and Perfect to confirm v0.5 didn't regress.
- Open Settings/Chapter/guide panels; confirm no input leaks or red Console errors; run MoraHomePanelTests, MoraCompanionPilotTests, MoraReactionPolicyTests and standard regression suite.
- Device test portrait Android/iOS before any merge.

No LevelData, BoardManager, progress/save, consent/ads, campaign 40 or monetization code was changed.
This is an editor/device **untested remote commit**, not a production merge.
