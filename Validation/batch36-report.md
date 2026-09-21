# SHIFT — Levels 36–40 / multi-color chapter finale

Only campaign Levels 36–40 are redesigned. Levels 1–35, their instructions/GUIDs, save/progression, scene references, movement/exit rules and the locked visual identity are preserved. No Levels 41+, new piece types, Hint/Undo, backend, monetization or new effects are added.

The separately approved objective foundation retains legacy `targetColor`, adds optional `targetColors`, and completes only after every configured target color is absent. Empty lists fall back to the old field. BoardManager has exactly three substitutions: stored objective, Load snapshot and membership in the completion predicate. The entire movement, pushing, rollback, switch/gate, exit and reaction code is unchanged. See [assumption audit](multicolor-audit.md).

The brief's individual level specifications each require two colors, despite the general request not to make every level multi-color. This implementation follows those specific five specifications: every level has two target colors. The finale uses the explicitly permitted Red + Blue option; adding a third target was not necessary to its central idea. Three-color objective support is nevertheless implemented and tested.

## A–C. Old versus new, identity and targets

| Level | Previous puzzle / measured baseline | New identity and topology | Targets |
|---|---|---|---|
| 36 | Cross Before Closing: switch-order route; minimum 7, random completion 0.462963% | **Two Ways Through**, 5×5 fork around one shared rotator and a receiving shelf | Red + Blue |
| 37 | Open the Second Pad: state access; minimum 6, random 0.925926% | **Borrowed Ground**, 6×5 horizontal lane meeting a vertical yielding bay | Red + Blue |
| 38 | Lift and Turn: box setup; minimum 5, random 12.5% | **A Favor Returned**, 6×5 asymmetric launch junction with a separate return lane | Red + Yellow, two pieces of each |
| 39 | Prepare Toggle Redirect: minimum 4, normal random completion 64.814815% | **Better Together**, 5×5 upper rendezvous and optional lower detour; shared lift replaces separate work | Red + Blue |
| 40 | The State of Shift: two-channel setup, minimum 6, random 2.083333% | **The Color Machine**, 6×5 separate feeders, shared firing row and two color-specific terminal routes | Red + Blue |

No board exceeds 6×5. Only 38 and 40 use gates, one channel each. Level 40 develops the assembly idea through **sorting the two colors into different endings**: following the first delivered target is the wrong plan for the second. Its main problem is preserving a second operation after releasing the first color, not adding more channels.

## D–H. Player questions, realization, mistakes and decisions

| Level | Intended question / aha | Likely first mistake and lesson | Distinct authored reasoning themes | Mixed-outcome states on recorded route |
|---|---|---|---:|---:|
| 36 | “How can both colors use this turn?” Clear the receiving shelf, then approach the same rotator from different directions. | Push Blue first: Red is pushed into the approach to a Blue-only exit. An open route is not necessarily an accepting route. | 3: receiving space, shared turn, colored destination | 3 |
| 37 | “Who owns the middle cell now?” Blue first redirects Red, then must be pushed upward out of the shared bay. | Continue Blue right behind Red: Blue occupies the future Red lane with no useful upward launch. Yield while the launcher can still reach you. | 4: stage behind, redirect, yield, preserve color route | 5 |
| 38 | “How does helping Red let Yellow finish?” Yellow launches Red onto the shared switch; the second Red delivers it and reopens access for the other Yellow. | Deliver the upper Yellow immediately, pushing the lower Yellow out before it launches Red. Clearing a target can remove an essential helper. | 4: launch another target, restore access, avoid premature helper delivery, preserve matching exits | 5 |
| 39 | “Can one move do two jobs?” Stage Blue below the crossing so its upward push both delivers Red and reaches its own launch cell. | Send Red downward first. This can still succeed, but needs separate positioning and delivery. A normal win can suggest a cleaner replay. | 3: shared staging, combined delivery/lift, reuse vacated landing | 2, plus 2 efficiency-choice states |
| 40 | “What must remain usable after Blue leaves?” Assemble the row for Blue, keep the lower operator available, then divert Red into the return cascade. | Fire Green early or advance Yellow past the switch too soon. The first occupies a target's staging cell; the second occupies the shared gate approach. Building access and preserving access are separate tasks. | 5: stage targets, align launcher, use staging stop, preserve operator access, divert the waiting color | 6, plus 2 efficiency-choice states |

