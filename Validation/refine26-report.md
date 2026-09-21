# SHIFT — targeted V2 refinement: 26, 27, 30

Only Levels **26, 27 and 30** are refined. Levels 1–25, 28–29 and 31–40, BoardManager, progression/save, runtime presentation and the final visual identity are frozen. No mechanic or piece type is added. Levels 31–35 are not started.

## A. Level 26: the support itself needs preparation

V1's one-step lane clearance is replaced by a compact **4×4 two-level elbow**. Red can start moving, Blue appears to clear its lane, and Yellow can move sideways: all three are committed legal openings. Only Yellow's sideways departure preserves the solution.

Blue sits below Yellow. Moving Blue first pushes Yellow into the upper bend and down onto the red exit approach. Moving Red first pushes Blue into the lower bend and onto that same approach. Correct preparation is therefore **vacate the upper landing → free the lower lane → advance the target**. It uses two support moves and two target moves, rather than one support move followed by three linear target taps.

This is still a four-move breather: no box, switch, gate or state parity. Its random-valid completion probability rises from 1.2346% to **4.1667%**, even though its opening geometry is less exposed. That is a deliberate demonstration that lower random probability was not the goal.

## B. Level 27: two parking destinations, one future route

The horizontal V1 parking arrangement becomes a **4×5 vertical fork with a hooked lower return**. The box starts away from the geometric center. A northward push sends it into a long pocket; an eastward push redirects it into an apparently empty lower recess. Both are real committed actions, not decorative choices.

Only the north pocket preserves the future return route. Blue must push twice: after the first push it occupies the shared crossing itself, and after the second it finally vacates that crossing. Advancing Red between those two pushes redirects Blue into the return route. The distinction is **choose parking → finish parking → vacate access**, not simply “move the box.”

The five-move budget remains. Two winning sequences exist; they share the same opening and preparation, then differ in whether Red takes its last step or Yellow pushes it into the finishing bend. Both are legitimate. Final reaction depth improves from two to **four** on the recorded route.

## C. Level 30: preserve access while preparing state

The seven-move solution, two targets, switch/gate preparation and final **MEGA SHIFT x8** remain. The old disconnected upper-left Yellow trap is replaced by a **lower entrant at (3,0)** sharing the box's staging cell at (3,1). An overflow cell at (5,1) and a refuge at (3,3) make mistakes recoverable rather than automatically permanent.

There are now two concrete preparation errors:

1. **Entry too early:** Yellow occupies box staging at (3,1). The box pushes it successively into the intended parking bay (4,1), then overflow (5,1). The original plan can still finish in **eight moves**, one over budget.
2. **Entry after opening the gate:** the lower entrant pushes the other Yellow upward into the now-open gate at (3,2). The state is correct but occupancy is wrong. Moving the blocking Yellow into refuge (3,3) restores access; completion takes **nine moves**, two over budget.

Thus the additional conflict is reserving staging/parking/access while changing state. Correct play does **not** touch the lower entrant. The main solution's coordinates intentionally remain unchanged; the refinement changes the competing plan and its consequences, not a successful sequence merely to make it longer. Existing premature Red moves remain bad directional choices, while the new tempting preparation mistakes have explicit recoveries.

## D–E. Aha moments and decision density

| Level | Intended realization | Distinct authored decisions | Mixed-outcome graph states on known solution |
|---|---|---:|---:|
| 26 | “The piece that frees Red's lane needs its own landing cleared first.” | **3:** vacate the upper landing; let Blue retain its own direction; stop using Blue once Red's lane is ready | 4 |
| 27 | “The empty recess is my future route, and parking is not finished while Blue occupies the crossing.” | **3:** choose north parking; complete the second push before advancing Red; preserve the lower return until delivery | 4 |
| 30 | “Opening the gate is not enough; my preparation must leave the staging bay and gate unoccupied.” | **5:** reserve staging; vacate the switch; restore state; preserve gate access; lead the paired targets from the rear | 6 |

Authored decisions are design judgments, not telemetry measurements. Repeated “do not tap the same wrong piece” alternatives are not additional insights. Level 30's second step has only one committed legal action and is **not** counted as a choice. Tests keep authored themes separate from graph-state counts.

## F–G–I–J. Exact analysis

