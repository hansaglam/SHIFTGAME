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


## v0.6.2a — Curious-visibility hotfix (screenshot feedback)
The user supplied three screenshots: Idle visible, Curious message with the entire MORA portrait missing, and Happy visible again. The distributed high-resolution `Mora_Curious.png` in the v0.4 framed-sprite package was checked: it contains opaque pixels. Screenshots alone cannot distinguish a local Unity import problem from a rendering/transition problem.

Fix: **never replace the base portrait with an expression sprite**. The Idle image stays full opacity; two raycast-disabled expression overlays blend above it. A problematic optional expression can now fail visually without the whole character vanishing. The .28 second cooldown, messages, idle motion, backdrop, island, ambient, Reduced Motion, and gameplay hook are preserved.

**Focused test after pulling**: Chapters > MORA; tap in sequence to show Happy, Curious, Happy, Curious; hold on Curious for 2 seconds and check that MORA stays on screen throughout. Also rapidly tap 10+ times and verify no blank frames. If the Curious face is not visible but the Idle avatar remains, inspect the local `Mora_Curious.png` import (Sprite/Single/Alpha Is Transparency) and compare to the packaged file. Run MoraHomePanelTests and review Unity Console. This remote change has not been compiled in Unity or tested on-device.

## v0.7 — Growth foundation (editor preview only until Level 41+ exists)
The player confirmed that the Curious/Happy character never disappears with the v0.6.2a fix. Growth is implemented on the MORA feature branch only.

**Authoritative model**: `MoraGrowthPolicy.FromCampaign(progress.HighestCompleted, progress.LevelCount)` uses existing saved campaign completion. Levels 1–40 never produce Flow and are unchanged. Only first completions beyond index 39 count; replay does not inflate the progression. No PlayerPrefs or new currency save keys are created. Stage 1 begins at new content; Stage 2 at Level 50 complete, Stage 3 at Level 70 complete, Stage 4 at Level 100 complete. This model supports LevelCount beyond 40 when future campaign data becomes available.

**UI**: natural, small growth stage label and flow bar in MORA habitat. Island has subtle decorative blossoms and ambient aura by stage, while preserving the MORA sprite and puzzle UI. With only 40 existing levels, the player sees a clearly labeled Level 41 future milestone; no earned growth is claimed.

**Testing now**: In the Unity Editor only, **TEST +** cycles 1→2→3→4→1 while in the MORA habitat. It is only visual; pressing the button does not modify chapter records, any save, Perfect Shift or the 40 levels; leaving and reopening the habitat resets preview to the actual Stage 1. This Editor button is excluded from non-editor builds. A real 41+ campaign will need level content, chapter UI scalability and regression testing before this v0.7 progression can be earned by play.

**Validation**: Pull the branch with Unity closed; preserve local Prototype scene settings and sprite PNGs. Test TEST + through four stages, EN/TR milestone strings, dark/light layout, avatar stays visible, tap facial responses, Reduced Motion and Back to Chapters. Run MoraGrowthPolicyTests and the existing MORA/editor regression tests. No remote Unity compilation or device tests were run. Keep PR draft and unmerged.


## CORRECTION — MORA identity reset
The player rejected v0.7's cosmetic-only four-stage prototype as misleading. The home was restored to its last verified v0.6.2a layout (no growth TEST+). The previous teal sprite is now explicitly labeled a **special-companion test**, while the leaf-seed character from the original fifth concept board will be the true evolving MORA. Main character art is still outstanding. See `Validation/mora-companion-identity-v1.md`.

Keep the draft PR unmerged. On local SHIFT-MORA worktree, `git pull --ff-only` after checking `git status`; preserve existing local teal sprites and Unity scene. Verify chapter entry, expression reactions, absence of four fake stages, and transparent MORA title switching once the new main art is imported.