These theme counts are design judgments, not measured independent player decisions. Repeated opportunities to choose the same wrong plan do not establish new insight. Graph counters measure branching under the current budget; they do not measure human difficulty, retention or fairness.

## I–L. Openings, exact probabilities, minima and budgets

Coordinates are zero-based from bottom-left. A valid action here means a committed move: free blocked taps and cancelled reactions are excluded.

| Level | Valid starts / starts retaining a win | Winning action sequences within budget | Exact random-valid completion | Optimal discovery | Proved minimum / budget | Reachable states |
|---|---:|---:|---:|---:|---:|---:|
| 36 | 3 / 1 | 3 | **5.555556% = 1/18** | 5.555556% | **6 / 6** | 30 |
| 37 | 3 / 1 | 12 | **2.880658% = 7/243** | 2.880658% | **6 / 6** | 98 |
| 38 | 4 / 1 | 7 | **2.083333% = 1/48** | 2.083333% | **7 / 7** | 230 |
| 39 | 3 / 3 | 19 | **11.111111% = 1/9** | **3.703704% = 1/27** | **5 / 8** | 142 |
| 40 | 4 / 3 | 71 | **1.070602% = 37/3456** | **0.675154% = 35/5184** | **7 / 8** | 659 |

**Disclosed ideal-target misses:** 36 is above its ideal <5%; 40 is slightly above its ideal <1% for ordinary completion. The final's extra move permits a legitimate eight-move recovery. Its optimal-discovery probability is below 1%, but that does **not** replace the ordinary-completion figure. No extra decoy or tighter limit was added to manipulate either statistic. The hard <3% requirement for 38 is met.

| Level | Committed initial actions | Winning initial actions |
|---|---|---|
| 36 | Red (2,1), Blue (1,2), Yellow (3,1) | Yellow |
| 37 | Blue (0,2), Red (2,2), Yellow (2,1) | Blue |
| 38 | Yellow (1,2), Yellow (1,4), Red (4,3), Red (2,2) | Yellow (1,2) |
| 39 | Red (2,3), Blue (0,2), Yellow (1,3) | All three can complete within eight; efficiency differs |
| 40 | Red (1,3), Blue (3,2), Green (0,2), Yellow (2,0) | Red, Blue or Yellow |

### Proof method

The unchanged `PuzzleBatchAnalysis` exhaustively replays the authoritative BoardManager. Its state key includes piece identity, active status, position, direction, gate state and remaining moves. At each node it averages child completion probabilities over committing legal actions. This is finite-horizon enumeration, not sampling. Counts distinguish tap sequences even when paths merge.

Optimal discovery uses only minimum-length continuations in its numerator but retains all legal choices in the denominator. The existing independent bounded `DesignValidation.CanWin` search rejects solutions below the claimed minimum and rejects one/two-move wins. Search-bound overflow throws; it cannot issue a partial certificate. Fingerprints bind objective colors as well as the existing layout/state/budget data. Legacy fingerprints are unchanged.

Evidence: `batch36-analysis.json`, per-asset Unity JSON under `batch36/`, `batch36-old-analysis.json`; reproduce through **SHIFT → Analyze Levels 36–40**. `author_batch36.py --apply` checks analysis fingerprints before touching the five allowed assets. Older whole-chapter authoring scripts must not be reapplied over later designs.

## M. Known solutions and alternate routes

| Level | Recorded optimal taps |
|---|---|
| 36 | (3,1), (3,2), (2,1), (3,2), (1,2), (2,1) |
| 37 | (0,2), (1,2), (3,2), (4,2), (2,1), (2,3) |
| 38 | (1,2), (4,3), (1,4), (1,2), (3,3), (2,2), (3,3) |
| 39 | (0,2), (1,2), (1,3), (2,2), (2,3) |
| 40 | (1,3), (1,2), (3,2), (0,2), (2,0), (1,1), (3,0) |

