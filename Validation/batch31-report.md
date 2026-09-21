# SHIFT — Levels 31–35 design review

Only campaign Levels **31–35** are redesigned. Levels 1–30 (including locked 26/27/30 V2), 36–40, BoardManager, core rules, progression/save, telemetry, scene references and the final identity remain unchanged. No new mechanic, piece type, SDK, Hint, Undo or presentation effect is introduced. The next batch is not started.

## A–D. Old/new design, identity, player question and aha

| Level | Old design and measured baseline | New identity | Player question | Intended realization |
|---|---|---|---|---|
| 31 | One pad/two gates, linear four-move solution; 100% random-valid completion | **Across the Divide**: 6×4 upper workshop and lower delivery lane, connected by one occupied choke. Entirely spatial dependency, no switches. | “What changes in the other room?” | Parking the box is not enough: the workshop operator must vacate the receiving cell before the choke occupant can leave. |
| 32 | Two opposite gate-state rooms, eight-move minimum; 75% random completion | **Return Changed**: 5×4 loop around a central return landing. Leave the original junction and return with the opposite direction; temporary parking must be cleared. | “Why leave the place I need to return to?” | Moving away from the exit is how the target returns to its own starting cell facing the useful direction. |
| 33 | Shared straight delivery gate, five-move solution; 1.851852% random completion | **A Moving Pair**: 5×4 perpendicular rendezvous with a shared descending route. Two reds begin facing different directions rather than as a ready convoy. | “How do these targets help each other?” | The arriving target redirects the waiting target; subsequent shared pushes deliver one while advancing the other. |
| 34 | Corridor preparation, five moves; 9.375% random completion | **The Easy Way**: 4×5 outer arc and lower return. The same rotator sends the target to different locations depending on approach direction. | “Where will this route actually end?” | The short east entry strands the target. Approach from above, return left, and clear the landing used during setup. |
| 35 | Two channel controls, five moves; 10.416667% random completion | **Assemble the Release**: 6×4 separate feeders, firing row and offset exit. Stage two targets, turn a launcher into alignment, set the gate and trigger the constructed chain. | “How do I assemble the final chain?” | The pieces must become a machine before the launcher can fire; the gate can be a staging stop before it becomes an opening. |

Only Level 35 uses a gate. This batch deliberately shifts emphasis from toggle sequences toward position, direction and interacting targets. Level 30 is retained as a quality reference, not reused as a layout.

## E–F. First mistakes, teaching and decision density

| Level | Likely first mistake | What it teaches | Authored decision themes | Mixed-outcome graph states on solution |
|---|---|---|---:|---:|
| 31 | Advance Red, pushing Blue into the delivery room | A pushed support loses its original escape direction; solve the receiving space first | **3:** other-room preparation, complete parking/vacating, preserve color-specific delivery access | 6 |
| 32 | Treat Blue's first parking cell as permanent | The outward journey creates a return-space obligation; the target revisits its initial junction with a different direction | **3:** leave the junction, use temporary parking, clear the return landing | 6 |
| 33 | Move the lower Yellow into the crossing | A spare-looking crossing is needed by the departing support and both targets | **3:** rendezvous, preserve crossing, use shared redirection/push | 5 |
| 34 | Tap Red directly toward the nearby rotator | Rotator behavior depends on arrival direction, not the symbol alone | **4:** reject direct entry, lift into outer arc, clear return cell, predict reverse approach/lower turns | 5 |
| 35 | Move Blue before staging the first target | A launcher that enters its firing position too early consumes a target's landing | **5:** feeder staging, launcher alignment, gate as stop, state before trigger, keep operator out of delivery approach | 8 |

The authored counts describe distinct reasoning themes, not independent statistically measured player decisions. Repeated rejection of one decoy is not a new insight. In particular, Level 32 allows Blue's two clearance steps to occur consecutively or around Red's first move; the required reversal is **Red's physical return through its original junction**, not a forced chronological claim about the support. There is no switch parity in that puzzle.

The graph counter counts states with both winning and budget-losing committed actions. It is useful regression metadata but does not establish human plausibility or retention. Manual review must verify the themes, especially whether Level 32's returning target reads as deliberate reversal and whether Level 35's feeder staging feels like construction rather than chores.

## G–I. Openings, exact probabilities and proved minima

Coordinates are zero-based from bottom-left. Free blocked taps and cancelled reactions are excluded.

