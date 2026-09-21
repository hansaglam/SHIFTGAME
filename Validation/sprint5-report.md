# SHIFT Sprint 5 — final report

Unity 6000.3.23f1. **143/143 full-suite tests passed; 0 failures, 0 skipped.** The independent five-candidate proof run passed before certificate publication. No C# compiler errors/warnings or relevant warning messages. No Sprint 6 work started.

## A) Files created

Paths below are relative to the Unity project root.

- `Assets/_Game/Scripts/Levels/LevelDesignCatalog.cs`: authoring types and reference-based catalog lookup.
- `Assets/_Game/Scripts/Presentation/ReactionPresentation.cs`: shared presentation/telemetry tier classification.
- `Assets/_Game/Resources/LevelDesignCatalog.asset`: 40 individually audited entries.
- `Assets/_Game/Editor/DesignValidation.cs`: design validation and bounded authoritative search.
- `Assets/_Game/Tests/Editor/LevelDesignTests.cs`: 24 new regression cases.
- `Assets/_Game/Tests/Editor/LevelDesignPresentationTests.cs`: live topology/mastery/input/reduced-motion test.
- `Assets/_Game/Tests/Editor/Sprint5FrozenFiles.txt`: baseline hashes for Levels 1–29 and BoardManager.
- Unity `.meta` files for these assets and new Resources/Presentation folders.
- `Validation/author_design_catalog.py`: explicit audit/catalog authoring source.
- `Validation/check_design.py`: offline design aid; never used as optimality authority.
- `Validation/sprint5-baseline.json`, this report, results/logs, file inventory, five proof files and portrait captures under `Validation/sprint5/`.

## B) Files modified

- `Assets/_Game/Data/Levels/Level30_ClearThenOpen.asset` through `Level40_TheStateOfShift.asset`: all eleven advanced layouts reviewed; seven primary redesigns and four supporting topology passes.
- `Assets/_Game/Scripts/Board/BoardView.cs`: omit blocked-cell floor/icon rendering for sculpted boards, keeping model pieces intact.
- `Assets/_Game/Scripts/Levels/LevelData.cs`: catalog accessor only; no new serialized level fields.
- `Assets/_Game/Scripts/Levels/VerifiedOptimality.cs`: reusable fingerprint and newly proven certificates.
- `Assets/_Game/Scripts/Systems/AnalyticsService.cs`: schema 3 design/payoff/tier fields.
- `Assets/_Game/Scripts/Systems/PrototypeGame.cs`: pass topology presentation setting and completed result to HUD.
- `Assets/_Game/Scripts/UI/GameHud.cs`: final reaction labels, transient Perfect Shift, restrained finale heading.
- `Assets/_Game/Tests/Editor/ChapterTwoTests.cs`: updated content-specific finale/minimum/wrong-order expectations; added Level39 minimum case.
- `Assets/_Game/Tests/Editor/ChapterTwoPresentationTests.cs`: updated finale gate count and verified transient mastery.
- `Assets/_Game/README.md`: current design, authoring, audit, proof/test and review instructions.
- `Validation/author_chapter2.py` and `Validation/chapter2-design.json`: canonical authored layouts, routes and Level39 alternative.

BoardManager, piece/rule enums, progression/save implementations, scene reference wiring and Levels 1–29 were preserved. Prototype.unity remains the existing saved 40-level scene; no re-wiring or new campaign entries were needed.

## C) Level Design 2.0

One primary archetype plus optional secondaries: Corridor, Rooms, Crossroads, StateMachine, Cascade, Optimization. Difficulty bands: Intro/Easy/Planning/Advanced/Mastery/Finale. Reasoning flags: Setup/Sequence/State/Space/Cascade/Optimization. A rationale explains each level's intended learning or challenge role.

Separate reference-based ScriptableObject metadata preserves tutorial asset bytes. Labels do not enter simulation. Topology uses only existing walls on 4×4–6×6 grids. The renderer exposes blocked cells as the board's continuous dark well rather than repeated wall icons; no dynamic mask or per-frame topology work.

