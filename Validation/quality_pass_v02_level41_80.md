# SHIFT — Level 41–80 Design Quality Pass v02

**Date:** 2026-11-12  
**Branch:** `feature/level41-80-design-quality-v02`  
**Based on:** `feature/chapter3-4-quality-pass-v01`  
**Status:** **ISOLATED CANDIDATE — do not merge to production or enable Perfect Shift.**

## Goal and protection

Improve memorability and originality, not the number of levels. Level 81–100 is explicitly out of scope. Levels 1–40, MORA, live campaign wiring, PlayerPrefs/save data, advertisement integrations, and existing certified Perfect Shift registry were not changed.

## Evidence-driven findings

- All 40 YAML LevelData candidates (41–80) were inspected for board dimensions, mechanics and recorded tap routes.
- **55 → 60:** The pre-v02 Level 60 reused **the first eight taps of Level 55 verbatim**. That is unacceptable for a chapter finale, even though Level 60 had extra moves and pieces.
- **58 / 59:** The *longer* winning routes share the first **six** taps and much of their remaining sequence, a likely repeat family. They still need distinct redesigns; v02 does not silently claim a fix.
- **69 / 72 / 77 / 78:** Reused vertical-corridor/order-of-operations route family. Level 78 had already been revised in v01, but the cluster still requires in-game observation and likely replacement of at least one idea.
- **80:** The prior v01 revision already added a second gate dependency and separated resonance events. Its exact shortest solution (8), alternative longer win (9), two resonance channels, two gate channels and three colors are preserved; only design/reaction regression assertions have been added in v02.

## Level 60 — actual redesign candidate

The old board had one switch/gate channel, shortest route 10, and a close copy of the Level 55 intended route.

In v02:
- A **second switch (4,2) / initially closed gate (2,2)** forms a prerequisite dependency; this is not decoration. Both channels open on the newly recorded route.
- The directions of two late yellow pieces and one directional tile were changed. These modifications reposition the setup and increase multi-piece reactions rather than simply extending the move count.
- The shortest *independently modeled* win is now **11** committed taps; a different **12**-tap win is stored in the editor-only `Chapter3AlternateSolutions`. Move limit stays 12.
- Reaction counts from the independent model, original → v02: **multi-piece taps 2 → 3**, **gate openings 2 → 3**, **recorded move actions 21 → 28**. Both gate channels activate.
- The new optimal route no longer opens with the eight taps copied from Level 55. The shorter route begins at (2,4), rather than (1,2).
- Original asset GUID and Level 1–40 content are preserved.

**Important:** An independent Python model reproducing the repository's `BoardManager` rules was used to replay candidate routes and breadth-first-search their minima. It is not the Unity runtime. Treat all results above as **candidate evidence**, not authoritative Unity certification or proof of player enjoyment.

## Quality method added

Menu **SHIFT → Quality → Export Route Similarities 41-80** creates `Validation/level41_80_route_similarity.json` locally inside the project. It compares every 41–80 pair using:
- Common opening length;
- Longest common subsequence of tapped coordinates in the shortest solutions;
- The same measures for distinct *longer* winning routes where they exist.

Risk flags use a conservative heuristic, not an automatic deletion decision. Coordinate resemblance without the same mechanical insight can be a false positive; different-looking boards can also feel identical. A designer should annotate **insight / deceptive first impression / required aha / payoff** and blind-test flagged pairs.

Existing **SHIFT → Quality → Export Level 41-80 Decision Matrix** remains responsible for authoritative Unity solver evidence, reaction data, and decision-density metrics.

## Unity acceptance gate (not run through the GitHub connector)

1. Open this isolated branch using Unity **6.3 LTS 6000.3.23f1**.
2. Run EditMode tests: `ChapterFinaleQualityTests`, `Chapter3AlternativeRouteTests`, `Chapter3DraftTests`, `ResonanceExtendedDraftTests`, `ResonanceMechanicTests`. Zero new failures.
3. Run **SHIFT → Chapter 3 → Validate Draft Levels 41-60** (target 20/20), Chapter 4 levels 61–65 (5/5), 66–80 (15/15); then **SHIFT → Quality → Export Level 41-80 Decision Matrix** (40/40).
4. Run the new route-similarity export; manually examine 58/59, 69/72/77/78 and any new risk pairs.
5. Preview Level 60 and Level 80 without giving away solutions. For each test player, record attempt time, restarts, incorrect taps, hint usage, reactions to the chain and whether the solution feels surprising rather than rote.
6. Validate 8×8 touch targets and reduced-motion animations on compact Android and iOS devices.
7. If any official solver check or human review fails, rework the candidate and rerun. **Never fabricate Perfect certificates**.

## Unresolved follow-up

v02 fixes the proven 55/60 eight-tap copy, but **is not a complete redesign of all repetitive levels**. 58/59 and the Chapter 4 vertical-lane family remain explicitly open. The Release gate remains closed until Unity and blind playtests pass.