| Level | Valid / winning openings | Winning sequences within budget | Exact random-valid completion | Minimum / budget | Reachable states |
|---|---:|---:|---:|---:|---:|
| 31 | 3 / 1 | 1 | **0.462963% (1/216)** | **6 / 6** | 52 |
| 32 | 3 / 1 | 2 | **1.157407% (5/432)** | **6 / 6** | 78 |
| 33 | 3 / 2 | 2 | **1.234568% (1/81)** | **6 / 6** | 94 |
| 34 | 3 / 2 | 6 | **2.469136% (2/81)** | **5 / 5** | 77 |
| 35 | 4 / 3 | 102 | **0.682790% (1699/248832)** | **8 / 8** | 412 |

Level 34 is **above the ideal <2% target**, while remaining a prediction puzzle. No extra decoy, blocker or tighter budget was added solely to lower this statistic. All other requested random targets are met. Every shipped budget equals its proven optimum, so optimal-discovery probability equals completion probability in this batch. These figures are not measured human difficulty.

| Level | Committed starts | Starts retaining a win |
|---|---|---|
| 31 | Red (4,1), Yellow (2,2), Green (1,0) | Yellow |
| 32 | Red (2,1), Blue (1,2), Yellow (0,2) | Blue |
| 33 | Lower Red (1,1), Blue (2,3), Yellow (2,1) | Lower Red or Blue |
| 34 | Red (2,2), Blue (2,0), Yellow (1,0) | Blue or Yellow |
| 35 | Red (1,3), Red (3,3), Blue (0,2), Yellow (2,0) | Either Red or Yellow; not Blue |

### Authoritative analysis method

The existing, unchanged `PuzzleBatchAnalysis` replays **BoardManager** for all transitions. It memoizes piece identity, position, direction, active state, gate state and moves remaining. A uniform random policy chooses among committed legal actions at each state. Child probabilities are averaged; winning terminals contribute one and failed terminals zero. Winning action sequences remain distinct even where state paths merge. Optimal probability restricts numerator branches to minimum-length continuations, while retaining all legal actions in the denominator.

This is exact finite-horizon enumeration, not Monte Carlo. The independent existing bounded `DesignValidation.CanWin` search rules out every shorter route and accidental one/two-move wins. A bound breach throws rather than issuing a partial certificate. Fingerprint certificates bind actual layout, directions, state and budget; names or authored route length alone never prove optimality.

Baseline and candidate data: `batch31-old.json`, `batch31-old-analysis.json`, `batch31-candidates.json`, `batch31-analysis.json`. The authoring harness compiles unchanged actual C# rules with Unity attribute stubs; production assets are rechecked inside Unity. Use **SHIFT → Analyze Levels 31–35** to reproduce production JSON.

## J. Known and alternate routes

| Level | Recorded optimal taps |
|---|---|
| 31 | (2,2), (3,2), (3,1), (4,1), (3,1), (2,1) |
| 32 | (1,2), (2,1), (2,2), (1,2), (2,2), (3,1) |
| 33 | (1,1), (2,3), (1,2), (2,3), (3,3), (4,2) |
| 34 | (2,0), (2,1), (2,2), (3,3), (2,2) |
| 35 | (1,3), (1,2), (3,3), (3,2), (0,2), (2,0), (1,1), (4,1) |

- **31:** one sequence within six moves.
- **32:** two sequences; Blue can finish vacating its parking before or after Red's first move. Both revisit the original target junction and require the same geometry.
- **33:** two optimal sequences, swapping the first Red and Blue moves. A **seven-move diagnostic recovery** starts with Yellow (2,1), then follows the six recorded taps. This is tested under an extended budget, **not shipped as a normal completion**. With a seven-move shipping budget the random rate rose to about 10%, so this candidate was evaluated but not used. The underlying redirection/shared-space problem remains even with extra moves.
- **34:** six optimal action sequences, including a Yellow-led support push and different handoffs. There is more than one useful preparation plan; this is reported as two winning openings rather than pretending Blue is mandatory.
- **35:** 102 optimal sequences because target staging and gate preparation can partly commute. These are action sequences, not 102 fundamentally different strategies. All successful plans respect occupancy and launcher alignment. A low random probability does not imply a unique solution.

Perfect Shift uses the existing behavior and proved optima. There is no new reward or scoring rule. Level 33's slower recovery is diagnostic only, unlike the deliberately generous shipped Level 29.

