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
- **58 / 59 (pre-continuation baseline):** The *longer* winning routes previously shared the first **six** taps. The later Level 59 revision below changes this; Level 58 itself is not modified.
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

v02 fixes the proven 55/60 eight-tap copy. The continuation also revises 59 to break its repeated routes with 58, but **this is not a complete redesign of all repetitive levels**. The Chapter 4 vertical-lane family remains open. Unity acceptance and blind playtests are still required before release.


## Design-first continuation — Level 59 and corridor checks

The owner chose to defer **real-player testing until the full content-design pass is complete**. This changes the sequence, not the release safety conditions. Independent candidate simulation and editor regression checks are kept throughout authoring; human testing, overall feel review, device UX and final Unity signoff can happen at the end of the content-design cycle.

### Level 59 — *Borrowed Momentum* (new candidate)
- Previous 58 and 59 used virtually identical 4×6 board silhouettes and similar longer-winning routes. Rather than copy the mechanical idea, the revised 59 places the chain on a **6×4 horizontal presentation**, changes the blue helper's trajectory, repositions the switch and redirects an early path.
- The blue helper now physically participates in the solution instead of sitting as dead decoration. A long chain builds the gate-opening setup.
- Independent simulation, reproducing current BoardManager movement and comparing against previously recorded shortest solutions, found an **optimal 10-tap win and a different 11-tap winning route**, within the original 11-move limit.
- Recorded 10-tap route has **seven multi-piece taps**, **44 movement/turn/gate/delivery actions total**, one gate opening, and all three red targets delivered. The previous 58 level remains at nine taps.
- Coordinate longest-common-subsequence between Level 58 shortest and revised Level 59 shortest is **3 / 10** (low, but this does not by itself prove conceptual novelty). The previous shared opening of their longer routes is gone.
- The exact Level 59 Unity asset and editor-only longer route were changed. Added focused EditMode assertions for shortest-path proof, the blue helper's participation, at least six multi-piece taps and non-repetition with 58.
- **Do not regard this as release-certified or player-validated yet.** The simulator is independent, and no Unity runtime compilation or device tests have been executed during this continuation.

### New single-level repetition check
`SHIFT → Quality → Export Route Similarities 41-80` now reports **corridor-run risks** as well as duplicate-route pairs. A level is flagged when four or more consecutive recorded taps stay on one row or one column. This is a *screening heuristic*, not automatic design rejection.

In particular **Level 77** has a five-tap vertical progression that risks feeling like repeated manual advancement rather than a new puzzle. Experimental modifications that only changed route coordinates without changing that underlying idea were **rejected**, not saved.

### Next design work (before player tests)
1. Redesign the **69 / 72 / 77 / 78** family by changing the necessary **insight**, not only the board orientation or length. Prioritize 77's long same-lane sequence.
2. Review 41–60 difficulty and insight progression; promote 60 only if its two-gate chain proves satisfying by design and solver consistency.
3. Lock 80's unique double-signal culmination (already revised in v01) after checking that earlier levels do not preempt the same revelation.
4. Complete Chapter 3/4 visual purpose, chapter milestones and concise feedback without adding a distracting metagame.
5. Only after the design pass, run final blind human-playtest rounds, Unity/Android/iOS regression and Perfect Shift certification. No main merge or production release until checks pass.


### Further gameplay-quality finding: useful vs decorative Resonance

Replaying the recorded routes in a separate BoardManager-style simulator exposed a defect that ordinary pulse-count tests miss:

- **77:** The source resonance pulse occurs when there is **no normal piece waiting on the paired node**. The visual signal therefore does not cause a useful remote piece movement on the saved route. The recorded route also contains five consecutive blue-lane taps; only one of eight taps moves more than one piece. **Do not approve this design as-is.**
- **80:** Both recorded resonance signals **do** have actual remote receivers and drive remote movement. Nevertheless, only two of its eight recorded taps move multiple pieces; its chapter-finale feel should be reviewed after the core quality pass.

The editor-only `Post40QualityAudit.CountResonanceReceivers` now reconstructs normal-piece positions event-by-event in the authoritative BoardManager action log, so the generated Unity quality matrix distinguishes pulse events with an occupied remote partner from **empty** ones. Focused regression assertions document this gap at 77 and guard the productive channels at 80.

**Level 77 redesign target:** a non-obvious setup that makes the remote receiver essential, changes the gate decision at a meaningful moment, avoids a four-plus-tap straight corridor and ends with an understandable chain payoff. Pure coordinate shuffles and mechanically decorative signals are rejected even when they produce a valid shortest route. No new mechanism is needed.
