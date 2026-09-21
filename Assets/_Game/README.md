# SHIFT — multi-color objectives and Levels 36–40

The current campaign is complete through Level 40. Only 36–40 are redesigned in this batch: shared turn, shared territory, cross-color assistance, multi-color optimization and a two-stage color machine. Targets are Red + Blue, except 38's Red + Yellow. Proven minimum/budget pairs: **6/6, 6/6, 7/7, 5/8, 7/8**. Level 39 has a tested seven-move normal completion and five-move Perfect Shift; Level 40 supports seven/eight moves and ends with **MEGA SHIFT x14**.

The approved objective extension retains legacy `targetColor`; empty `targetColors` falls back to it. All configured colors must clear. BoardManager has only three objective-related substitutions; movement, pushing, exits, rollback, move cost and reaction sequencing are unchanged. Single-color HUD output and all Levels 1–35 remain unchanged. Multi-color HUD uses compact discs and Clear Both/Clear All with EN/TR-ready formatting. Telemetry adds resolved objective metadata without changing its legacy field or completion semantics.

**Foundation: 206/206 passed. Final full suite: 219/219 passed**, zero failed/skipped or C# compiler errors/warnings. Nine 1080×1920 captures were reviewed. All 85 protected files and 35 untouched catalog entries passed verification; old board-specific regressions retain exact original fixtures. See `Validation/batch36-report.md` for A–U analysis, evidence, solutions and screenshots, and `Validation/multicolor-audit.md` for the compatibility audit.

Exact random-valid completion rates: **5.555556%, 2.880658%, 2.083333%, 11.111111%, 1.070602%**. Optimal discovery is separately **3.703704%** for 39 and **0.675154%** for 40. Levels 36 and 40 miss their ideal random-completion targets slightly; these policy metrics are not measured player difficulty. The extra final move preserves a legitimate recovery.

Use **SHIFT → Analyze Levels 36–40** or scoped `Validation/author_batch36.py`; do not rerun historical whole-chapter authoring scripts. **Manually playtest 36–40 next. No Levels 41+ were created.**

## Previous batch — Levels 31–35

Latest batch redesigns only Levels 31–35: two-room dependency, reversible commitment, synchronized targets, false-safe route and chain construction. Proven minimum/budget pairs: **6/6, 6/6, 6/6, 5/5, 8/8**. Uniform-valid-action completion rates: **0.462963%, 1.157407%, 1.234568%, 2.469136%, 0.682790%**. Level 34 misses its ideal <2% target; these probabilities are not human-difficulty measurements.

**194/194 Unity tests passed**, zero failed/skipped and zero C# compiler errors/warnings. Thirteen new cases cover exact graphs, minimum proofs, wrong choices, diagnostic recovery, payoff and 30→31→…→35→36 progression. All 85 protected files and 35 untouched catalog entries are preserved. Level 35 has a constructed 15-event trigger and MEGA SHIFT x8 finish. Six 1080×1920 captures were reviewed.

See `Validation/batch31-report.md` for the complete A–Q report and evidence. Use **SHIFT → Analyze Levels 31–35** and scoped `Validation/author_batch31.py`; historical authoring scripts can overwrite later designs. BoardManager, Levels 1–30 / 36–40, rules, progression/save/telemetry, scene references and final visual identity remain unchanged.

**Manually playtest 31–35 before proceeding to 36–40.** In particular, verify Level 32's reversal reads clearly, Level 34 feels fair, and Level 35's staging feels like constructing a chain. No work on 36–40 is included.

## Previous refinement — Levels 26, 27 and 30 V2

Latest refinement changes only 26, 27 and 30. Level 26 now requires clearing the support's landing before freeing Red's lane. Level 27 offers two real box-parking directions and requires vacating the shared crossing. Level 30 adds a competing entrance that consumes staging or gate access; wrong preparation is recoverable in diagnostic eight/nine-move routes while the shipped optimum/budget remains seven.

Proven minimum/budget pairs: **4/4, 5/5, 7/7**. Exact uniform-valid-action completion probabilities: **4.166667%, 2.777778%, 0.065104%**. Final event depths: **6, 4, 8**. These are authoritative graph metrics, not player measurements.

**181/181 tests passed**, zero failed/skipped and zero C# compiler errors/warnings. All 87 protected files, 37 untouched levels (including 28/29), and tested/delivered assets match. Four actual 1080×1920 captures were reviewed. See `Validation/refine26-report.md` for the complete A–N review, wrong-choice recoveries, evidence and screenshots. `Validation/author_refine26.py` is the scoped V2 authoring source; historical batch scripts would overwrite these refinements if reapplied.

Manual review is still required before 31–35, especially whether the new competing openings look plausible. BoardManager, rules, progression/save and current visual identity remain unchanged. No later levels were started.

## Previous batch — Levels 26–30

Only Levels 26–30 are redesigned in the latest batch: delayed target, shared-space parking, route prediction, alternate-route optimization and a multi-stage relay. Proven minimum/budget pairs are **4/4, 5/5, 5/5, 5/8 and 7/7**. Level 29 also has tested seven- and eight-move wins; only five earns Perfect Shift.

**Full Unity suite: 175/175 passed**, zero failed/skipped and zero C# compiler errors/warnings. Twelve new cases cover exact graphs, wrong plans, alternate wins/mastery, protected content and actual 25→26→…→30→31 progression. Eight 1080×1920 captures include the five starts, Level 29 normal/Perfect and Level 30 MEGA SHIFT x8. All existing visual identity, core rules, BoardManager, progression/save systems and Levels 1–25 / 31–40 are preserved.

Use **SHIFT → Analyze Levels 26–30**. Exact uniform-valid-action completion rates are 1.234568%, 2.777778%, 1.234568%, 37.019890% and 0.115741%. Level 29's optimal-discovery rate is separately 1.851852%; ordinary completion deliberately remains forgiving. These are graph-policy probabilities, not player or retention measurements. `Validation/batch26-report.md` contains the complete A–P review, solutions, authored decision themes and limitations. Author only this batch with `Validation/author_batch26.py`; do not rerun old whole-chapter authoring scripts.

**Manually playtest 26–30 before redesigning 31–35.** In particular, check the breather's perceived difficulty, the third decision theme in Level 28 and Level 29's replay motivation. No new motion or particle effects were added; existing mastery, chain feedback and Reduced Motion are retained and tested.

## Previous batch — Levels 21–25

Only Levels 21–25 have been redesigned in this batch. Their minimum/budget pairs are 3/3, 4/4, 5/5, 5/5 and 6/6. The goals are switch discovery, route preparation, delayed gate reversal, shared-passage order and a two-state setup/payoff puzzle. Levels 1–20 and 26–40, BoardManager, progression/save/telemetry and the final visual identity are unchanged.