## K. Wrong-choice consequences and recovery

- **31:** Red first redirects Blue left into the delivery room. Blue approaches a red-only exit with no remaining escape route. Green likewise occupies the narrow exit approach. Completing only the first workshop push still leaves Yellow in the receiving cell: the room is not ready for Blue.
- **32:** looping the target before clearing Blue's temporary return landing pushes Blue downward, onto the rightward return arrow and into the exit approach at (3,1). The wrong color then blocks delivery. The correct route passes through the original Red cell (2,1) facing right, after starting there facing left.
- **33:** Yellow occupies (2,2) before Blue vacates the crossing. The plan needs an additional committed move. A full seven-move BoardManager replay proves recovery; the exact graph excludes a six-move completion after that prefix. The waiting front Red cannot simply be delivered independently in its initial leftward direction.
- **34:** direct Red enters the rotator from the left, turns downward and stops in the lower dead pocket (3,1). The successful plan enters that same rotator from above, turns left and returns through (2,2). Blue's former landing must be free before the target makes this return.
- **35:** an early launcher occupies the rear target's landing. Pressing the control is useful, but continuing the control piece afterward sends a wrong-colored piece toward the gate/exit approach. The gate can intentionally stop the assembled row before state preparation; opening it is a separate step from constructing the row.

Only the specified Level 33 recovery is claimed as proven. Other mistakes have visible obstructions/directional consequences; no assertion of global unsolvability or recovery beyond the tested budget is invented.

## L. Reaction depth and earned payoff

| Level | Final depth | Recorded-route maximum | Maximum across all reachable actions |
|---|---:|---:|---:|
| 31 | 6 | 6 | 6 |
| 32 | 4 | 5 | 13 |
| 33 | 8 | 11 | 12 |
| 34 | 8 | 8 | 15 |
| 35 | 8 | 15 | 18 |

Depth is the existing **action-event count**, not the number of moved pieces, recursion depth or animation duration. Global maxima can arise in losing branches. Level 35's constructed shared trigger reaches **15** events and delivers the front target; the final eight-event turn sequence delivers the remaining target. Both use existing MEGA SHIFT feedback. The final cascade route is fixed terrain, but its **target/launcher chain is not assembled at the start**: separate downward-facing targets must be staged and the downward-facing launcher redirected into the row.

No new particle, shader, pulse, timing or global visual changes were necessary. Quiet one-event staging creates contrast with the existing large reaction feedback. Existing Reduced Motion, raycast and mastery checks remain in the suite.

## M. Instructions

| Level | New text |
|---|---|
| 31 | What changes on the other side? |
| 32 | Some progress must come back. |
| 33 | Two paths. One rhythm. |
| 34 | Where does the easy way end? |
| 35 | Build it before you fire it. |

All are conceptual. None names the first piece, reveals a switch order or gives a route. Out-of-scope instruction bytes remain untouched.

## Quality gate: explicit per-level review

These are design-review judgments, not claims of completed player testing.

| Criterion | 31 | 32 | 33 | 34 | 35 |
|---|---|---|---|---|---|
| 1. Unique identity | Cross-room occupancy | Return to own junction changed | Perpendicular target rendezvous | Same rotator, different entry | Assemble feeders and launcher |
| 2. Non-obvious start | Target push competes with remote parking | Exit is nearby but target must leave | Either target staging or clearance works | Direct-looking rotator route is unusable | Several setups work; launcher first does not |
| 3. Plausible choices | Three committed starts | Three starts | Three starts, two useful | Three starts, two useful | Four starts, three useful |
| 4. New reasoning | Clear receiving room before choke | Outward progress creates return obligations | Targets change each other's direction | Approach direction changes route outcome | Position and align pieces before state/trigger |
| 5. Understandable mistake | Wrong color enters exit room | Parked support is pushed into exit approach | Crossing occupancy costs a move | Target visibly stops facing dead pocket | Launcher occupies a staging cell |
| 6. Fairness | No hidden link or remote switch | Visible direction tiles; no toggle trick | Shared pushes follow existing rules | Clockwise rotation is unchanged | Gate, direction and occupancy all visible |
| 7. Memorable aha | Other-room parking includes vacating | Return to the original cell differently | Two targets are one system | The same tile is not the same route | Build the firing row before firing |
| 8. Earned payoff | Freed choke releases six events | Returned direction enables exit turn | Shared delivery then eight events | Reversed approach releases eight events | Constructed 15-event trigger, eight-event finish |
| 9. Visual distinction | Wide workshop above lower route | Compact loop/junction | Wide crossing and offset descent | Tall folded arc/lower return | Twin feeders above a firing row |
| 10. Interesting without tight limit? | Yes: occupancy/color obstruction remains | Yes: direction reversal and return space remain | Yes: target redirection/rendezvous remain, though efficiency pressure falls | Yes: direct route remains physically unusable | Yes: launcher and target alignment remain necessary; staging order becomes more forgiving |

