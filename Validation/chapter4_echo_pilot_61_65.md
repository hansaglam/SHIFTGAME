# SHIFT — CHAPTER 4 / YANKI HATTI — Pilot 61–65

Status: experimental gameplay. This branch is NOT merged into main, does not change Level 1–60 files or publish new content.

## New system: Echo Rotator / Yankı Karosu

A stateful teal tile. Its **next** entering movable piece is turned clockwise, then the tile flips to counterclockwise. Next entering piece turns counterclockwise, and the tile resets to clockwise. Every entry counts, including pushed pieces. Independent Echo tiles have independent state. Undo, Restart, canceled reactions and the solver state key restore/account for the state exactly. No randomness. The tile is a terrain component, not a draggable or independently tappable piece.

The teal alternate-arrows/diamond icon updates after triggering; existing Rotate SFX and light haptics are reused and Reduced Motion is respected by the current Piece feedback layer. First encounter guide is in EN/TR.

## Candidate level route
| Level | Name | Objective | Draft shortest taps | Mechanic identity |
|--|--|--|--|--|
|61|First Echo|Red + Blue|2|Learn alternating turns |
|62|Hidden Push|Red + Blue|3|3-tap Perfect candidate via pushed piece, 4-tap normal win |
|63|Gate in the Echo|Red + Blue|4|Echo plus switch and gated route |
|64|Two Echoes|Red + Blue|4|Two independent tile states plus direction redirect |
|65|The Efficient Ripple|Red + Blue|4|Two Echo tiles, push chain, several orders |

Initial paths are checked by an independent Python model based on BoardManager rules; they MUST pass real Unity LevelValidation and CampaignCertification before acceptance. Levels 61–65 deliberately introduce the new system rather than being the hardest five levels in the game. The real challenge ramp will be in 66–80.

## Safe Unity test sequence
1. Open a new worktree of feature/chapter4-echo-pilot in Unity 6.3 LTS, not SHIFT-MORA or the production main working copy.
2. Run Editor Test Runner > EchoRotatorTests. Confirm all tests pass including Undo, blocked/no-cost, alternative path, and shortest proof.
3. SHIFT > Chapter 4 > Validate Echo Levels 61-65. Expect **5/5 certified**, otherwise no publishing.
4. Exit Play Mode. SHIFT > Chapter 4 > Create Isolated Echo Preview Scene. The tool creates Chapter4_EchoPreview.unity from the locked Prototype.unity. Do not add it to release Build Profiles.
5. Enter Play Mode and SHIFT > Chapter 4 > Preview Controls. Load 61–65. Test CW/CCW indicator, direction change, two pieces passing the same Echo, push entry, switch/gate, Undo, Restart, Reduced Motion and both languages.
6. Verify level 62's normal 4-move win vs 3-move shorter route. There is **no real Perfect reward** for Levels 61+ until the exact level fingerprints and shortest proofs are reviewed and installed in VerifiedOptimality.
7. Record screenshots at 1080x1920 and a narrow device. Confirm new teal tile readable on small phones; profile midrange Android before any main merge.
8. No change to previous Levels 1–60, MORA, monetization, player-save scheme or existing level certification.

## Design roadmap (not yet built)
- 66–70: Echo with switch/gate timing and irreversible mistakes; several viable routes, fewer optimal routes.
- 71–75: after Echo playtest, research ONE second genuinely different system rather than proliferating gimmicks. Candidate: a one-way conditional conduit, not approved for implementation yet.
- 76–80: creative combined puzzles, clear introduction/payoff rhythm, handcrafted chapter finale. Demand proven minimum with longer winning alternatives for selected levels. Aim for player reasoning, not high move counts alone.

Unity Editor compilation, device testing and certification have NOT been run by this connector. Status: implementation draft and independent model check only.