Use **SHIFT → Analyze Levels 21–25** for exact BoardManager-based finite-horizon analysis. Uniform random valid-action solve probabilities are 12.5%, 6.25%, 1.3889%, 0.2604% and 2.0833%; these are policy probabilities, not measured player solve or retention rates. State-level branching counts may repeat one strategic choice. See `Validation/batch21-report.md` for the A–N design review, known solutions, metrics, wrong-choice consequences and manual-playtest caveats.

The five assets retain their original filenames/GUIDs. Their catalog entries and exact-fingerprint optimality certificates are updated. `Validation/author_batch21.py` is the scoped authoring source; the older whole-chapter script is historical and would restore older designs if rerun. Proof/capture outputs live under `Validation/batch21/`.

**Manually playtest this five-level batch before redesigning 26–30.** No later levels or new mechanics are included. Final full Unity suite: **163/163 passed**, zero failed/skipped and zero C# compiler errors/warnings. All 85 protected files and the tested/delivered asset comparison passed. Five actual 1080×1920 game captures were reviewed. Evidence is recorded in the batch report; physical-device/player validation remains separate.

## Final visual identity

The final identity pass adds an original alpine-lake background, a reusable light SHIFT wordmark, satin HUD and warm information cards, and a coherent icon-button family. Teal marks the primary action; Replay becomes navy when Next Level is available. Settings and chapter selection share the same card materials. The Step 2 layout and Step 3 board remain intact.

Presentation is built through `Wordmark`, `IdentityStyle`, `IdentityResources`, `IdentityIcon`, `ScenicBackdrop` and `RewardGleam`. Two tiny surface sprites are cached per UI canvas and released with it. The background is a project-owned static Resource texture, cropped proportionally on resize. No custom shader, imported font, package or scene-only wiring is required. Completion glints are decorative and nonblocking; Reduced Motion suppresses scale feedback, including a button already held when the setting changes.

Gameplay, all 40 levels, BoardManager, progression, saves, telemetry and board visuals are unchanged. No Hint/Undo controls or other mechanics were added. New tests freeze 72 existing files, check visual resource cleanup/aspect handling and exercise actual controls, completion and Reduced Motion. Full-suite results and review captures are recorded in `Validation/identity-report.md`; artwork source and exact generation prompt are in `Validation/identity-art.md`.

Validation: **150/150 tests passed**, zero failed/skipped and zero C# compiler errors/warnings. Eight actual UI screenshots were reviewed at 1080×1920. All 72 protected files and the tested/delivered asset comparison passed.

Remaining limits: built-in UI typography rather than a bespoke font; static background; Editor validation rather than physical-device profiling. Broader localization needs actual translations and font coverage. Optional custom wordmark/board art could refine the product further, but no additional asset is required for this pass.

## Sprint 5: Level Design 2.0 and mastery

**Small moves. Big reactions.** Sprint 5 differentiates the existing 40 puzzles through topology, preparation and payoff. No new mechanics, piece types, SDKs, packages, progression keys or builds. BoardManager and Levels 1–29 remain byte-identical to the Sprint 4 baseline.

## Design language and authoring

`Resources/LevelDesignCatalog.asset` contains 40 explicit LevelData references, a primary archetype, optional secondary archetypes, difficulty, reasoning flags, a written audit rationale and optional payoff/creative tags. The separate catalog preserves existing level asset bytes. Edit it in the Inspector; `Validation/author_design_catalog.py` is the corresponding reviewed authoring source. These labels never affect movement, state transitions, scoring or progression. Runtime lookup occurs at level setup and telemetry start, not per frame.

Archetypes: **Corridor** constrains routes; **Rooms** separates interacting zones; **Crossroads** makes pieces share space; **StateMachine** centers gate parity; **Cascade** prepares a large reaction; **Optimization** offers meaningfully different completion costs. Difficulty bands: Intro, Easy, Planning, Advanced, Mastery, Finale. Reasoning flags: Setup, Sequence, State, Space, Cascade, Optimization. Labels describe design intent, not measured player difficulty.

Topology uses only existing Wall placements on 4×4–6×6 grids. Advanced boards omit the floor inset and wall icon on these cells, exposing the dark board well as continuous blocked space. Gate/pad/arrow visuals and all simulation pieces remain intact. No shader mask, per-frame topology traversal or new terrain type. Primary redesigns: **30, 32, 34, 36, 38, 39, 40**; 31, 33, 35 and 37 receive constrained floor plans while preserving their core teaching/reasoning roles.

## Advanced design summary

| Level | Topology and plan | Taps / budget | Final depth | Quality evidence |
|---|---|---:|---:|---|
| 30 | L corridor, Blue staging alcove, raised corner exit | 5 / 5 | 4 | topology, setup, state, final payoff |
| 31 | Narrow lane, offset finish alcove | 4 / 4 | 2 | topology, one pad prepares two gates |
| 32 | Two rooms with opposite gate pairs | 8 / 8 | 2 | topology, two parity changes, upper exit must clear before lower pad |
| 33 | Control room above a shared two-Red lane | 5 / 5 | 2 | topology, setup, state, cooperative shared route |
| 34 | Central choke, south control pocket, raised east exit | 5 / 5 | 4 | topology, two-piece access order, state, payoff |
| 35 | Two isolated controls feed a narrow delivery lane | 5 / 5 | 2 | topology, setup, two independent states |
| 36 | Control pocket, upper crossing, lower delivery | 7 / 7 | 2 | open A, cross, reverse A, open B, finish; premature-close decoy |
| 37 | Upper staging room unlocks lower delivery route | 6 / 6 | 2 | topology, setup, two ordered state stages |
| 38 | Box staging below a rising serpentine exit | 5 / 5 | 6 | setup, push, switch, redirect, final cascade |
| 39 | Two Reds share a single delivery throat | 4 / 6 | 6 | topology, shared route, optimization: individual delivery also wins in 6 |
| 40 | Lower box room, A choke, upper return-loop finish | 6 / 6 | 8 | topology, box parking setup, two channel changes, shared space, Yellow decoy, multi-stage plan, final payoff |

Depth means committed **BoardAction count**, including moves, turns, delivery and gate events, not distance or number of pieces. `hasPayoffMove` and `expectedPayoffDepth` express an authored reaction threshold. Validation checks it against the known route. Tests separately assert the **last** reaction for 30, 34, 38 and 40, so a large opening action cannot substitute for their finish.

