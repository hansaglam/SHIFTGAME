# SHIFT Chapter 4 — Resonance Levels 66–80 (pilot handoff)

**Status:** 15 isolated draft assets committed to `feature/chapter4-resonance-levels-66-80`. This is **not a release** or an authoritative Unity-certified Perfect Shift registry. Level 1–60 assets, original `Prototype.unity`, MORA, save data and ad config have not been edited by this branch.

## Core design retained

Resonance 2.0: a moving piece entering a node triggers the paired node's waiting colored piece, then the source continues its movement in the original direction; remote move executes before the source resumes. Each channel can pulse only once per tap. No RNG, drag, teleport or added currency. Undo/Restart, cyclic rollback and Reduced Motion are inherited from the already tested V2 pilot. No new core gameplay mechanic is introduced.

## Candidate progression

| Levels | Intended focus | Independently found minimum taps | Winning alternative |
|---|---|---|---|
| 66–70 | Signals as remote planning; three colors and two channels | 5, 5, 6, 6, 6 | Shortest + 1 tap |
| 71–75 | Gates, directions, order-of-operations and dual signals | 6, 6, 7, 7, 7 | Shortest + 1 tap |
| 76–79 | Restrictive routing; delayed signals and harder decision sequences | 7, 8, 8, 8 | Shortest + 1 tap |
| 80 | Three colors, two signal pairs, a gate and direction tile | 9 | 10 taps |

**How candidates were chosen:** thousands of alternative layouts evaluated in a separate behavioral simulation based on the current `BoardManager` rules. Filtered for valid board layers, correct Resonance pairing, a winning path, shortest-route evidence, a different longer winning path, active Resonance on the candidate shortest route and variety of objective/gate systems. This screening does **not** replace Unity's own engine test and does not prove player enjoyment.

### Mandatory Unity tests

1. Open the new worktree in Unity 6.3 LTS 6000.3.23f1. Exit Safe Mode only once all compilation errors are resolved.
2. Run **EditMode > ResonanceMechanicTests** and **ResonanceExtendedDraftTests**. Both must pass. The latter replays all saved shortest and +1 alternate solutions using the actual Unity BoardManager and calls the authoritative exhaustive prover.
3. Menu **SHIFT > Chapter 4 > Validate Resonance Levels 66-80**. Required result: **15/15 certified**. A failed level is blocked and must not be shipped. For prior levels separately run **Validate Resonance Levels 61-65**, target **5/5**.
4. Menu **SHIFT > Chapter 4 > Analyze Resonance Decisions 66-80**. Review meaningful decisions, efficiency, solution-count estimate, state count and reaction depth. A large reaction depth is *not* sufficient for an interesting puzzle.
5. Exit Play Mode and select **SHIFT > Chapter 4 > Create Isolated Resonance Preview Scene**. If one exists, choose **Replace Preview** (only replaces `Chapter4_ResonancePreview.unity`; it is an experiment). `Prototype.unity` remains unchanged. The regenerated preview now holds 80 level references.
6. Enter Play, open **SHIFT > Chapter 4 > Resonance Preview Controls**, play Levels 66–80. Check especially 68/70/73 for two independent channels, 75 for three colors + gate, and 80 for full chapter finale.
7. Perform human playtests **without showing the known routes**. Assess first understanding, unintended accidental wins, satisfying chains, restarts, hint use, time-to-win, normal-vs-optimal discovery, tap correctness and chapter climax. Replace levels that feel like repeated tapping or minor board reskins.
8. QA 1080×1920, compact Android devices and iOS. Check 8×8 boards (75, 78, 80) for readable tiles and hit targets at small sizes; line animation readability, localization and Reduced Motion. Run all prior level regressions.
9. Only after design freeze + passing solver proofs: create exact fingerprint-bound `VerifiedOptimality` certificates. Until then **Perfect Shift remains intentionally unavailable** for 61–80 and no live player progress should be migrated.

### Acceptance rule

All Editor tests green; correct and pleasant interaction on device; at least two genuinely nontrivial decisions for difficult levels; no repetitive filler; clear visually understandable long-range signals; confirmed longer but winning routes. Failures should be **reworked**, not hand-waved by increasing MoveLimit.

**Limitations:** Unity Editor and on-device playtests cannot be run through the GitHub connector. No claims of Unity compilation, certified Perfect Shift or published content have been made by this handoff.