| Level | Proven minimum / budget | Valid / winning openings | Winning sequences within budget | Exact random-valid completion | Reachable states | Final / route-max / all-state-max depth |
|---|---:|---:|---:|---:|---:|---:|
| 26 V2 | **4 / 4** | **3 / 1** | 1 | **4.166667% (1/24)** | 11 | **6 / 6 / 8** |
| 27 V2 | **5 / 5** | **3 / 1** | 2 | **2.777778% (1/36)** | 63 | **4 / 4 / 7** |
| 30 V2 | **7 / 7** | **4 / 1** | 1 | **0.065104% (1/1536)** | 646 | **8 / 9 / 11** |

Coordinates use a bottom-left origin. Blocked zero-cost taps are excluded.

| Level | Committed opening taps | Only winning opening |
|---|---|---|
| 26 | Red (0,1), Blue (1,1), Yellow (1,2) | Yellow (1,2) |
| 27 | Red (0,2), Blue (1,1), Yellow (3,2) | Blue (1,1) |
| 30 | Rear Red (0,2), front Red (1,2), side Yellow (1,1), lower Yellow (3,0) | Side Yellow (1,1) |

The unchanged `PuzzleBatchAnalysis` replays authoritative BoardManager transitions, enumerating complete piece/gate state plus remaining budget. Its policy uniformly selects a committed legal action at each state. These are exact finite-horizon probabilities, **not Monte Carlo or player success estimates**. Distinct paths are counted separately even when states merge. The existing independent bounded search proves there is no shorter solution; bounds throw rather than publish a partial certificate.

All depths count BoardManager action events, not pieces or animation duration. The higher global maxima can arise from losing branches; they are not presented as the final payoff.

### Recorded solutions

- **26:** (1,2), (1,1), (0,1), (1,1).
- **27:** (1,1), (1,2), (0,2), (1,2), (2,1). Its alternative replaces the last tap with Yellow (3,2), pushing Red into the same finishing route.
- **30:** (1,1), (2,1), (2,2), (0,2), (1,2), (2,2), (3,2).

### Level 30 recovery proofs

- Early-entry eight-move route: **(3,0)**, then the complete seven-move known solution. The misplaced Yellow ends in overflow at (5,1).
- Late-entry nine-move route: (1,1), (2,1), (2,2), **(3,0), (3,2)**, (0,2), (1,2), (2,2), (3,2). The obstructing Yellow moves to refuge at (3,3).

Tests independently reject completion within seven after the first mistake and within eight after the second prefix, then replay the stated recoveries under diagnostic extended budgets. **The shipped budget stays seven.** These tests establish visible move-cost penalties, not recoverability claims made without a bound.

## H. Important wrong choices

| Level | Mistake | Observable consequence |
|---|---|---|
| 26 | Red first | Blue is pushed through the lower arrows and stops at (2,2), facing the red-only exit. |
| 26 | Blue first | Yellow is pushed through the upper arrows into (2,2), blocking the same approach. |
| 26 | Continue Blue after clearing the lower lane | Blue enters the upper branch and competes for the exit approach; clearing the lane did not authorize clearing every support. |
| 27 | Park east using Red | The box rests at (2,1), where Red later needs to descend. A box cannot pass the red exit. |
| 27 | Red after only one north push | Blue still occupies (1,2); Red pushes it into the lower return, replacing a box obstruction with a wrong-color obstruction. |
| 27 | Send Yellow into the hooked return early | Yellow consumes the same limited return space before the target arrives. |
| 30 | Move the lower entrant before parking | The staging cell is occupied and its occupant is displaced into the parking bay. Recovery costs one extra move. |
| 30 | Use the lower entrant after opening the gate | Yellow occupies the open gate. Clearing it into refuge costs two moves in total. |

## Quality gate — explicit design review

These answers are reasoned review judgments, not evidence that a human playtest has already occurred.

| Question | 26 V2 | 27 V2 | 30 V2 |
|---|---|---|---|
| Is the opening visually too obvious? | Improved: Red, direct Blue clearance and sideways Yellow departure all commit. The real first move clears another support's destination. | Improved: north pocket and east recess both accept the box; future use distinguishes them. | Improved locally: the lower entrance invites “clear this first,” but shares parking resources. The established main sequence is retained. |
| Is the wrong choice understandable? | The wrong-colored piece visibly occupies the exit approach. | A parked box or redirected Blue occupies the return path. | Gate state can be visibly open while Yellow blocks it; explicit recoveries demonstrate the lost moves. |
| Is there an actual aha? | Prepare the support before the target. | A parking cell may be route space, and the operator must leave too. | Preserve occupancy as well as state; do not clear every apparently movable piece. |
| Is topology distinct? | Compact 4×4 upper/lower elbow. | Tall 4×5 fork and hooked return. | 6×5 two-zone staging, gate and high-corner relay with lower access. |
| Would a human likely pause? | Likely to compare the three starts; still only four moves and no gate parity. | Likely to inspect where each parking option leaves Blue and the box. | Likely to inspect the competing entrance and distinguish open from usable. Requires manual confirmation. |
| Does the final reaction feel earned? | Two support preparations release the six-event target turn. | Parking and vacating release a four-event lower turn. | Reserved access and state preparation retain the eight-event MEGA SHIFT finish. |