Levels 1–20 retain the original introduction → corners → shared routes → finale sequence. Levels 21–29 retain one-switch entry, other-color activation, parity, closed barriers, budget preparation, odd toggles, box behavior, direction and rotator teaching. Every level has an individual audit rationale in the catalog.

## Mastery and chain presentation

Verified-optimal completion adds **PERFECT SHIFT** under the ordinary completion headline for 1.6 seconds, with a restrained mint treatment and the existing success pulse. Next/Replay remain immediately usable; Chapter Complete remains the first line. Unknown or nonoptimal results do not show it. Reduced Motion disables scaling. No rewards, stars, currency or new audio dependencies.

Presentation/telemetry tiers: depth 0–1 Normal, 2–3 CHAIN, 4–5 BIG SHIFT, 6+ MEGA SHIFT. During playback the existing CHAIN counter shows progress. The final tier label appears once after the committed reaction, then fades; it does not announce a new superlative for each action. MEGA SHIFT is intentionally distinct from verified-optimal PERFECT SHIFT. Finale headers receive a subtle gold accent; the clean chapter result uses the brand line after the mastery cue expires.

## Creative candidates and telemetry

Six explicit editorial candidates: **30 OneTapChain**, **32 WrongChoice**, **34 WhichOneWouldYouTap**, **38 OneTapChain**, **39 PerfectShift**, **40 LooksImpossible**. OneTapChain refers to the prepared payoff, not a claim that the entire puzzle takes one tap. These are review tags, not validated advertising performance. No creative boards enter progression and no marketing UI is added.

Telemetry schema **3** preserves existing event names and reactionClass while adding archetype, difficultyBand, reasoningStyle, creativeHook, hasPayoffMove, expectedPayoffDepth, creativeCandidate, payoffReached, maxReactionTier, and event reactionTier. Payoff reached means a committed reaction met the authored threshold at any point in the attempt; it does not claim that this was the finishing tap. Blocked/cancelled taps cannot trigger it. Retries clear payoff/tier while preserving visit restart count. Existing duration, Perfect Shift, session accounting and no-op behavior remain.

## Validation and limits

Final Unity 6000.3.23f1 full suite: **143/143 passed, 0 failed, 0 skipped** (117 retained cases plus 26 added cases). No C# errors/warnings or relevant warning messages. Results: `Validation/sprint5-results.xml`; log: `Validation/sprint5-unity.log`. The separate pre-certification search also passed 5/5. Re-certified minima: **30=5, 34=5, 37=6, 39=4, 40=6**, using 25/23/37/16/106 search nodes respectively. All existing Chapter 1 certificates are unchanged. Reviewed actual 1080×1920 renders are retained in `Validation/sprint5/`.

Use **SHIFT > Validate Level Design 2.0** for all 40 references, metadata, explicit creative coverage and known solutions. `DesignValidation.CanWin` exhaustively replays authoritative BoardManager states, stopping at 200,000 nodes. Reaching the bound throws and proves nothing. Proof artifacts bind exact content fingerprints to the minimum and explored node count; only successful search results may update `VerifiedOptimality`. Layout/budget/state changes invalidate certificates. Metadata is outside the fingerprint. Runtime never runs a solver.

Run the complete Editor test assembly, including its play-transition presentation tests. The preserved wiring/progression/save/migration and Switch/Gate rollback tests remain. New coverage includes catalog validity, final-tap payoff, wrong parity order, finale decoy, both Level39 solutions, optimality searches, chain boundaries, telemetry snapshots, transient nonblocking mastery and byte-level preservation of 1–29 plus BoardManager. Final results and the complete A–N report are recorded in `Validation/sprint5-report.md`.

Manual review: open **SHIFT > Open Prototype**, use portrait Game view, Play, and use Inspector Unlock All Levels only for review. In Levels > Chapter 2 compare 30/32/34/36/38/39/40. On 39, deliver Reds individually for a valid six-tap result, then replay using the rear Red to earn four-tap Perfect Shift. On 40, compare the tempting Yellow push with the prepared Blue-first route. Settings > Reduced Motion keeps all text feedback without scaling. No manual wiring or save reset is required.

Limitations: wall topology and parity remain deterministic, with no undo, deadlock detection or new hint system. Difficulty and creative tags need human playtests. No physical-device readability/performance/haptics or acquisition tests were run, and production audio remains unassigned. **Recommended Sprint 6:** instrumented human playtests of room readability, decoy fairness, perceived payoff and Level39 mastery; tune only from evidence. Sprint 6 is not started.

---

# Historical Sprint 4 report: Chapter 2 (Levels 21-40)

## Switch / Gate rules

A Normal piece entering a diamond Switch toggles every Gate on that channel, once per entry. Starting on a pad does nothing. Pushed Normal pieces activate pads; PushBlocks do not. A Switch stops travel like an empty cell; it does not automatically redirect or launch its visitor. Channels use one or two diamond symbols, not color-only matching.

Closed Gates block new entry by Normal pieces and boxes. Open Gates allow entry. If a Gate closes around an existing occupant, the occupant remains and can leave; no crushing, displacement or automatic delivery occurs. If a pushed piece closes the gate that its pusher was about to enter, the unsafe reaction is rolled back completely, including gate states, with no move charge. This follows the existing cancelled-reaction feedback. No timers, pressure plates, delayed toggles or colored gates.

BoardManager remains authoritative. Minimal additions: GateOpen on BoardPiece; snapshot/rollback includes GateOpen; Switch activation toggles matching terrain in authored order. Appended enum/action values preserve old assets. SwitchActivated and GateOpened/GateClosed each count once toward reaction depth. BoardView reads action records and never determines gate state.

## Authoring / validation

PiecePlacement adds channel (1 or 2 for Switch/Gate; 0 otherwise) and initialOpen (Gate only). Switch/Gate are terrain and require no color/direction. Each channel must contain at least one Switch and one Gate. Duplicate terrain, invalid channels, orphan relationships and closed-gate initial overlap are rejected. Open-gate occupancy is supported. Existing Levels 1-20 are byte-unchanged.

`Validation/author_chapter2.py` holds the explicit handcrafted source for new assets and recorded solutions. It is an offline authoring utility, not a runtime generator or solver. `chapter2-design.json` records the same specification.

## Levels 21-40