- 36–38 have several valid setup/finish interleavings. Their sequence counts do not imply that many fundamentally different strategies.
- **39 normal seven-move route:** (2,3), (0,2), (1,2), (3,3), (1,3), (2,2), (2,3). Red is repositioned and delivered separately. The five-move route keeps Red at the crossing and lets Blue's lift deliver it. Both win under the shipped budget; only five earns Perfect Shift. The visible lower detour is not presented as a guaranteed normal solution within budget.
- **40 eight-move recovery:** the first five recorded taps, then (2,1), (3,1), (3,0). Advancing Red directly rather than using the aligned launcher takes two trigger moves instead of one. It still completes within the shipped budget; only seven is Perfect.
- 40's 71 completions include commuting feeder/control preparation and longer successful sequences. This is not advertised as a unique solution.

## N–O. Cross-color dependencies and wrong orders

**36.** Red and Blue traverse the same rotator at (2,2), approaching from south and west. Blue first pushes Red onto (2,0), facing a Blue-only exit. Red first can push the unparked box into its own exit approach. The shelf must be parked and vacated before either target uses the junction. Red can finish before Blue; the objective correctly remains Playing. Clearing Red alone does not solve the shared routing problem.

**37.** Blue's second action pushes Red from the shared bay into the horizontal route and changes its direction. Yellow then pushes Blue upward out of that bay. Continuing Blue right occupies the wrong lane and loses within budget. Red may be delivered first, but Blue must still preserve upward access; the two deliveries are not two independent initial routes. No extended-budget recovery for the bad rightward Blue branch is claimed.

**38.** Yellow's first push moves Red from (2,2) through the up arrow to the switch at (3,3), closing the gate on Yellow's other route. The second Red pushes the launched Red through the rotator into a Red-only exit and itself lands on the switch, reopening the gate. Thus a target's delivery also restores the other color's access. The remaining Yellow must enter the gate before its partner's later switch landing closes access again; a piece already inside a closing gate can still leave under the unchanged rules. Early upper-Yellow delivery sacrifices the launch opportunity; the lower Red's apparent downward route terminates at a Yellow-only exit. Direction and occupancy are essential in addition to state.

**39.** On the optimal fourth tap Blue rises from the shared turning cell and pushes Red into its accepting exit in the same reaction. Blue occupies the vacated launch cell and is then pushed toward its Blue-only exit. Delivering Red too early can still work, but forfeits that combined operation. A Red-only objective would incorrectly stop before Blue's remaining delivery; the runtime does not do so.

**40.** On optimal tap six the constructed row redirects and delivers Blue, advancing Red into the shared gate cell. The simulation remains Playing because Red is active. Tap seven uses the still-accessible Yellow operator to push Red upward, across the vacated Blue starting cell/rotator and through the separate Red exit. Simply following Blue leads Red toward a Blue-only exit. Red and Blue directly push in the shared row, then require different endings. Early Green and premature Yellow advance have zero winning continuations in the shipped budget. No global-unsolvability claim beyond the analyzed budget is inferred.

### Color-necessity counterfactual

As a design check, target pieces **and their accepting exits** were all changed to Red in temporary analysis data, never in campaign assets. Minima become **2, 5, 5, 5, 7**. In 39 the shared lift is no longer required to obtain the same optimum and the final action drops to two events; in 40 the preserved second operator can be skipped and the final action drops to six. In 38 the coordination puzzle falls from seven to five, with zero mixed-outcome decisions on the recorded monochrome route. This supports color-specific constraints being meaningful rather than decorative. It is not a player-study result. Evidence: `batch36-monochrome-analysis.json`.

## P. Reactions and presentation