Audit: 1–20 retain tap/push/corner/shared-space instruction and the Chapter1 finale. 21–29 retain one switch, remote activation, parity, blocked gates, preparation, three entries, box nonactivation, direction and rotator teaching. Each of the 40 entries has its own rationale rather than an inferred name-based category.

## D) Levels redesigned

Primary: **30, 32, 34, 36, 38, 39, 40**. Supporting topology: **31, 33, 35, 37**. Levels **1–29 are byte-identical**, verified against the captured baseline in a permanent regression test. The same test freezes BoardManager.

## E) Level 30–40 design summary

| Level | Design and concrete quality evidence | Solution / budget | Last reaction depth |
|---|---|---:|---:|
| 30 | L-shaped corridor; Blue clears a shared landing and opens the high gate; final corner payoff | 5 / 5 | 4 |
| 31 | Narrow lane and offset exit alcove; one pad prepares two successive gates | 4 / 4 | 2 |
| 32 | Two separated rooms with opposite gate pairs; upper pad reverses all four gates; lower pad reverses them again only after upper delivery | 8 / 8 | 2 |
| 33 | Isolated switch control room and shared two-Red delivery corridor; staged gate opening then cooperative pushes | 5 / 5 | 2 |
| 34 | Central cross-shaped choke; Blue must yield south and open the east gate before Red can use the junction; clean raised finish | 5 / 5 | 4 |
| 35 | Two control pockets; separate channels must both prepare the constrained target lane | 5 / 5 | 2 |
| 36 | Yellow opens A; Blue crosses, reverses A, then opens B; Red completes the lower lane. Premature Yellow second-pad entry strands Blue | 7 / 7 | 2 |
| 37 | Upper staging room unlocks the lower route through two ordered channel activations | 6 / 6 | 2 |
| 38 | Blue clears box parking; Red pushes and activates the gate; final rising direction/rotator cascade | 5 / 5 | 6 |
| 39 | Two targets share one throat: cooperative pushing takes 4 taps; individually delivering the front then rear target takes 6 genuine movements | 4 / 6 | 6 |
| 40 | Lower box-staging room, single A connection, upper finishing room; Blue yields shared parking; Yellow's tempting upward box push fails within budget; two channel changes; final return-loop cascade | 6 / 6 | 8 |

Every level 30+ has concrete topology and state/setup evidence; 35+ have at least three substantive traits. Finale has topology, setup, state, shared space, a tested decoy, a multi-stage route and final payoff. No synthetic quality-count flag changes gameplay.

Depth is committed BoardAction count, including turns and state events. For 30/34/38/40, tests check the **finishing tap**, not only the largest reaction anywhere. Level32 has two depth-6 state reactions; its final delivery remains depth 2.

## F) Mastery / Perfect Shift

A verified minimum completion shows PERFECT SHIFT below the normal success/chapter headline for 1.6 seconds, in a subtle mint treatment with the existing success pulse. Next/Replay remain available immediately. Reduced Motion suppresses scaling. The live test advances directly from the active cue to Level40. Unknown optimum and the six-tap Level39 route do not receive mastery. No stars, rewards, currency or audio dependency.

## G) Chain tiers

Normal 0–1; CHAIN 2–3; BIG SHIFT 4–5; MEGA SHIFT 6+. During action playback only the ordinary CHAIN counter progresses. The final stronger label appears once per completed reaction and fades. No additional action delay or simulation change. MEGA SHIFT does not imply optimal completion.

## H) Creative-first tagging

Six explicit candidates: 30 OneTapChain; 32 WrongChoice; 34 WhichOneWouldYouTap; 38 OneTapChain; 39 PerfectShift; 40 LooksImpossible. They represent prepared payoff, visible ordering, shared access, cascade, efficiency and finale shape respectively. OneTapChain describes the payoff tap, not total puzzle length. Candidate selection is editorial, not measured ad performance. No new creative-only campaign levels or marketing UI.

## I) Telemetry

