# SHIFT — CHAPTER 4 / BAĞ GEÇİDİ (Relay) — 61–65 candidate pilot

**Status:** Isolated feature branch only, not in published SHIFT. The previous Echo/alternating rotator was rejected because it did not create distinct enough gameplay. This implementation starts again from feature/chapter3-levels-41-60 and does not import Echo code.

## Mechanic
Two matching teal Relay pads (channel 1 or 2) connect otherwise distant board positions. A Normal piece or pushed block enters a free Relay and emerges from its paired pad; it then attempts to continue in exactly the same direction. Color and direction are unchanged. The destination must be free (if occupied, entering is blocked). The paired pad does not retrigger just from the landing. Relay pair channels are static, deterministic and exactly two tiles per channel. Two pairs may coexist.

The model emits a dedicated BoardActionType.Transfer from entry to destination. Visual playback flashes both endpoints and jumps rather than animating a long slide. Reduced Motion removes the intermediate pause. Undo and Restart use the authoritative board snapshot, and the existing bounded-reaction rollback handles cycles.

This is primarily a spatial reasoning system, not rotation. It supports remote gate activation, timed occupancy, pushed-piece transports and geographically separated reactions.

## Candidate levels (independent simulation; Unity certification NOT YET RUN)
| Level | Name | Shortest recorded taps | Main idea |
|--|--|--:|--|
|61|Two Shores|2|Same linked pair, two colors and different incoming directions|
|62|Occupied Landing|3|Free the paired destination before transferring|
|63|Distant Switch|5|Link triggers a remote switch that opens a different route|
|64|Double Link|3|Two distinct pairs, landing must be vacated in the correct order|
|65|Linked Cascade|4|Push a differently facing target through a linked passage into a switch; complete another color|

All solutions were replayed in an independent BoardManager-style behavioral model, and a BFS found candidate shortest lengths as above. These are **not yet Unity-certified**. Level 61 is intentionally an introductory contrast to Level 60; later chapter levels should increase reasoning difficulty, not just taps.

## Unity verification
1. New worktree, branch feature/chapter4-relay-pilot. Never overwrite main or MORA worktree.
2. Recompile in Unity 6.3 LTS. If Safe Mode appears, inspect and fix all compiler errors before continuing.
3. Editor Test Runner: RelayMechanicTests and RelayCandidateTests.
4. Menu SHIFT > Chapter 4 > Validate Relay Levels 61-65. Require 5/5 proven via actual Unity BoardManager and shortest-search checks.
5. Exit Play; SHIFT > Chapter 4 > Create Isolated Relay Preview Scene; press Play and use Relay Preview Controls to load 61–65.
6. Confirm linked symbol and channel readability, blocked destination behavior, pushed-block portal, two independent pairs, Undo and Restart, no disappearing pieces, TR/EN guide, Reduced Motion, small phone resolution.
7. Human playtest each level. Reject levels where the link is ornamental or where a guess wins without an interesting chain. Run full original level regressions before a production merge.

Perfect Shift for 61+ is **not** in the runtime fingerprint certificate registry. No speculative Perfect awards. 41–60 preview stays independent, original 1–60 assets unchanged.

## Next design
After acceptance: 66–70 stronger link/switch/push combinations and intentional long/nonperfect valid routes; 71–80 consider one additional distinct mechanic only if this system proves fun. Avoid stacking gimmicks. MORA and original campaign remain frozen.