| Level | Finishing depth | Recorded-route maximum | Maximum over reachable actions |
|---|---:|---:|---:|
| 36 | 4 | 4 | 6 |
| 37 | 4 | 4 | 5 |
| 38 | 6 | 8 | 12 |
| 39 | 6 | 6 | 8 |
| 40 | **14** | **14** | 18 |

Depth means the existing **action-event count**, not pieces moved or animation duration. Global maxima may belong to losing branches. Level 40's last two optimal reactions are 13 then 14 events, producing existing MEGA SHIFT feedback. Single-event staging remains quiet. No particles, shaders, animation timings, reward economy or Reduced Motion behavior were changed.

Instructions: 36 “Two colors. One turning point.”; 37 “The same space, at different times.”; 38 “Make room for each other.”; 39 “Can one move do two jobs?”; 40 “Build one system. Finish every color.” None names a first move, color order or switch sequence.

## Q. HUD and objective compatibility

The existing goal/moves panel is retained. Single-color levels retain the original 78px disc, anchors, font and `Clear Red` output. Multi-color levels display two/three 42px colored discs and `Clear Both` / `Clear All`, derived from the resolved objective. EN/TR formatting is isolated in `ObjectiveText`; Turkish output is also width-tested. No language selector or whole-game translation is introduced. Safe-area and bottom controls remain unchanged.

`LevelData.ValidatePlayable` checks that each target color has an accepting exit. The exit simulation itself is untouched. Telemetry retains its legacy `targetColor` field and adds `targetColors`; completion/counter/event meanings stay unchanged. Save/progression already follow GameState and indexes, so they need no extension. Full details are in the foundation audit.

## Quality gate — per-level review

These are explicit design judgments; manual playtesting remains necessary.

| Question | 36 | 37 | 38 | 39 | 40 |
|---|---|---|---|---|---|
| Unique identity? | Shared turn with different exits | Borrow and yield a bay | Target launches target; target restores access | Shared action versus separate effort | Shared launch followed by color diversion |
| Multi-color meaningful? | Wrong exit traps redirected Red | Following Red loses Blue's route | Early Yellow delivery loses the Red launch | Red must leave via its own exit during lift | Red cannot follow Blue out |
| Works almost unchanged all-Red? | No: minimum falls to 2 | No: yielding bypass shortens it | No: minimum falls to 5 | No: same minimum, different cheap route | No: second operator/long return bypassed |
| Targets interact? | Occupancy at one turn | Direct push and shared bay | Direct push, delivery and restored gate access | One reaction moves both | Shared row push and reuse of vacated start |
| Non-obvious opening? | Three plausible starts | Three competing starts | Four target choices | All starts can win; cleanliness differs | Four starts, three viable |
| Understandable mistake? | Wrong-color exit/blocked shelf | Wrong-color lane occupation | Helper delivered too soon or wrong exit | Visible extra separate moves | Staging or gate approach occupied |
| Strong aha? | Same tile, different approaches | Leave the space after helping | Delivering Red restores Yellow access | Lift and delivery are one move | First finish prepares the second finish |
| Interesting without limit? | Yes: accepting routes still matter | Yes: upward access must survive | Yes: launch/access order still matters | Yes as coordination; mastery pressure declines | Yes: both colors still need different endings |
| Different topology? | Fork/shelf | Long cross | Offset upper/lower delivery zones | Compact loop and detour | Feeder row and return bridge |
| Earned payoff? | Parked shelf unlocks turn | Correct yield unlocks Blue | Reciprocal setup enables six-event finish | Shared lift earns efficient finish | Constructed 13/14-event pair |
| New reasoning? | Plan two destinations through one tile | Temporarily share space | Do not deliver a useful target early | Optimize work across colors | Preserve an operation after the first release |

### Finale-specific gate

- **Memorable tomorrow?** Design hypothesis: “Build the row for Blue, keep the second launcher for Red.” This is a player-test question, not a proven retention claim.
- **Reaction justifies setup?** Seven optimal moves lead to two large, purposeful reactions, not added visual noise.
- **Chapter finale?** It combines known mechanics around one clear two-stage system, with an eight-move normal recovery.
- **Describable central idea?** One shared machine needs two different endings.
- **Readable before solving?** Only one gate channel and two target colors; all routes use existing icons. The initially occupied rotator under Blue particularly needs on-device review for recognition before it is vacated.