| Level | Name | Budget | Purpose / instruction |
|---|---|---:|---|
| 21 | FirstSwitch | 3 | Enter the diamond pad to toggle its matching gate. |
| 22 | OpenForRed | 3 | Yellow can open the route for Red. |
| 23 | ToggleTwice | 4 | Two entries toggle twice. Prepare the open gate before Red enters its pad. |
| 24 | ClosedLane | 4 | Closed gates block movement. Activate Yellow first. |
| 25 | BeforeYouGo | 5 | Plan the gate state before spending moves along the lane. |
| 26 | ThreeEntries | 5 | Every entry toggles. Three pad entries leave the gate open. |
| 27 | BoxDoesNotPress | 5 | Boxes do not press pads. Push it aside, then let Yellow enter. |
| 28 | TurnThrough | 3 | Open the gate before the arrow sends Red through it. |
| 29 | ClockworkGate | 3 | The rotator still turns clockwise. The pad prepares its landing. |
| 30 | ClearThenOpen | 5 | Clear Blue into the pad, then send Red around the corner. |
| 31 | OnePadTwoGates | 4 | One diamond pad toggles both matching gates. |
| 32 | OppositeStates | 7 | Finish the open upper route before changing both gates. |
| 33 | SharedDeliveryGate | 5 | Open both gates, then push both Reds down the shared lane. |
| 34 | ClearTheCorridor | 5 | Blue clears the corridor and opens its two gates in one tap. |
| 35 | TwoChannels | 5 | Match one diamond with one; two diamonds with two. |
| 36 | CrossBeforeClosing | 6 | Cross Blue first. Yellow closes its approach while opening Red's route. |
| 37 | OpenTheSecondPad | 6 | One diamond opens the way to the two-diamond pad. |
| 38 | LiftAndTurn | 5 | Clear the box parking space. Only Red presses the pad beneath it. |
| 39 | PrepareToggleRedirect | 6 | Clear Blue, park the box, then activate the two pads in order. |
| 40 | TheStateOfShift | 7 | Prepare Blue and the box. One diamond opens two gates; two diamonds unlock the finish. |

Early levels isolate one activation, toggle parity and blocked routes. Middle levels combine lane clearing, boxes, direction/rotator tiles and shared channels. Later levels introduce two independent channels and ordering. Level40 uses seven recorded taps, two switch entries, three gates, a box and a direction/rotator route with an opening reaction depth of at least six. No new hints feature or arbitrary move-budget inflation is added; authored instruction text uses the existing subtitle/status path.

## Progression, migration and wiring

The explicit catalog now contains 40 ordered unique assets, and setup/repair overwrites every slot. The Inspector remains read-only for the array. Chapter selection shows 20 buttons per page with Chapter 1 / Chapter 2 tabs. Chapter 2 locks until Level21 is unlocked; recommendations retain existing labels. Level20 completes Chapter1 and enables Next to Level21; Level40 shows CHAPTER 2 COMPLETE with no further Next.

Existing SHIFT.Progress.v1 keys are retained. Old completed-frontier 19/19/19 migrates to unlocked/current 20 and completed 19. An old replay selection below 19 is preserved while unlocking 20. Incomplete Chapter1 does not unlock Chapter2; corrupt indexes still clamp. Migration persists once and is idempotent. No forced save reset.

## Telemetry, visuals and audio

Schema version 2 adds switchActivations, gateOpens and gateCloses to attempt/result snapshots. Only committed action lists are counted, so rollback has zero state events. Existing metrics and Perfect Shift semantics remain. Local/no-op providers are both tested; no external SDK.

Switches use a gold pad and channel diamonds; closed gates read as barriers, open gates as subdued passable surfaces. Existing short piece pulses provide activation/toggle feedback without adding per-action waits or Update loops. SwitchActivate, GateOpen and GateClose are optional null-safe audio cues. Switch activation uses the existing cooldown-limited Impact haptic. No production audio, native plugin or new illustration.

## Optimality and tests

All 40 recorded solutions are exercised. Existing Chapter1 proofs are preserved. Bounded exhaustive searches verified Levels 30, 34, 37 and 40 at **5, 5, 6 and 7 taps** respectively. Content-bound certificates now include channel and initial-open data for stateful puzzles. Perfect Shift is verified in the live Level40 completion test. Final full suite: **117/117 passed**, retaining the 75 previous test cases with chapter-count/finale expectations updated for the explicit expansion, plus 42 new cases. No C# compiler errors or warnings. Results: `Validation/sprint4-final-results.xml`; log: `sprint4-final-unity.log`. Reviewed actual Unity portrait renders in `Validation/sprint4/`, including the persistent switch-channel badge under occupied cells. No solver runs in players and recorded solution length alone never awards Perfect Shift.

## Manual steps / limitations

Open SHIFT > Open Prototype, choose portrait Game view and Play. The saved scene is wired to 40 assets. SHIFT > Repair Prototype Level References repairs an old loaded scene; SHIFT > Validate Chapter 2 Levels checks new recorded solutions. Use Reset Progress only when intentionally starting a new save; normal upgrade preserves it. No Android/iOS build work.

The occupied-gate rule is deterministic and documented above; there is no deadlock detector, undo, new hint system or native haptic differentiation. Gates only block entry. Physical-device readability, tactile comfort and subjective difficulty still require playtests. Runtime art is reused; production sounds remain unassigned.

Recommended Sprint 5: focused Chapter2 playtests using the local metrics, especially Levels 32, 36 and 38-40, before any tuning or additional mechanics. Sprint5 has not started.

---

# SHIFT - Sprint 3: Audio and Telemetry Foundation

## Architecture and privacy

**NO production analytics SDK is connected.** No network traffic, account/device identifier or analytics file output is added. `IAnalyticsService.Track(AnalyticsEvent)` is the replaceable provider contract. `TelemetryTracker` owns independent attempt/session snapshots; `PrototypeGame` coordinates accepted taps, resolution, navigation and lifecycle. BoardManager remains unchanged and analytics-free.

The local provider retains the latest 128 events in memory. **Verbose Telemetry** enables schema-versioned JSON in the Console only in Editor/development builds. **Analytics Enabled = false** selects the no-op provider; result metrics still function without event retention/output. Provider exceptions increment `ProviderFailures` without breaking play or causing retry/log floods. A future provider replaces the constructor argument in the coordinator; simulation needs no changes.

## Events and metrics

Schema version 1 events carry a sequence, monotonic session-relative time and independent snapshots where applicable:
- session_start, session_end, session_pause, session_resume.
- level_start, level_complete, level_fail, level_restart, level_select.
- level_abandon for an unfinished attempt replaced or ended with its session.
- piece_tap (successful or cycle-cancelled), blocked_tap, chain.

Count accepted normal-piece taps once after RequestMove succeeds. Exclude input rejected during resolution, terminal state or modal lock. Empty/terrain taps have no callback and are not counted; no extra raycasts are added. Committed moves count at acceptance even if presentation is interrupted. Completion/failure emits once after resolution. Cycle rollbacks count separately from ordinary blocked taps.

