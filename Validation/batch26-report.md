# SHIFT — Levels 26–30 review

This batch changes only the five puzzle assets numbered 26–30 and their authoring/verification metadata. Levels 1–25 and 31–40, BoardManager, core rules, progression/save systems and the final visual identity are preserved. Level 25 is the difficulty reference, not the layout template. No new mechanic, Hint, Undo, SDK or runtime solver was introduced.

## A–C. Old/new designs, identity and intended realization

| Level | Previous design | New identity and purpose | Intended aha |
|---|---|---|---|
| 26 | Three pad entries and one gate; 5-move optimum, 12.5% random completion | **Let It Wait**, a 5×4 asymmetric elbow. Four moves, no switch, gate or box. A waiting crossing piece must depart under its own direction. | “Pushing the target now changes the other piece's direction. Let the lane clear first.” |
| 27 | Box/pad teaching with one committed action at each step; 5 moves, 100% random completion | **Borrowed Space**, a 5×4 crossing and lateral parking bay. Move the box sideways, then vacate the borrowed crossing. | “Moving the box is not enough; its operator is still in the way.” |
| 28 | Direct switch/arrow/gate path; 3 moves, 100% random completion | **Beyond the Turn**, a 5×5 folded route with an upper entry and lower finishing bend. A push changes the target's approach direction before Direction + Rotator transformations. | “The same target needs a different approach, and the first turn is not its destination.” |
| 29 | Single pad/rotator/gate path; 3 moves, 100% random completion | **One Move, More Work**, a 6×4 straight convoy route with an independent upper loop and lower return. Generous eight-move budget supports real alternative completions. | “Both plans work, but shared pushes move several targets with one tap.” |
| 30 | Lower switch preparation and high-corner delivery; 5 moves, 6.25% random completion | **Release the Relay**, a 6×5 two-zone board. Park a box, vacate its switch, restore gate state with the corridor occupant, then release two targets through three turns. | “The gate being open does not mean the board is ready.” |

Level 26 is a breather in rule interactions and route length, not a claim of greater random success: its exact random rate is below Level 25's. That distinction matters when playtesting perceived difficulty.

## D. Decision density — human themes versus graph counts

| Level | Authored decision themes | Mixed-outcome states on recorded route | Different-cost viable-choice states |
|---|---|---:|---:|
| 26 | Clear before redirecting; preserve the exit approach from the wrong color | 4 | 0 |
| 27 | Lateral parking versus exit-lane obstruction; vacate the crossing after parking; keep the descending competitor out of the shared landing | 4 | 0 |
| 28 | North entry versus short east step; hand off to target or continue the supporting push after the upper bend; predict the lower wrong-color trap | 5 | 0 |
| 29 | Independent loop versus group redirection; merge the returning target or deliver separately; preserve the remaining convoy rather than opening gaps | 0 | 4 |
| 30 | Prepare despite the open gate; leave the switch accessible; restore state with the corridor occupant; lead with the rear target for shared delivery | 7 | 0 |

The authored theme counts are **2, 3, 3, 3, 4**. They are review hypotheses, not measured cognitive decisions. In particular, the three early support steps in Level 28 repeat one plan and are **not three separate insights**. Its handoff/continued-push alternatives can both win in five moves while producing different intermediate occupancy. Whether that feels like a meaningful third decision needs manual testing. Level 29 deliberately has no losing alternative on the recorded route within its full budget; its meaningful choices concern cost and route, so the old win/loss-only counter would misleadingly say zero.

Tests check metadata completeness and exact graph properties separately. They cannot establish human plausibility, retention or satisfaction.

## E–G. Openings, exact probabilities and verified optima

Coordinates use a bottom-left origin. Openings below spend a move; free blocked taps are excluded.

| Level | Valid opening taps | Openings with a completion | Minimum / budget | Winning sequences within budget | Exact random-valid completion | Exact optimal discovery | Reachable states |
|---|---|---:|---:|---:|---:|---:|---:|
| 26 | Red (0,2), Blue (1,2), Yellow (0,1) | 1 / 3 | 4 / 4 | 1 | 1.234568% (1/81) | 1.234568% | 29 |
| 27 | Red (2,3), Blue (1,2), Yellow (3,3) | 1 / 3 | 5 / 5 | 1 | 2.777778% (1/36) | 2.777778% | 44 |
| 28 | Red (1,3), Blue (1,0), Yellow (2,0) | 1 / 3 | 5 / 5 | 3 | 1.234568% (1/81) | 1.234568% | 24 |
| 29 | Rear Red (0,1), middle Red (1,1), front Red (2,1) | 3 / 3 | 5 / 8 | 116 | 37.019890% | **1.851852% (1/54)** | 146 |
| 30 | Rear Red (0,2), front Red (1,2), Yellow (1,1), Yellow (0,3) | 1 / 4 | 7 / 7 | 1 | 0.115741% (1/864) | 0.115741% | 265 |