The intended rhythm is discovery → rethink → coordination → prediction → construction. This is a perceived-difficulty hypothesis, not a ranking derived from random percentages. Levels 32/34 both involve returning movement, but one concerns revisiting a junction under a different direction and the other concerns pushing to control entry into a rotator; manual review must check that they do not feel repetitive.

## N. Files changed and preservation

- Five existing `Data/Levels/Level31_OnePadTwoGates.asset` through `Level35_TwoChannels.asset`; original filenames and GUIDs retained.
- Only catalog entries 31–35 in `Resources/LevelDesignCatalog.asset`; five proved fingerprint certificates in `Scripts/Levels/VerifiedOptimality.cs`.
- New `Editor/Batch31Review.cs`, `Tests/Editor/Batch31Tests.cs`, `Batch31FrozenFiles.txt`, `Batch31CatalogFrozen.txt` and metas.
- `LevelDesignTests.cs`: raise Level 34's expected payoff from four to eight. Preserve the old two-room gate-parity regression by loading an exact copy of the former Level 32 from new `Tests/Editor/Fixtures/LegacyOppositeRooms.asset`; no old parity assertions are removed. The fixture is outside the campaign asset directory and is not wired into progression.
- Historical frozen manifests refresh only authorized level/certificate/catalog digests. New manifests freeze 85 existing files and all 35 untouched catalog entries, including locked V2 puzzles, rules, progression/save, telemetry, scene and visuals.
- Scoped authoring/trace/freeze scripts, old and new analysis snapshots, test artifacts, this report and captures under `Validation`; README batch notes.

The simulation and analyzer algorithms are unchanged. No scene reference edits or automatic level-population changes are required: asset identities are stable. Do not rerun older whole-chapter/batch authoring scripts over these designs.

## O. Tests/results

**Full Unity 6000.3.23f1 suite: 194/194 passed, zero failed/skipped, zero C# compiler errors/warnings.** Thirteen cases were added to the previous 181. The suite ran in the isolated `ValidationProject` against matching production code/assets. Evidence: [test results](batch31-results.xml), [Unity log](batch31-unity.log), and five production-asset analysis JSON files in `batch31/`.

New coverage includes five exact graphs, independent minimum proofs, no one/two-move wins, spatial/directional consequences, target interaction, seven-move diagnostic recovery, decision metadata, constructed chain/event depths, 85 frozen files, 35 untouched catalog entries and actual-control **30→31→…→35→36** progression. Existing ordered/unique/restoration, Reduced Motion, save and presentation tests remain. The original Level 32 parity fixture is byte-identical to its pre-sprint asset.

## P. Screenshots

Six actual **1080×1920** Unity captures were produced and visually reviewed with the locked current identity:

- [Level 31 start](batch31-level31-start.png)
- [Level 32 start](batch31-level32-start.png)
- [Level 33 start](batch31-level33-start.png)
- [Level 34 start](batch31-level34-start.png)
- [Level 35 start](batch31-level35-start.png)
- [Level 35 final MEGA SHIFT x8 / Perfect Shift](batch31-level35-payoff.png)

No obvious clipping was observed. These captures are visual verification, not a substitute for player or physical-device testing.

## Q. Before Levels 36–40

**Stop at this batch for manual playtesting.** Especially check that the two zones in 31 read as separate jobs, 32 produces a return/reversal realization, 33 feels like coordination rather than another straight convoy, 34's approach difference is predictable, and 35's assembly feels satisfying despite multiple commuting setup orders.

Level 34's ideal probability threshold is missed slightly and is explicitly disclosed. Do not address that with arbitrary blockers. If players find 32/34 too similar or 35's setup too mechanical, revise the geometry before advancing. Physical-device readability/tap comfort and productive struggle require player observation. No changes to 36–40 have been made.