A **visit** starts on session launch, Next or level selection. Restart/replay starts a fresh attempt and carries visit restart count. Explicit selection resets restart count; attempt number continues per level within the session. Per-attempt time/moves/taps/depth reset on retry. Session counters aggregate attempts, including replays; completed count is completed attempts, not unique levels. highestLevel is one-based; prototype fallback is level 0 / index -1. Debug selection has reason `debug_select`.

Injected monotonic time excludes application pause. Terminal attempt duration freezes at resolution; session duration freezes at end. In-game modals count as thinking time. Quit/destruction cannot double-send session_end. Forced OS termination may omit it; no durable analytics recovery is implemented.

Attempt fields: identity/title/dimensions/target, budget/remaining moves, recorded solution length, attempt number, visit restarts, successful moves, accepted/blocked/cancelled taps, max/total depth, chains (>=2), strong chains (>=4). Derived values: blocked ratio, average depth per successful tap, verified move difference and completion efficiency. Reaction categories: simple=1, chain=2-3, strong=4-5, major=6+. Categories never affect rules.

## Verified optimality and Perfect Shift

`KnownSolutionLength` is a recorded route length; `VerifiedOptimalMoveCount` is nullable and separate. Only **7=3, 9=2, 10=4, 15=5, 18=6, 20=6** have proofs from existing exhaustive bounded-search tests. SHA-256 certificates in `VerifiedOptimality` bind exact dimensions, budget, target and placements. Changed puzzle data invalidates the certificate. No asset layout changes or runtime solver.

`Telemetry.Result` exposes the terminal attempt snapshot for a future result screen. `perfectShift` requires completion, a verified optimum and exactly that many successful moves. Unknown optimum never awards it. JSON uses hasOptimal=false / verifiedOptimalMoves=-1; ignore moveDifference/efficiency when unknown. Efficiency=optimal/actual only on verified completion. No stars, rewards or new win-screen feature.

## Audio, settings and haptics

AudioClips centralizes existing cues plus UIButton, PanelOpen and PanelClose. The four-voice pool is reused even on reinitialization. Master `volume` and `sfxVolume` multiply; mute stops active voices. Null clips are safe. CuePlayed still publishes intent when muted/without clips. Production clips remain empty; optional procedural sounds were skipped.

Bottom-right **SETTINGS** provides Sound, Haptics and Reduced Motion. Changes apply immediately and persist under **SHIFT.Settings.v1.Sound / Haptics / ReducedMotion**, separately from **SHIFT.Progress.v1.** Defaults: sound on, haptics off, reduced motion off. Missing settings can inherit explicit Game Feel defaults; saved values take precedence. Master/SFX volume is Inspector configuration, not extra persisted user settings. Tests isolate their keys.

HapticService exposes Light, Impact, Exit, Success and Failure; Medium remains compatible. Impact/Exit/Success/Failure share the cooldown-limited Handheld.Vibrate fallback. Light intentionally does nothing. No differentiated native intensity or plugin is claimed; Editor/desktop do not vibrate.

## Debug tooling and cost

Enable **Show Telemetry Overlay** before Play, or restart after changing it. It defaults off and is compiled only for Editor/development. Text refreshes twice per second only while instantiated. Graphics have no raycast targets and blocksRaycasts=false. It displays level/attempt/time/moves/taps/blocked/restarts/max-chain/optimum. The diagnostic overlay can cover part of the HUD; release UI never includes it.

Telemetry has no Update loop, file writes, reflection or per-frame serialization. Small event snapshots and a bounded queue keep storage controlled. Fingerprints run at level load, not on taps. Existing UI construction/coroutine allocations remain.

## Verification and manual setup

Full Unity 6000.3.23f1 suite: **75/75 passed (61 existing + 14 new), 0 failed**, with no C# compiler errors or warnings. Results: `Validation/sprint3-results.xml`; log: `Validation/sprint3-unity.log`. Actual 1080x1920 settings and debug-overlay renders were reviewed and retained under `Validation/sprint3/`. All previous 61 tests are retained. Added coverage: deterministic duration, counters, retry/session/pause, perfect/unknown results, provider failure/no-op, bounded snapshots, certificates, independent settings, audio gain/mute/pool and live overlay/modal input.

Open **SHIFT > Open Prototype**, choose portrait Game view and Play. No manual wiring or SDK install. Use SETTINGS for persisted toggles and Audio Clips for future original sounds. Verbose Telemetry is for local inspection; disable it for ordinary playtests. Physical-device audio, haptics, safe areas and performance still require validation. No Android/iOS signing or builds in this sprint.

Recommended Sprint 4: a small instrumented playtest, local difficulty/funnel review and original audio/haptic device validation before any tuning decision. Sprint 4 has not started. No mechanics, levels, backend or monetization added.

---

# SHIFT — Sprint 2.5: Visual Identity Lock



## Sprint 2.5 presentation



The identity pairs a pale sky-to-sand atmosphere with a deep slate board, colorful raised circular pieces, dark ink typography and teal primary actions. Simple translucent landscape shapes stay behind the safe area; they never intercept taps. All art is original procedural UI geometry and reusable sprites. No image downloads, audio clips or external packages are required.



- **Board:** wider portrait allocation (92% of safe width, up from 89%; taller available region), thick rounded outer frame, recessed well and inset cell shadows. Logical coordinates and piece-to-cell alignment are unchanged.

- **Pieces:** raised circular faces, highlights, stronger direction arrows, an intentional box symbol, distinct purple rotators and colored exit wells with pulse/delivery response.

- **HUD:** larger bold SHIFT title, level subtitle and one light goal/moves card with a target-color badge and emphasized counter. Low moves use a warm high-contrast ink color. Chain feedback occupies the strip above the board.

- **Controls:** shared rounded surfaces, soft shadows, short color/press feedback and teal Next Level emphasis. Success/retry panels use light tinted surfaces; the chapter finale uses a warm gold panel.

- **Chapter select:** a light card with contrasting locked, available, completed and recommended PLAY states. Labels preserve non-color status distinctions. The panel opens and closes quickly; closing releases its raycasts immediately and fades the remaining visual.

- **Motion:** existing 0.14-second travel and deterministic action playback are preserved. Tap/blocked responses, push accents, tile pulses and delivery glow remain local to pieces. Chain increments settle with a short pop; restart and next level receive a short board arrival. No animation delays selection or adds an inter-action wait.

- **Configuration:** `GameFeelSettings.panelDuration` controls board/modal arrival and modal exit (default 0.16 seconds). Reduced Motion disables decorative scale, shake and trails, including button presses and panel arrival, while retaining fades and necessary board travel.