Schema 3 adds archetype, difficultyBand, reasoningStyle, creativeHook, hasPayoffMove, expectedPayoffDepth, creativeCandidate, payoffReached and maxReactionTier; individual events include reactionTier. Existing event names/reactionClass, duration, restart count, PerfectShift and provider interface remain intact. Payoff means a committed reaction met the authored threshold anywhere in that attempt, not necessarily on the finishing tap. Cancelled/blocked taps cannot trigger it. Restart resets tier/payoff, and previously emitted snapshots remain immutable. No-op provider tested; no SDK/network/backend.

## J) Verified optimality

Certificates were updated **after** the dedicated proof run succeeded. Search replays BoardManager and excludes every shorter committed-tap path. A 200,000-node bound throws instead of certifying. Each proof file contains content fingerprint, minimum and explored nodes.

| Level | Proven minimum | Nodes | Proof |
|---|---:|---:|---|
| 30 | 5 | 25 | `sprint5/proofs/Level30_ClearThenOpen.txt` |
| 34 | 5 | 23 | `sprint5/proofs/Level34_ClearTheCorridor.txt` |
| 37 | 6 | 37 | `sprint5/proofs/Level37_OpenTheSecondPad.txt` |
| 39 | 4 | 16 | `sprint5/proofs/Level39_PrepareToggleRedirect.txt` |
| 40 | 6 | 106 | `sprint5/proofs/Level40_TheStateOfShift.txt` |

Changed 30/34/37/40 fingerprints replace their old certificates; 39 gains its first proof. Chapter1 certificates are unchanged. Other Chapter2 optima remain unknown. Tests repeat the searches and verify data mutation invalidates certificates while design labels do not.

## K) Tests / results

- Pre-certification run: **5/5**, `sprint5-proofs-results.xml` / `sprint5-proofs-unity.log`.
- Full suite: **143/143 passed**, 0 failed, 0 skipped; `sprint5-results.xml` / `sprint5-unity.log`.
- Preserved 117 cases; added 24 design-model cases, 1 design presentation case and 1 new case in the existing minimum test.
- Preserved all40 ordered/unique references, 10→11→12, 20→21, Level40 completion, old-save migration, save isolation, deterministic solutions and Switch/Gate rollback coverage.
- New checks cover metadata/all archetypes, six creative candidates, final payoff depths, two state reversals and dead end, finale decoy, two valid optimization routes, five bounded proofs, tier boundaries, payoff snapshots/no-op, hash invalidation, frozen1–29/simulation, reduced-motion topology, mastery expiry and immediate Next.
- No C# compiler errors/warnings or relevant warning messages. Startup licensing/D3D diagnostic lines did not prevent the licensed Editor run or rendering.
- Reviewed actual 1080×1920 captures for 30/32/34/36/38/39/40, mastery and finale; stored in `sprint5/`.
- Tested C# and asset files exactly match the delivered source; byte-level comparison found zero differences before final documentation updates.

## L) Manual Unity steps

Open SHIFT > Open Prototype, portrait Game view, Play. For review, Inspector Unlock All Levels then Levels > Chapter2. Compare the seven primary boards. On Level39 use front-target-first for six taps, replay and push from the rear for four; compare normal completion with PERFECT SHIFT. On Level40 compare Yellow-up with Blue-down first, then observe the final depth-8 loop. Check Settings > Reduced Motion. Normal upgrades need no wiring, asset creation, save reset or package installation.

## M) Known limitations

The automated suite and portrait render review do not replace human puzzle/difficulty studies or physical-device testing. Creative tags are hypotheses. Gate parity, box behavior and blocked/cancelled reactions keep existing rules; no deadlock detector, undo or hint economy. Production audio remains unassigned. The local telemetry store remains bounded and nondurable. No Android/iOS builds or external services were introduced.

## N) Recommended Sprint 6

Run instrumented human playtests focused on topology readability, Level32/36 parity fairness, the Level40 decoy, perceived last-move payoff and whether players discover Level39's cooperative route. Use existing local metrics to prioritize changes before adding any mechanics. **Sprint 6 has not started.**