Level 29's 37% completion rate is intentional: experimentation can finish. The requested resistance to *optimal discovery* is under 5%; it is not appropriate to present 1.85% as its ordinary completion probability.

### Reproducible method

`PuzzleBatchAnalysis` enumerates the finite state graph by replaying the **unmodified BoardManager**. Its key includes piece identity, active state, position, direction, gate state and remaining moves. Uniform random policy selects among committed legal actions at each state. A winning terminal has probability 1, a failed terminal 0; each branch averages child probabilities. Distinct action sequences remain distinct even when states merge.

Optimal discovery sums only edges that preserve the node's minimum distance, but still divides by **all** legal actions. A separate clone with budget equal to the proved optimum reproduces this probability. The existing independent `DesignValidation.CanWin` search excludes every shorter route. Search bounds throw instead of issuing partial certificates. There is no Monte Carlo seed or confidence interval: these are exact policy probabilities, not player measurements.

Global maximum depth covers all enumerated committed reactions within budget; route maximum covers the recorded solution. Neither denotes a count of pieces. `VerifiedOptimality` certificates bind the entire puzzle data and budget to a fingerprint.

### Recorded optimal solutions

| Level | Taps |
|---|---|
| 26 | (1,2) → (0,2) → (1,2) → (2,2) |
| 27 | (1,2) → (2,2) → (2,3) → (2,2) → (3,1) |
| 28 | (1,0) → (1,1) → (1,2) → (2,3) → (2,2) |
| 29 | (0,1) → (1,1) → (2,1) → (3,1) → (4,1) |
| 30 | (1,1) → (2,1) → (2,2) → (0,2) → (1,2) → (2,2) → (3,2) |

## H. Wrong choices and visible consequences

- **26:** Red first pushes Blue sideways, overwriting its downward direction and placing it in the target lane. No winning continuation within budget. Yellow first pushes the target upward into the dead-end pocket. The blocked exit remains color-specific, as before.
- **27:** Red first pushes the box down onto the direction tile and into the exit lane at (3,1). A box cannot enter the red exit. Clearing the box laterally is necessary, and Blue must subsequently leave (2,2). The parking bay and descending competitor make occupancy visible.
- **28:** Red's direct east step abandons the north-entry plan. Yellow first follows the lower arrow/rotator chain but stops at (3,0), unable to enter the red-only exit. The same transformations are visible for both colors; no hidden trigger is involved.
- **29:** The upper loop is **not a failure**. It completes in eight moves if delivered separately, seven if merged into the convoy. Breaking the convoy can cost efficiency, but the budget permits it. Only the five-move route earns Perfect Shift.
- **30:** Moving either target before preparation pushes Blue right into the open gate, changing Blue's direction and losing the switch operator. Correct preparation first closes the gate when Yellow enters the exposed switch; Yellow must leave before Blue can enter it to reopen the gate. The rear target then shares movement with the front target.

Early mistakes can create dead-ends in 26–28 and 30. Their directional/occupancy consequences are visible, but comprehensibility and willingness to retry require player observation. Low random probability alone does not establish fairness.

## I. Finishing and maximum reaction depth

| Level | Final depth | Recorded-route maximum | All-reachable maximum |
|---|---:|---:|---:|
| 26 | 6 | 6 | 6 |
| 27 | 2 | 3 | 5 |
| 28 | 8 | 8 | 9 |
| 29 | 4 | 6 | 18 |
| 30 | 8 | 9 | 9 |

These are existing action-event depths. Level 30's final move is Move → Turn → Move → Turn → Move → Turn → Move → Deliver, classified **MEGA SHIFT** by the existing presentation. Its preceding shared push reaches nine events. Level 29's independent detour can produce an 18-event reaction; this is a real route characteristic, not suppressed to make Level 30 appear numerically largest. Mastery there rewards efficiency, not maximal spectacle.

## J. Level 29 alternate routes

- **Five, Perfect:** recorded route above; shared pushes redirect the upward-facing front target onto the main lane and keep the convoy compressed.
- **Seven, normal:** (2,1), (2,2), (0,1), (1,1), (2,1), (3,1), (4,1). Send the front target through the upper loop, then merge it at (4,1) with a shared push.
- **Eight, normal:** (2,1), (2,2), (4,1), (0,1), (1,1), (2,1), (3,1), (4,1). Deliver the front target independently through the lower return, then move the remaining pair.

All three routes are tested against authoritative state and actual telemetry. The eight-move route fits the shipped budget exactly. Five-move optimality is independently proved; seven/eight are valid examples, not claims that they are the only alternatives.

## K. Presentation and game feel

No presentation code or timing changed. The locked identity and existing direction/rotator, switch/gate, delivery, chain-tier and mastery feedback are reused. Authored payoff metadata now identifies Levels 26, 28, 29 and 30 at depths 6, 8, 4 and 8 respectively. This does not change reaction timing or rules.