The three candidates were checked against these causal differences before acceptance. The acceptance basis is changed dependency/space management and demonstrated consequences, not the random percentages alone. Level 26's percentage actually increases; Level 27's stays unchanged.

## K. Files changed

- Only `Data/Levels/Level26_ThreeEntries.asset`, `Level27_BoxDoesNotPress.asset`, `Level30_ClearThenOpen.asset`; original filenames and GUIDs remain.
- Only catalog entries 26, 27, 30 in `Resources/LevelDesignCatalog.asset`. Level 27 now records its four-event payoff. All other 37 entries are frozen.
- Three new fingerprint certificates in `Scripts/Levels/VerifiedOptimality.cs`; existing mappings retained.
- `Editor/Batch26Review.cs`: update only these three authored decision-theme entries; 28/29 remain unchanged.
- `Tests/Editor/Batch26Tests.cs`: revise only these three exact expected graphs and old layout-specific assertions. All Level 28/29, boundary progression, mastery and visual checks remain.
- New `Refinement26Tests.cs`, `Refinement26FrozenFiles.txt`, `Refinement26CatalogFrozen.txt` and metas. Additional assertions cover the upper landing, two parking directions, vacating order and both recoverable Level 30 mistakes.
- Historical frozen manifests refresh only the three authorized puzzle/certificate/catalog hashes; no checks are removed. New protection expands to **37 untouched levels / 87 protected files**.
- Scoped `Validation/author_refine26.py`, `freeze_refine26.py`, baseline/catalog snapshots, analysis JSON, this report, test evidence and four captures; README notes.

No BoardManager, analysis-algorithm, gameplay, progression/save, scene or runtime visual code changes. Historical batch authoring scripts must not be reapplied over this refinement.

## L. Tests/results

**Full Unity suite: 181/181 passed, zero failed/skipped, zero C# compiler errors/warnings.** The existing 175 cases remain, with six new cases for targeted V2 properties, recoverability, expanded frozen scope and captures. Existing actual-control progression covers **25→26, 27→28, 29→30 and 30→31**. Ordered/unique reference and restoration regressions remain intact.

Evidence: [test results](refine26-results.xml), [Unity log](refine26-unity.log), [preservation and delivery checks](refine26/preservation.json). All 87 protected files match their baseline, including all 37 untouched level assets. Delivered code/assets match the tested isolated project. Only documentation and copied validation artifacts were finalized after the run; no code or puzzle changes followed it.

## M. Screenshots

Four actual Unity captures were produced and visually reviewed, each **1080×1920**:

- [Level 26 V2 start](refine26/refine26-level26-start.png)
- [Level 27 V2 start](refine26/refine26-level27-start.png)
- [Level 30 V2 start](refine26/refine26-level30-start.png)
- [Level 30 MEGA SHIFT x8 / Perfect Shift](refine26/refine26-level30-payoff.png)

Current identity, piece directions, instruction cards and controls remain readable. The three starts have distinct silhouettes. Visual review supports the intended spatial relationships; human opening-choice plausibility and physical-device comfort remain manual checks.

## N. Recommendation before 31–35

**Ready for focused manual review, not an automatic move to the next batch.** On design inspection, 26 and 27 address their exposed first-move/parking problems, and 30 adds a recoverable occupancy-versus-state conflict while preserving its payoff. Ask players why they chose their first action and whether the resulting obstruction makes sense without explanation.

Specifically check that 26 remains a breather despite its richer arrow geometry, that both parking choices in 27 look plausible, and that 30's lower entrant is perceived as a real preparation temptation rather than an obviously irrelevant piece. If that last choice still reads as a decoy, revise its geometry rather than celebrating a lower random percentage. Proceed to 31–35 only after this manual review; no later level has been changed.