- **Audio preparation:** Rotate and ChapterComplete are separate optional cues alongside existing Tap, Move, Push, DirectionChange, Exit, Blocked, Win and Lose. No external sound was sourced. Empty clips remain silent and safe. `AudioManager.CuePlayed` publishes semantic cues even when no clip is assigned.



UI is constructed on level load; no prefab or manual scene wiring is required for these visuals. Prototype.unity saves the new panelDuration and empty Rotate/ChapterComplete clip fields; existing serialized level references and selection are preserved. Shared palette/surfaces live in `VisualTheme`; the atmosphere uses a four-vertex gradient. Motion accents are reused rather than spawned each frame. Layout construction and existing coroutine/text allocations still occur on level load and reactions.



### Sprint 2.5 verification



Final full suite on Unity 6000.3.23f1: **61/61 passed, 0 failures**, with no C# compiler errors or warnings. Reports: `Validation/sprint25-final-results.xml` and `sprint25-final-unity.log`. Reviewed actual offscreen Unity renders at 1080×1920: Level20, completion/Next Level and locked chapter panel. Captures are under `Validation/sprint25/`. Existing gameplay, progression and level-reference tests remain intact. A new Reduced Motion presentation regression covers modal input blocking/restoration, decorative raycasts and restart consistency. Existing progression presentation coverage additionally asserts distinct Rotate, DirectionChange, Exit and ChapterComplete cues during the Level20 solution; capture-only waits allow entrance motion to settle without weakening gameplay assertions.



### Editor steps and limitations



Open **SHIFT > Open Prototype**, choose **1080×1920 / 9:16**, then Play. Use the existing Game Feel and Audio Clips Inspector fields. No asset import or manual setup is necessary. If an already-open scene still has stale level references from an earlier session, exit Play Mode and use **SHIFT > Repair Prototype Level References**.



The font remains Unity's bundled runtime font; no bespoke font/logo file was introduced. There are no production sound clips. Rendered UI review does not replace physical-device checks for tap comfort, safe areas, font readability or sustained frame rate. The scene is still a procedurally built UI, not a prefab authoring system.



Recommended next sprint: a focused Android/iOS device validation pass, then original audio/haptic tuning. Confirm readability, input comfort, safe areas and performance before expanding content. Sprint 3 and monetization work are not included.



---



# SHIFT — Sprint 2: Chapter 1



Open **SHIFT > Open Prototype**, select a **9:16 / 1080×1920** Game view and press Play. First launch starts at Level 1; subsequent launches resume the saved selection. Complete a level, then use **NEXT LEVEL** (primary) or **REPLAY**. Level 20 shows **CHAPTER COMPLETE** and replay. Restart/Retry remain immediate, without scene reloads.



## Player progression and local save



`LevelProgression` owns unlocking and selection independently of `BoardManager`. `SaveService` stores three PlayerPrefs integers under `SHIFT.Progress.v1.`: `Unlocked`, `Current`, and `Completed`. Indexes are zero-based internally. Save writes are flushed when a level is selected or completed. No board state is stored.



- Completion unlocks the next level and saves that next level as the relaunch destination, even before Next is pressed.

- Replay explicitly saves the earlier selection, without lowering unlock/completion progress.

- Selecting an unlocked level saves it immediately. Restart reloads its original layout and move budget.

- Missing saves begin at zero; invalid negative/oversized values clamp to the available chapter and unlocked range.

- PlayerPrefs is local, unencrypted and device-specific; no cloud sync or tamper protection is provided.

- Completed status is the highest completed level in the sequential chapter. Editor Unlock All is a test bypass, not a production completion system.



The **LEVELS** button opens a compact Chapter 1 modal in four rows of five. Locked buttons show LOCKED and reject selection. Completed levels show DONE; the next recommended level is highlighted. PLAY CURRENT LEVEL reloads the current selected puzzle; BACK closes the modal without resetting the board. Opening it prevents board input. An already-running animation may finish behind the modal; selecting a level cancels its coroutine, deactivates its view/effects, stops audio, and creates a clean board/HUD.



## Editor controls



On `SHIFT Prototype`, use **Load Selected Level** (index 0–19) during Play to inspect any level without saving its completion. It does not unlock content. Use **Unlock All Levels** to exercise normal player selection/Next with all levels available, or **Reset Progress** to reset only SHIFT's three keys. Reset/Unlock also work outside Play. These bypass methods and controls are compiled only in the Editor; production player selection always checks locks.



Play normally resumes the save, regardless of the debug index. Disable `Use Level Set` to retain the original FirstChain regression scene behavior. All 20 level assets are already assigned; no manual component wiring is required.



**SHIFT > Validate Chapter 1 Levels** checks all 20 layouts and recorded solutions. The Sprint 1 validator remains available for Levels 1–10. Tests use separate PlayerPrefs prefixes and do not erase player progress.



## Levels 11–20



Only existing mechanics are used. KnownSolution stores tap coordinates at the moment of each tap, with bottom-left origin. Every target is Red; colored exits make route conflicts visible.



| Level | Board | Budget | Planning idea | Recorded solution |

|---|---|---:|---|---|

| 11 OpenTheLane | 5×5 | 4 | Move Blue before advancing Red | (2,2), (1,2), (2,2), (3,2) |

| 12 CrossingOrder | 5×5 | 3 | Wrong crossing order traps Blue at the red exit | (1,1), (0,1), (1,1) |

| 13 SecondAct | 5×5 | 2 | First chain positions Red; second reaction delivers | (0,1), (3,1) |

| 14 TwoCorners | 5×5 | 4 | Trace two Direction tiles and clear the landing | (3,2), (0,1), (2,2), (3,2) |

| 15 TheLongSetup | 6×6 | 5 | Clear the landing before the tempting box chain | (3,2), (0,1), (3,2), (3,3), (3,4) |

| 16 TransformAhead | 6×6 | 5 | Rotator → Direction → second Rotator | (4,3), (1,0), (2,2), (3,3), (4,3) |

| 17 SharedCorridor | 5×5 | 5 | Move Blue sideways before Red enters their corridor | (1,1), (1,0), (1,1), (2,2), (3,2) |

| 18 ClearTriggerFinish | 6×6 | 6 | Park Green, clear Blue, trigger, finish | (2,2), (3,2), (0,1), (3,2), (3,3), (3,4) |

| 19 SharedDelivery | 6×6 | 4 | Two Reds share an exit; pushing saves one tap | (3,3), (0,1), (3,3), (3,4) |

| 20 TheFinalShift | 6×6 | 6 | Two setup moves, box chain, then Rotator/Direction route | (2,2), (3,2), (0,1), (3,2), (4,3), (5,4) |



