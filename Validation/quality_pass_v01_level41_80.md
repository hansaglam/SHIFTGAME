# SHIFT Level 41–80 — Quality Pass v01 (12 Nov 2026)

**Branch:** `feature/chapter3-4-quality-pass-v01`, based on `feature/chapter4-resonance-levels-66-80`.
**Scope:** Four prioritized Rezonans level designs plus quality analysis code. No edits to 1–40, MORA, production scene, BoardManager, save service or shipped Perfect Shift registry.

### Independent simulator comparison — not Unity certification

| Level | Previous shortest | Revised shortest | Multi-piece taps (old→new) | Pulse events (old→new) | Gate openings (old→new) |
|---|---:|---:|---:|---:|---:|
| 68 | 6 | 6 | 1→4 | 3→4 | 1→1 |
| 75 | 7 | 7 | 1→1 | 1→1 | 1→2 |
| 78 | 8 | 8 | 3→4 | 3→3 | 1→1 |
| 80 | 9 | 8 | 1→2 | 2→2 | 1→2 |

Every edited level has a recorded shortest route and a distinct winning route with **+1 tap**, both replayed and searched to the minimum in a separate BoardManager-style simulator. These are *candidates* until Unity's authoritative solver and EditMode tests pass.

**Design changes**
- **68:** move Rezonans activation into more than one useful decision; remove decorative 0,0 wall and ineffective rotator; retain two separately distinguished channels.
- **75:** add the second switch/gate channel, coupling the red and blue exit pathways; avoid merely increasing move budget. The board still has one Rezonans event; it remains a design-risk checkpoint.
- **78:** redistribute path control with a directional tile and move the gate; remove unrelated direction decorations. Still visually resembles 69/72 and needs human similarity review.
- **80:** split the two Rezonans stages into separate player decisions and add a second interdependent gate/switch. Red, blue and yellow form an ordered cascade instead of an all-at-once signal followed by eight simple taps.

**Open design risks:** 69, 72 and 78 share a vertical-lane design family; consider replacing one after physical playtests rather than creating more reskins. Level 75 has extra strategic gate dependency but only a single Rezonans pulse. Chapter atmosphere/progression and levels 41–60 are unchanged in this quality pass; their design review is not claimed complete.

### Unity review / acceptance

1. Open the isolated branch in Unity 6.3 LTS. Run EditMode `ResonanceExtendedDraftTests` (including four new reaction profile cases), existing `ResonanceMechanicTests`, and Chapter 3 tests.
2. **SHIFT → Chapter 4 → Validate Resonance Levels 66-80**: require 15/15. Check Level 61–65 independently: require 5/5.
3. **SHIFT → Quality → Export Level 41-80 Decision Matrix**: require 40/40 actual Unity shortest-path proofs; inspect the output `Validation/level41_80_unity_quality_matrix.json`. Flags identify potential filler, not definitive player difficulty.
4. **SHIFT → Chapter 4 → Create Isolated Resonance Preview Scene** then play/test 68,75,78,80 on a phone. Ensure visual clarity of paired channels, safe-area and Reduced Motion, no misleading signal while Undo/Restart, and usable 8×8 hit targets.
5. Observe *blind* player tests: wrong choices, useful surprises, undo/restarts, hint usage, first-attempt time, reaction understanding, and whether 80 is satisfying without showing solutions.
6. Do not merge or generate the runtime Perfect Shift registry until all authoritative proofs and player evaluation pass.

**What this is not:** a release, proof of retention, device performance benchmark, or Unity test execution result.