No additional particles, shaking, overlays or input blockers were necessary. New UI regression coverage checks existing decorative icons remain raycast-transparent, the Level 29 normal/Perfect distinction, and the Level 30 MEGA SHIFT with Reduced Motion enabled. Existing presentation/Reduced Motion tests remain in the full suite.

## L. Instruction text

All five instructions are conceptual and omit named-piece order:

| Level | New instruction |
|---|---|
| 26 | A clear lane is worth waiting for. |
| 27 | Moving aside is only half the plan. |
| 28 | Look past the first turn. |
| 29 | One route does more work. |
| 30 | Prepare the board, then release it. |

They replace the previous explicit triple-toggle, box-pad, arrow-gate, rotator-pad and lower-alcove teaching prompts. All out-of-scope instructions are untouched.

## M. Changed files and preservation

- Five existing `Data/Levels/Level26_ThreeEntries.asset` through `Level30_ClearThenOpen.asset` assets. Original filenames/GUIDs retained for scene/reference stability.
- Only catalog entries 26–30 in `Resources/LevelDesignCatalog.asset`; five new fingerprint certificates in `Scripts/Levels/VerifiedOptimality.cs`.
- `Editor/PuzzleBatchAnalysis.cs`: add exact optimal-discovery probability, viable cost comparisons and global/route max depth; retain original solve probability and win/loss counters.
- New `Editor/Batch26Review.cs`, `Tests/Editor/Batch26Tests.cs`, `Batch26FrozenFiles.txt`, `Batch26CatalogFrozen.txt` and metas.
- `LevelDesignTests.cs` / `ChapterTwoTests.cs`: update Level 30's authored expectations from five to seven moves and payoff four to eight, retaining independent searches.
- Existing frozen manifests: refresh only authorized level/certificate/catalog entries. The previous batch's protected references remain covered at their new baseline; no assertion is removed.
- Scoped authoring script, baseline snapshots, C# harness, analysis JSON, report, test evidence and screenshots under `Validation`; README batch notes.

**Pre-existing scene edit preserved:** before this task, the user's scene had `selectedLevel: 21` instead of the earlier frozen `15`. The scene itself was not edited. Only that previously changed Inspector-selection snapshot is acknowledged in the two existing manifests after verifying the exact single-field difference. All 40 reference entries remain unchanged.

New preservation checks freeze 85 files, all 35 out-of-scope level assets, and all 35 out-of-scope catalog entries. BoardManager, scene, progression, saves, telemetry, runtime UI, board art and current identity are included.

## N. Tests/results

**Full Unity suite: 175/175 passed, zero failed/skipped, zero C# compiler errors/warnings**, using Unity 6000.3.23f1 on 2026-09-21. All previous 163 cases remain, plus 12 new cases. New coverage includes five exact graphs/minima, independent shorter-route searches, wrong-choice consequences, three valid Level 29 routes with Perfect Shift discrimination, decision metadata, reaction thresholds, protected hashes, real 25→26 and 30→31 progression, Reduced Motion and eight UI captures.

Evidence: [full test results](batch26-results.xml), [Unity log](batch26-unity.log), [preservation and delivery comparison](batch26/preservation.json). The final main-project code/assets match the tested isolated project; documentation and copied validation artifacts are finalized separately. All 85 protected hashes and 35 out-of-scope catalog entries match. The existing forty-reference uniqueness/order/restoration and presentation regressions also pass.

## O. Screenshots

Eight actual Unity captures were produced and visually reviewed, all **1080×1920**, using the current final identity. They are game renders, not illustrations or mockups.

- Starts: [26](batch26/batch26-level26.png), [27](batch26/batch26-level27.png), [28](batch26/batch26-level28.png), [29](batch26/batch26-level29.png), [30](batch26/batch26-level30.png).
- Level 29: [normal eight-move completion](batch26/batch26-level29-normal.png), [five-move Perfect Shift](batch26/batch26-level29-perfect.png).
- Level 30: [MEGA SHIFT x8 and Perfect Shift](batch26/batch26-level30-payoff.png), with Reduced Motion enabled.

The screenshots preserve the sculpted dark blocked areas, readable piece directions, conceptual instructions and current control layout. Physical-device comfort/performance remains a manual check.

## P. Before Levels 31–35

**Stop at this batch for manual playtesting.** Observe first taps, retries and explanations of mistakes without giving solutions. Check that 26 feels like relief despite its low random probability; 27 communicates that parking and vacating are separate; 28 encourages prediction rather than three rote support taps; 29 invites a second attempt after a legitimate slower completion; and 30's temporarily closing gate reads as preparation rather than contradiction.

Do not infer retention from graph statistics. In particular, validate Level 28's third decision theme and whether the strict budgets in 26–28 feel fair. Test tap comfort on a physical device. Levels 31–40 remain untouched; do not start the next batch until this one is reviewed.