Wrong moves can create dead ends; Restart recovers. No automatic deadlock detector was added. Level 19 intentionally penalizes separately delivering the leading Red: combine deliveries to stay within four taps. Bounded Editor tests check the intended minimum budgets on Levels 15, 18 and 20; no solver runs in the player.



## Sprint 2 visual polish



The board now has a dark backing and shallow shadow. Normal pieces have a soft highlight and larger arrows. Direction/Rotator tiles have a contrasting rim and larger symbols. Exits have a colored rim, dark inset and low-amplitude glow; Reduced Motion keeps their glow static. The existing move duration remains **0.14 s**: there is no added inter-action delay or slow-motion beat. AudioManager and all semantic events are preserved; Rotator continues sharing DirectionChange. No audio files were added.



## Sprint 2 changed files



Created with Unity metadata:



- `Scripts/Systems/SaveService.cs`, `LevelProgression.cs`

- `Scripts/UI/ChapterSelect.cs`

- `Tests/Editor/ProgressionTests.cs`, `ChapterLevelTests.cs`, `ProgressionPresentationTests.cs`

- Ten `Data/Levels/Level11_*.asset` through `Level20_*.asset` files listed above.

- `Validation/sprint2-*` reports and chapter screenshots.



Modified:



- `Scripts/Systems/PrototypeGame.cs`: progression coordination, guarded selection, clean next/replay and modal lifecycle.

- `Scripts/UI/GameHud.cs`: chapter navigation, Next CTA and chapter-complete state.

- `Scripts/Board/BoardView.cs`, `Scripts/Pieces/Piece.cs`: presentation polish only.

- `Editor/PrototypeGameEditor.cs`, `PrototypeSetup.cs`, `LevelValidation.cs`: debug controls, 20-level setup and validation.

- `Tests/Editor/SprintPresentationTests.cs`: isolated test save prefix and reusable screenshot helper; previous assertions retained.

- `Scenes/Prototype.unity`: references to Levels 11–20.

- This README.



BoardManager, BoardAction, LevelData, existing level assets, audio/haptic implementation, packages and ProjectSettings were not intentionally edited for Sprint 2.



## Sprint 2 verification and remaining work



Run **Window > General > Test Runner > EditMode > Run All**, including the tests that enter Play Mode. Final verification on 2026-09-19 with Unity **6000.3.23f1**: **56/56 tests passed**, including all original regressions and presentation tests. No C# compiler errors or warnings were reported. Final reports are `Validation/sprint2-final-results.xml` and `sprint2-final-unity.log`. Earlier attempts remain as audit logs; use the final report for delivery status.



Tests cover persistence defaults/clamping/reset, locks, replay, next/finale, modal flow, switching during animation, and all 20 recorded solutions. Bounded search verified minimum move counts of **5, 6 and 6** for Levels **15, 18 and 20**. Level 13's two interactions each have reaction depth 3–5; Level 20 includes a chain depth of at least 4. Final runtime/editor/test scripts, assembly definitions, scene and level assets were hash-checked against the tested copy. Core simulation and Levels 1–10 remain unchanged.



Visually inspected the 1080×1920 chapter-select, Next Level, Level 20 board and chapter-complete captures saved in `Validation`. The modal uses an opaque background to avoid underlying text bleed. These images use Unity's offscreen camera API, not device screenshots.



Device touch, vibration, notched layouts and frame rate still need hardware validation. Audio remains silent until real clips are assigned. No mid-level board persistence, automatic deadlock detection, cloud sync, production sounds or Android/iOS build/signing work is included. The next milestone should be device playtesting and difficulty tuning with new players, especially Levels 15–20, before expanding mechanics or content.



The following sections retain the core rules, Sprint 1 settings, and onboarding-level reference.



## Rules and architecture



- Coordinates start at bottom left. Up is +Y. Only Normal circles accept taps.

- A tap attempts one cell along its arrow. **Blocked taps do not consume a move.** A successfully committed reaction costs exactly one move regardless of depth. Empty cells, boxes, terrain, and taps during resolution cost nothing.

- Accepted blocked taps briefly remain `Resolving` so feedback cannot overlap another request. They return to `Playing` afterward. There is no automatic deadlock loss; Restart is always available.

- Normal pieces and PushBlocks can be pushed. Pushing transmits the incoming direction. A blocked push chain does not move.

- Walls and boundaries block movement. Terrain and movable occupancy remain separate layers.

- On entry, Direction tiles set direction; Rotators turn clockwise. Both immediately attempt another cell. A blocked continuation stops on the tile and keeps its new direction. Starting on a tile does not trigger it.

- Exits deliver Normal pieces. None-colored exits accept every color; colored exits accept only that color. Boxes and mismatched colors are blocked.

- Deliver every Normal piece of the target color to win. A last-move win takes precedence over loss. Successful non-winning taps can exhaust the move limit.

- Cyclic/oversized reactions roll back the entire transaction, clear their actions, and **cost no move**. Existing limits remain: 256 actions/operations, 128 nested steps. Partial state is never presented as a successful move.

- Reaction depth still counts the authoritative sequence's moves, turns/tile activations, and deliveries. `CHAIN xN` uses the action index during playback; UI does not recompute game rules.



`BoardManager` remains the authoritative plain C# model. Its only Sprint 1 rule change is charging a move only when the committed action list is nonempty. `BoardAction` is unchanged. `CompleteResolution` still releases the input lock after presentation. `PrototypeGame` coordinates input, load/restart and semantic feedback; `BoardView` plays actions, while `Piece` owns reusable visual effects. No simulation code references audio, vibration, animation, or UI.



## Game Feel settings



Expand **Game Feel** on the scene's `PrototypeGame` component. All production effect timings live in the serializable `GameFeelSettings` class:



| Setting | Default | Purpose |

|---|---:|---|

| Move Duration | 0.14 s | Smoothstep ease per cell, sequential readable chain |

| Tap Anticipation | 0.045 s | Immediate punch/highlight before motion |

| Piece Pulse Duration | 0.14 s | Tap and landing pulse |

| Blocked Duration | 0.12 s | Small local shake, no move charge |

| Exit Duration | 0.10 s | Scale/fade out |

| Shake Duration / Pixels | 0.10 s / 2.5 | Small board-only impact shake |

| Success Duration / Scale | 0.28 s / 0.035 | Clean completion pulse |

| HUD Pulse Duration | 0.22 s | Moves decrement response |

| Chain Fade Duration | 0.55 s | Clear temporary chain indicator |

| Low Moves Threshold | 2 | Warm counter color, no flashing |



Strengths for highlights, tap/impact punches, trails, HUD pulse and blocked shake are also editable. **Reduced Motion** disables decorative scaling, trails and shakes while retaining ordinary board travel and fades. Rounded tiles, soft shadows and one reusable motion accent per piece need no effect prefabs. There is no Instantiate/Destroy loop for reaction effects. UI/board construction happens on level load/restart; small coroutine and text allocations remain.



