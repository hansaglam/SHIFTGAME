# MORA v0.5 — Animation & Personality (opt-in pilot)

## Status
The player confirmed v0.3/v0.4.1 visual positions and events in Unity. This change is an untested **code iteration** on the development branch, NOT a release.

## Changed
- Preserves the *user-verified* MORA dock (x 0–.22, y .135–.277) and optional imported sprites.
- Differentiates procedural animation timing/poses by reaction:
  - `Chain`: single short bounce, 1.3 s
  - `Complete`: gentle double bounce, 1.9 s
  - `Perfect`: stronger double bounce, 2.4 s
- Existing per-event sprite selection and fallback (Mora_Idle, Mora_Chain, Mora_Happy, Mora_Perfect) remain.
- `MoraMotionProfile.Sample` is a deterministic, frame-rate independent pose sampler with bounded translation/tilt/scale.
- Respect Reduced Motion *at every frame*, so toggling it during playback instantly stops movement.
- `MoraReactionPolicy` unchanged: **no** responses to routine moves; success reactions stay opt-in.

## Important limitation
This is high-quality *procedural body motion on existing sprite art*, NOT actual eye-blink, head bone rig, sprout rig, facial skeletal animation, or isolated chest-core glow. Those need additional separated layers/sprites or a 2D rig. Do not misrepresent this version as containing them. No paid features or companion progression.

## Test in Unity
1. Close Unity Editor.
2. In `D:\UnityProjects\SHIFT\SHIFT-MORA` run `git status` and `git pull --ff-only`; stop on merge conflicts and never reset user's local `Prototype.unity`, Project Settings or MORA PNGs.
3. Open `Prototype`, check `Enable Mora Pilot`; the existing 5 locally imported sprites still work. There are no new PNGs.
4. Trigger a normal action: no MORA cameo. Trigger a 3+ action/depth2+ chain: one short bounce. Win: gentler double bounce. Perfect: more prominent double bounce.
5. Verify completion text, board/hint/undo/restart, modal panels, daily shift, narrow 9:16/tall portrait screen, Reduced Motion and zero Console red errors.
6. Run `MoraCompanionPilotTests`, `MoraReactionPolicyTests` and complete regression suite. Test on Android/iOS before merge.

## No edits
No changes to BoardManager, puzzles, chapter data, saves, rewards, ad logic, Level 40 finale or main branch.
