# SHIFT × MORA visual pilot — v0.3

## Previous verification
The player confirmed Unity v0.2 works: ordinary actions stay silent, meaningful chains trigger MORA, and level completion is longer.

## What changed
- New temporary on-screen dock at **left of the result card** instead of above the Hint control.
- Draw order: above HUD/board (visible), below subsequent modal panels.
- Light lift, tilt and stretch animate a moment once rather than repeatedly flashing.
- Optionally load `Mora_Chain.png`, `Mora_Happy.png`, `Mora_Perfect.png` from `Assets/_Game/Resources/Mora/`. `Mora_Idle.png` remains required; optional art falls back to it.
- Reduced Motion still uses a static presentation. Pointer raycasts stay disabled.
- No changes to board, 40 levels, saves, ad logic or campaign ending. Opt-in setting defaults OFF.

## Local test
Close Unity. In `D:\UnityProjects\SHIFT\SHIFT-MORA` run `git status`, `git pull --ff-only`; do not reset the local scene or untracked MORA PNG. Reopen Prototype, verify Enable Mora Pilot is checked.
Check: a) normal actions show nothing; b) a strong chain shows MORA on the *left edge* near result text; c) wins/Perfect show longer; d) text and controls stay readable on narrow/tall portrait displays; e) Chapter/Settings panels cover it correctly; f) Reduced Motion works. Run MoraCompanionPilotTests, MoraReactionPolicyTests and full regression suite.

This remains a **single-sprite visual experiment** until optional face sprites are imported. It is not a full frame-animation or Living World implementation. Unity compile and device checks have **not** been run by the remote code changes. Keep the draft PR unmerged.