## Levels 1–10



All goals are Clear Red. Assets are under `Assets/_Game/Data/Levels`. Known solutions are verification metadata, not runtime instructions or hardcoded simulation branches. Coordinates below are the piece's position at the time of each tap.



| Level / asset | Board | Budget | Lesson | Known solution |

|---|---|---:|---|---|

| 01 FirstTap | 4×4 | 1 | Immediate tap/movement success | (1,1) |

| 02 FindTheExit | 4×4 | 3 | Follow a direct route into an exit | (0,1), (1,1), (2,1) |

| 03 ClearTheBox | 4×4 | 3 | Yellow pushes a box out of Red's path | (0,1), (1,0), (1,1) |

| 04 FirstChain | 5×5 | 2 | Yellow → box → Red → exit | (0,2) |

| 05 BlockedIsFree | 4×4 | 2 | Wall-blocked Yellow is free to try | (1,2), (2,2) |

| 06 TurnTheCorner | 4×4 | 2 | One Direction tile | (0,1), (1,2) |

| 07 MakeRoom | 4×4 | 3 | Move Blue first; a red exit rejects Blue | (1,1), (0,1), (1,1) |

| 08 Clockwise | 4×4 | 2 | Rotator turns Up into Right | (1,0), (2,1) |

| 09 PushAndTurn | 5×5 | 2 | Push chain into a Direction tile | (0,1), (3,2) |

| 10 SetTheChain | 6×6 | 4 | Clear Blue, trigger the chain, finish Red's route | (4,3), (1,2), (4,3), (4,4) |



Level 1 necessarily uses an adjacent exit to retain the existing delivery goal; Level 2 teaches deliberately navigating to it. Level 10's exit is red: pushing Blue upward first traps it below that exit, so the player must move Blue left before starting Yellow's chain. The on-screen hint points to clearing space. Mistakes can produce dead ends; these are reversible with Restart, and every initial board has a recorded valid solution.



**SHIFT > Validate Sprint 1 Levels** verifies the preserved first ten levels: dimensions, enums, duplicates, targets, a compatible exit, loading, and each recorded solution. `ValidatePlayable` adds the compatible-exit check without preventing pure simulation fixtures from testing deliberately incomplete boards through `Validate`. Tests additionally perform a tiny bounded search on Levels 7, 9 and 10 to check the intended minimum tap counts (3, 2 and 4). There is no runtime solver.



## Audio and haptics



Expand **Audio Clips** on `PrototypeGame` to assign Tap, Move, Push, Direction Change, Exit, Blocked, Win and Lose clips plus master volume. The four-voice `AudioManager` pool reuses AudioSources; exhausted voices replace the oldest round-robin voice. Null clips are silent and safe. No audio files were generated or added, so the sprint is silent until real clips are assigned. A single AudioListener lives on the generated background camera.



`HapticService.Light/Medium/Success/Failure` are presentation-only calls. Light is intentionally a no-op: Unity's generic vibration API cannot produce a reliable light tap. Medium/Success/Failure use a cooldown-limited `Handheld.Vibrate` fallback on Android/iOS, and no-op in the Editor/other platforms. **Haptics Enabled defaults off** because fallback strength/duration is device-dependent and cannot be calibrated through this API. Enable it for hardware evaluation; differentiated native feedback is future work. Restart does not create more audio voices or haptic services.



## Sprint 1 file inventory



Created (with Unity metadata):



- `Scripts/Systems`: GameFeelSettings.cs, HapticService.cs, AudioManager.cs

- `Editor`: LevelValidation.cs, PrototypeGameEditor.cs

- `Tests/Editor`: SprintLevelTests.cs, SprintPresentationTests.cs

- Ten `Data/Levels/Level01_*.asset` through `Level10_*.asset` assets listed above.

- Project-level `Validation/sprint1-*` reports and rendered screenshots when available.



Modified:



- `Scripts/Board/BoardManager.cs`: blocked/cancelled tap charging only.

- `Scripts/Board/BoardView.cs`, `Scripts/Pieces/Piece.cs`: configurable movement/effects and action-driven feedback.

- `Scripts/UI/GameHud.cs`, `Scripts/UI/PlaceholderVisuals.cs`: polished HUD, rounded placeholders, chain/counter/result feedback.

- `Scripts/Systems/PrototypeGame.cs`: feedback coordination, level selection, audio/haptics lifecycle.

- `Scripts/Levels/LevelData.cs`: authoring labels/hints, known solutions, playable validation.

- `Editor/PrototypeSetup.cs`: wire the level list when restoring a missing scene.

- `Tests/Editor/BoardManagerTests.cs`, `PrototypeSceneTests.cs`: new charging rule and preserved legacy scene regression.

- `Scenes/Prototype.unity`: ten-level references and selection, keeping the original level reference.

- This README.



`BoardAction`, grid/direction core types, `GridCell`, `SafeArea`, assembly definitions, original FirstChain asset and packages are unchanged by Sprint 1. No ProjectSettings edits were made by this implementation; the already-open Editor independently reserialized `ShaderGraphSettings.asset` during the session, and that file was left alone.



## Testing and remaining manual work



Run **Window > General > Test Runner > EditMode > Run All**. This includes tests that enter Play Mode. It exercises the existing rules, blocked/free taps, successful spending/loss, exact reaction depth, all ten solutions, minimum planning depth, safe missing audio, Editor haptics, chain fade, level switching, win/retry, and restart during the original chain animation.



Verified with Unity **6000.3.23f1** on 2026-09-19 in an isolated project copy: **40/40 tests passed**, followed by **1/1 presentation test passed** after adding offscreen portrait captures. No C# compiler errors or warnings were reported. Results are in `Validation/sprint1-results.xml`, `sprint1-presentation.xml`, and their corresponding Unity logs. The delivered scripts, assembly definitions, scenes and level assets were hash-checked against the tested copy.



Visually reviewed the actual rendered 1080×1920 Level 10 start, chain, and win images in `Validation/level10-*.png`: board, arrows, HUD, chain indicator and restart/replay controls are readable without overlap. These are offscreen Unity UI renders, not device screenshots. Device haptics, physical touch, notched screens, audio mixes and 60 FPS must still be verified on hardware. A 60 FPS request is not a measured performance guarantee.



No Android/iOS builds, signing, production audio, automatic progression/map, monetization, analytics or online systems were added. The next milestone is a short device playtest: tune motion/readability, confirm the ten-level learning curve with new players, and record real audio clips before expanding content.

