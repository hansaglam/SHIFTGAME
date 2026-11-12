# SHIFT — Level 41–80 Design Quality Pass v02

**Date:** 2026-11-12  
**Branch:** `feature/level41-80-design-quality-v02`  
**Based on:** `feature/chapter3-4-quality-pass-v01`  
**Status:** **ISOLATED CANDIDATE — do not merge to production or enable Perfect Shift.**

**Latest design outcome:** The latest continuation revised 59/60 and Chapter 4's 66, 69, 71, 72, 74, 76, 77, 78 and 79. Earlier open issues below are retained as historical investigation; the newest results are in the final section.

## Goal and protection

Improve memorability and originality, not the number of levels. Level 81–100 is explicitly out of scope. Levels 1–40, MORA, live campaign wiring, PlayerPrefs/save data, advertisement integrations, and existing certified Perfect Shift registry were not changed.

## Evidence-driven findings

- All 40 YAML LevelData candidates (41–80) were inspected for board dimensions, mechanics and recorded tap routes.
- **55 → 60:** The pre-v02 Level 60 reused **the first eight taps of Level 55 verbatim**. That is unacceptable for a chapter finale, even though Level 60 had extra moves and pieces.
- **58 / 59 (pre-continuation baseline):** The *longer* winning routes previously shared the first **six** taps. The later Level 59 revision below changes this; Level 58 itself is not modified.
- **69 / 72 / 77 / 78 (previous baseline):** Reused vertical-corridor/order-of-operations route family. The Level 77 rework described below breaks its straight-line motif; **69/72/78 still require design review**.
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
1. Redesign the remaining **69 / 72 / 78** corridor family by changing the necessary **insight**, not only the board orientation or length. Level 77 has a distinct new candidate below.
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


## Level 77 — The Return Signal (new design candidate)

**Previous issue:** Level 77's saved route had **8 taps**, only **1 multi-piece tap**, a decorative pulse with **no remote receiver**, and a **five-tap straight-column run**. The original blue lane mainly advanced through repetitive manual taps.

**New intended insight:** The red target's first action pushes the yellow non-target helper and enters the resonance source; this actually moves a waiting blue target remotely. The yellow helper then pushes blue onto the switch, opening the closed gate **on red's route**. Blue's movement allows red to finish, while the yellow helper subsequently pushes blue toward its own exit. The dependency is reciprocal across colors, not an arbitrary longer corridor.

| Candidate property | Previous draft | New draft |
|---|---:|---:|
| Board | 6×8 | 6×8 |
| Shortest recorded win (independent BFS) | 8 | 8 |
| Distinct longer win | 9 | 9 |
| Move budget | 10 | 9 |
| Multi-piece taps on recorded shortest route | 1 | **5** |
| Resonance pulses reaching and moving a waiting receiver | 0 | **1** |
| Longest same-row/same-column run of tap coordinates | 5 | **3** |
| Total BoardManager-style action events on shortest route | 20 | **29** |

**Recorded optimal:** `(4,1) (2,1) (1,2) (3,3) (3,5) (1,3) (1,4) (1,5)`

**Recorded 9-tap alternate:** `(3,1) (2,1) (4,1) (1,2) (3,3) (3,5) (1,3) (1,4) (1,5)`

These are editor-only verification data, not player-facing solution spoilers. They were replayed and independently searched with a Python reproduction of current `BoardManager` recursive pushing, turns, switches/gates, Resonance 2.0 and win-state rules. In the counterfactual model **with only remote Resonance movement disabled**, the shortest completion becomes **9** committed taps rather than 8. Thus the remote movement is genuinely useful to the optimal route; it is **not a mandatory prerequisite for every possible win**. This distinction matters for honest mechanic claims.

The chosen 8-tap route delivers red **before** the helper re-enters the switch and closes red's gate, avoiding visually confusing gate closure beneath the red target on the recorded solution. Its 9-tap alternative follows the same constraint.

### Source and protections

- Existing Unity asset filename/GUID preserved; updated serialized `Level77_FalseLead.asset` retains references, but its display title is `77 - The Return Signal`.
- Updated editor-only 77 alternate in `Chapter4ResonanceExtendedDrafts.cs`.
- `ChapterFinaleQualityTests` now enforces 8/9 solution structure, remote blue movement after the pulse, correct gate timing, five multi-piece reactions, no 4+ tap corridor and exact shortest-path proof in Unity.
- `Post40QualityAudit` now distinguishes **empty pulses**, **occupied but ineffective pulses**, and **productive remote movements**. Mere visual signals are never sufficient.
- **Not release-certified:** Unity compilation / authoritative solver, on-device UX, localization and human playtests still have not run. Keep the design branch isolated and Perfect rewards off.

