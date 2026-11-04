# SHIFT × MORA — Visual pilot v0.2

This branch provides a **feature-flagged, presentation-only test**. The current `Enable Mora Pilot` setting remains OFF by default.

## Observed v0.1 issue
The first device/Editor observation was that MORA appeared briefly for **every** accepted tap and remained longer only after completion. This was functional, but distracting.

## v0.2 response
- Ordinary moves with fewer than three actions and reaction depth under two **do not** trigger MORA.
- Significant chains (3+ actions or depth 2+) trigger a cyan-tinted 1.3 s cue.
- Level wins trigger 1.9 s; confirmed Perfect Shift gets a warmer 2.4 s cue.
- These are still temporary single-sprite effects, **not** true facial or skeletal animation.
- Reduced Motion keeps the reaction static. The overlay remains non-interactive.
- Position remains provisional: the lower-right area can conflict with the hint/status panel. This must be redesigned before production.

## Artwork and Editor setup
Import the separately supplied transparent MORA preview as `Assets/_Game/Resources/Mora/Mora_Idle.png`, Sprite (2D and UI), Single. Without that file, no overlay is shown.

Open the existing `Prototype` scene. On the `PrototypeGame` component, turn on `Enable Mora Pilot`. It is only enabled in this local test scene; do not enable it for a production release.

## Check
1. Make single ordinary moves: **no MORA pop-up**.
2. Trigger a real multi-action chain: MORA shows momentarily.
3. Win a level: MORA reacts even if the last move has no large chain.
4. Earn verified Perfect Shift: the reaction is warmer/longer.
5. Verify no hint, Undo, Restart, board tap, Safe Area, Daily Shift or Level 40 finale regression.
6. Run `MoraReactionPolicyTests` and then the existing complete Unity tests.

Not included: Living World, player progression, multiple character frames or animations, new levels, purchases. **Unity compilation and device tests are not run from this connector. Do not merge without those checks.**