## R. Changed files / preservation

- Five existing `Data/Levels/Level36_CrossBeforeClosing.asset` through `Level40_TheStateOfShift.asset`; existing asset names/GUIDs retained.
- Only catalog entries 36–40 in `Resources/LevelDesignCatalog.asset`; objective-aware fingerprints and proved certificates in `Scripts/Levels/VerifiedOptimality.cs`.
- Authorized foundation: `BoardManager.cs`, `LevelData.cs`, `GameHud.cs`, new `ObjectiveText.cs`, additive objective metadata in `AnalyticsService.cs`.
- New `Editor/Batch36Review.cs`, `MultiColorObjectiveTests.cs`, `Batch36Tests.cs`, frozen manifests and original-level/source fixtures with metas.
- Existing chapter/design tests update shipped optimum/payoff expectations. Historical Level 36 wrong-order, Level 39 alternative route and Level 40 box/two-channel regressions retain their original data as fixtures. Visual tests cover both the new campaign and the old occupied-switch/two-channel case. Single-color HUD assertions remain for earlier levels; the final's objective expectation is updated.
- Prior frozen manifests update only authorized files/levels/catalog entries. The new manifest freezes 85 files and all 35 untouched catalog entries. A separate exact source assertion proves BoardManager has only the three approved substitutions.
- Scoped authoring/harness/freeze scripts, baseline/counterfactual analyses, this report, captures, logs, XML and README notes.

## S. Tests/results

Foundation: **206/206 passed**, zero failures/skips or C# compiler diagnostics. This includes legacy fallback, explicit single-color override, Red-first/Blue-first non-winning states, both cleared, three-color completion, immutable loaded objective, malformed objective/exit validation, deterministic replays of all 35 legacy levels, fingerprint compatibility, single/multi HUD and EN/TR width checks, and additive telemetry metadata.

**Final full Unity 6000.3.23f1 suite: 219/219 passed, zero failed/skipped, zero C# compiler errors/warnings.** This is 25 more cases than the pre-sprint 194. The isolated validation project matched all 316 delivered `_Game` files before the documentation-only README update. See [test results](batch36-results.xml) and [Unity log](batch36-unity.log). New batch coverage includes five exhaustive graphs, independent minima, trivial-win exclusion, cross-color consequences, 39's normal/Perfect routes, 40's recovery, frozen content/source comparison and real-control 35→36→…→40 chapter completion. Existing movement/push/rotator/gate/rollback tests are unchanged.

## T. Screenshots

Nine actual **1080×1920** Unity captures were produced and visually reviewed. No obvious clipping was observed; normal completion and Perfect Shift show distinct result text. Physical-device and player testing remain separate:

- [Level 36 start](batch36-level36-start.png)
- [Level 37 start](batch36-level37-start.png)
- [Level 38 start](batch36-level38-start.png)
- [Level 39 start](batch36-level39-start.png)
- [Level 40 start](batch36-level40-start.png)
- [Level 39 normal completion](batch36-level39-normal.png)
- [Level 39 Perfect Shift](batch36-level39-perfect.png)
- [Level 40 multi-color finale / MEGA SHIFT](batch36-level40-payoff.png)
- [Multi-color HUD](batch36-multicolor-hud.png)

## U. Recommendation after Level 40

Stop authoring here and manually playtest the full five-level arc. Check whether 36 clearly teaches both colors, 37's yielding reads as deliberate, 38's target-as-helper relationship is understood, 39 invites a cleaner replay after a normal win, and 40 feels like two stages of one machine. Compare perceived difficulty rather than sorting by random-policy percentages. Check occupied-special-tile readability and color recognition on a physical phone. Revisit 36/40's disclosed ideal-probability misses only if player observations justify it. No Levels 41+ have been created.