### Remaining design priorities

1. Keep 77's reciprocal color-and-gate relationship unique; do not duplicate it in 78 or 80.
2. Rework **72** and **69** first where their corridors still repeat similar decisions; re-check 78 against the resulting family.
3. Preserve Level 80 as the chapter's distinct dual-signal, dual-gate climax rather than making 77 another finale.
4. Final real-player experience tests occur after the content-design phase, as requested; automated certification gates remain mandatory for release.


## Structural audit — Levels 69, 72, 78 (still unresolved)

The independent replay used the exact saved LevelData route, a BoardManager-style simulator and a counterfactual replay with **only remote signal movement disabled**. These three levels are **not accepted yet**:

| Level | Shortest | Multi-piece taps | Visible pulses | Productive remote moves | Shortest with remote movement disabled |
|---|---:|---:|---:|---:|---:|
| 69 | 6 | 2 | 3 | **0** | 6 |
| 72 | 6 | 1 | 2 | **0** | 6 |
| 78 | 8 | 4 | 3 | **0** | 8 |

These are **not merely cosmetic near-duplicates**: the same initial yellow/red/blue lanes, the 3,2 and 1,3 resonator pair, the switch at 1,4 and a closely related exit/gate sequence recur. The 69 and 72 routes can both be solved just as efficiently with remote signal movement disabled, as can 78. This independently supports the conclusion that remote signal movement is currently *not necessary for their optimal solutions*.

Research candidates for 72 that simply repositioned the receiver onto an already-occupied pad still resembled Level 77's new insight. They were **rejected rather than committed**: the next 72 revision should make preparation of an empty remote node a deliberate **setup decision**, rather than duplicating 77's initially ready receiver. The initial design search did not yield a candidate meeting the useful pulse, distinct insight and shorter/longer win constraints together.

Likewise, experimental 69 candidates that merely moved the gate onto the red lane reused the red/blue dependency from the revised 77. These remain unsaved experiments, not accepted designs.

**Editor QA now emits an explicit design-blocking error** when any Level 66–80 candidate has visible resonance pulses but zero confirmed remote-piece moves, even if a solvable shortest route exists. This prevents declaring all levels design-ready simply because the mechanical solver passes.

### Next revision brief

- **69:** Make the remote pulse causally meaningful using a simpler lesson than 77; avoid copying the exact red gate/blue receiver reciprocity.
- **72:** Require a genuine receiver-preparation moment *before* firing the source, with visible consequences and no straight-line filler. Do not accept a different-looking board with the same 77 solution.
- **78:** After 69 and 72 are distinct, use a late-stage *delayed consequence* and coordination idea—not a longer version of their original vertical lane.
- Keep all three as **design blockers**; no promotion to main or any Perfect certificates until redesigned and solver-certified.


## 12 November 2026 — Chapter 4 full functional-quality sweep (latest status)

**Objective:** Remove objectively decorative Resonance from the *recorded optimal solution*, while preserving different insights and valid manual +1-tap wins. This is a **design candidate** pass, not a completed player-quality or Unity certification.

### Independently replayed / searched results

The post-edit source candidate board for every level 66–80 was enumerated in a separate Python reproduction of the current `BoardManager` movement, switch/gate, rotator/direction, recursive push, Resonance and win rules. A **counterfactual** search disabled only the *remote receiver movement*, leaving pulses and other rules unchanged.

| Level | Recorded minimum | Without remote movement | Multi-piece taps on recorded optimum | Total pulses | Productive remote moves |
|---|---:|---:|---:|---:|---:|
| 66 | 5 | 6 | 3 | 2 | 1 |
| 67 | 5 | 6 | 1 | 3 | 1 |
| 68 | 6 | 7 | 4 | 4 | 1 |
| 69 | 6 | 7 | 3 | 1 | 1 |
| 70 | 6 | 8 | 1 | 2 | 2 |
| 71 | 6 | 7 | 1 | 1 | 1 |
| 72 | 6 | 7 | 2 | 1 | 1 |
| 73 | 7 | 8 | 1 | 3 | 1 |
| 74 | 6 | 7 | 1 | 2 | 1 |
| 75 | 7 | 8 | 1 | 1 | 1 |
| 76 | 7 | 8 | 2 | 2 | 1 |
| 77 | 8 | 9 | 5 | 1 | 1 |
| 78 | 8 | 9 | 7 | 3 | 1 |
| 79 | 8 | 9 | 4 | 1 | 1 |
| 80 | 8 | 9 | 2 | 2 | 2 |

**Candidate result: 15/15 levels have a real remote movement in the recorded shortest solution, and disabling remote movement worsens the computed minimum for all 15.** This is much stronger than a simple “pulse animation exists” check. It is still *not a Unity runtime certification*; Unity's `CampaignCertification` and EditMode tests remain required, and player delight has not been measured.

### Concrete changes and intentionally different solution ideas

- **66 — Signal Lock:** Moved blue onto the waiting resonance receiver and shifted the red exit. The pulse now opens the blue gate remotely. Introductory immediate-echo payoff; 5 versus 6 taps.
- **69 — Distant Arrival:** The yellow helper corrects a wrong-facing red piece while the same chain awakens the distant blue route; the switch opens the red path. 6 versus 7 taps.
- **71 — Delayed Relay:** Moving the partner pad beneath the blue starting piece makes a *delayed remote step* beneficial. The longer alternative explicitly advances blue manually **before** the pulse. 6 versus 7 taps, but a quieter consolidation level with only one multi-piece tap.
- **72 — Prepare the Echo:** Yellow first opens a gate blocking the receiver's path; a later red signal then moves the waiting blue piece for free. Distinct **prepare-before-signal** concept. 6 versus 7 taps.
- **74 — Crossed Priority:** Red starts the signal; distant blue is redirected downward, activates the switch and opens a gate across red's path. Longer route may manually reposition blue before the signal. 6 versus 7 taps; remains a lower-chain, higher-planning candidate.
- **76 — Held in Resonance:** Yellow pushes red **onto** the receiver, where a closed gate prevents it from moving away. The same yellow reaction turns onto the switch and opens the gate. Blue's later source pulse releases red remotely. Alternative can move red manually. 7 versus 8 taps.
- **77 — The Return Signal:** Previously revised reciprocal pulse/gate design preserved. Remote blue opens red's route, 5 multi-piece taps, 8 versus 9.
- **78 — The Deferred Handoff:** New four-normal design. Red's source pulse pushes a yellow non-target helper onto the switch, opening a gate directly ahead. Later blue frees that helper through a **neutral shared exit**, then follows into the same exit. Gate closure is timed for an **unoccupied** gate; saved optimal route has 7 multi-piece taps, 8 versus 9.
- **79 — The Last Key:** Blue first triggers remote movement of red **into yellow's push lane**. Yellow's later chain delivers red. The slower alternative positions red manually before blue fires, establishing real Perfect-vs-normal discovery. 8 versus 9.
- **80 — Heart of Resonance:** No direct asset revision in this continuation; the v01 two-channel/two-gate/three-color finale still has productive remote movement in both channels. Remains a final payoff evaluation priority.

All altered levels preserve their original `.asset` file paths and Unity GUIDs, avoiding scene-reference churn. Updated recorded optimum routes and editor-only distinct longer winning routes for each edited level. The early 41–40 released content, MORA, save schema, production scene, shipped Perfect registry and app builds are untouched.

### Quality regressions and remaining design judgement

- New EditMode checks cover exact shortest and +1 route contracts, actual remotely moved pieces, minimum reaction density of revised boards, neutral shared-exit behavior and timing of gate closure. An **all-66–80 actual remote movement** regression prevents return to purely decorative signals.
- `SHIFT → Quality → Export Level 41-80 Decision Matrix` emits a **design-blocking error** if any post-65 candidate has signals but no productive remote movement; it also flags excessive simple-move sequences for editorial review.
- **Still under design review:** 67, 70, 71, 73, 74, 75 have only one multi-piece tap on the recorded optimum. Some may be legitimate pacing/teaching steps, but a long block of similar simple taps could still feel like filler. They should be judged as a group during final design pass; not automatically given a quality grade.
- **78:** Two of its three signal events have no remote receiver; only the first is productive. This is consistent with the per-tap Resonance rule, but visual/audio feedback must not falsely imply another movement.
- **80:** Has two productive signals and two gate dependencies but only two multi-piece taps on its eight-tap optimum. Evaluate chapter-ending satisfaction separately; do not manufacture a “finale” merely by extra moves.
- The *static* claim “15/15 are now causally useful” does not establish puzzle memorability, chapter atmosphere or retention. Final blind playtests are deliberately deferred, per the owner's direction.

### Release boundary

Keep `feature/level41-80-design-quality-v02` and PR #2 **draft**. Final conditions remain: Unity 6.3 LTS compilation, `ChapterFinaleQualityTests`, `ResonanceExtendedDraftTests`, other mechanic suites, Chapter 3 **20/20**, Chapter 4 **5/5 + 15/15**, complete 41–80 decision matrix **40/40**, phone readability, reduced motion, EN/TR and user data migration. Never enable or fabricate Perfect Shift certificates before exact Unity proof and design acceptance.
