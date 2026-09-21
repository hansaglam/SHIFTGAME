# SHIFT — Levels 21–25 redesign / manual-playtest batch

Scope: exactly five puzzle assets, their design entries and fingerprint-bound optimality certificates. BoardManager, rules, progression, save, UI identity and Levels 1–20/26–40 are protected. Existing asset names/GUIDs stay stable so all 40 scene references retain their order.

## A–B. Old versus new designs and purpose

| Level | Previous design | New design / purpose |
|---|---|---|
| 21 — Switch Discovery | Red traversed its own pad, gate and exit; only one useful thing to tap, random valid tapping always won. | Compact one-pad/one-gate elbow. Blue is the route operator; Yellow can move into, and obstruct, the red-only exit approach. Teach control versus delivery in three moves. |
| 22 — Prepare Before Advancing | Separate Yellow pad and Red lane; explicit instruction named the opening piece. | Blue occupies Red's route. Blue must leave under its own downward direction. An early Red push turns Blue sideways and strands it before the closed gate. Teach preparation through a visible directional consequence. |
| 23 — Toggle Trap | Two independent pad entries with solution-revealing text; mostly independent toggle ordering. | Opposite-state gates and two pads on one channel. Blue opens the entry gate, Red crosses, Blue reverses the gates, then Red exits. The second press is necessary **after** crossing and fatal **before** it. |
| 24 — Shared Passage | Separate operator and single straight Red lane. | Vertical shared throat, two Reds, a Blue occupant and a competing Yellow pad entry. Blue yields space; rear-led pushes advance both Reds efficiently. Moving the front Red alone needs an extra move. |
| 25 — Prepare the Release | Red walked a long lane after an isolated switch action. | 5×5 bent route: clear box parking → first pad/gate → shove box sideways and redirect Red → second pad/gate → corner delivery. The upward box lift is a tempting losing setup. |

No extra traversable regions were added for decoration. The small side pockets serve control, parking or a visible wrong push; the remaining blocked cells define the route. Unused outer wall rows/columns were removed: sizes are **3×3, 5×3, 5×4, 4×5 and 5×5**. Level 24's rotated vertical passage makes its topology distinct from the horizontal teaching lanes. Level 23's control path sits below its gates so neither initial gate is hidden beneath a piece. Level 25 is a compact two-state preparation puzzle using only existing mechanics.

## C. Decision density — measured opportunities versus design judgment

The analyzer counts a state on the intended route only when there are at least two **committed**, distinct actions, at least one winning continuation and at least one continuation that cannot finish within budget. Blocked taps, cancelled reactions and forced states never count. This is an exact state-space measure, **not a claim that every counted state is an independent strategic insight**. An unchanged temptation can remain available over several turns.

| Level | Mixed-outcome decision states | Authored decision families for manual review | Minimum target |
|---|---:|---|---:|
| 21 | 3 | 2: choose the gate operator; use Red rather than letting Yellow occupy the exit approach once the gate opens. | 2 |
| 22 | 4 | 2: let Blue retain its downward direction; keep Blue parked instead of spending the delivery budget on it. The second is intentionally a teaching-level efficiency choice. | 2 |
| 23 | 5 | 3: operate the route rather than lift Red; cross before reversing; reverse before committing to the arrow landing. | 3 |
| 24 | 5 | 3: yield the throat with Blue rather than the competing pad entrant; lead from the rear to share pushes; preserve the open passage instead of retriggering its channel. | 3 |
| 25 | 6 | 3: move the box sideways rather than lift it; clear its landing before the shove; preserve preparation budget while traversing the two gate states and release the corner route. | 3 |

The authored counts are design assessments to validate with humans. In particular, do not interpret the repeated late “Red versus spare move” states in 21–22 as four deep decisions. No retention improvement, human solve rate or measured curiosity is claimed before playtesting.

## D. Intended aha moments

- **21:** “The piece near the exit is not the piece that opens it.”
- **22:** “If I push Blue, it loses the direction it needs to reach the pad.”
- **23:** “The second press must wait until Red is on the other side.”
- **24:** “The rear Red can do work for both Reds.”
- **25:** “Clear the landing, leave the box low, then the prepared route carries Red around the corner.”

## E–G. Opening choices, optimality and random resistance

Coordinates are zero-based, origin bottom-left. “Legal” below means a move-spending action. Plausibility to a human remains a playtest question; opening counts are not inflated with taps into visibly closed gates.

| Level | Valid opening taps | Openings that can win | Minimum / budget | Winning action sequences within budget | Exact random-valid solve probability | Previous probability |
|---|---|---:|---:|---:|---:|---:|
| 21 | Blue (0,0), Yellow (2,0) | 1 of 2 | 3 / 3 | 1 | 12.5000% | 100% |
| 22 | Red (0,2), Blue (1,2) | 1 of 2 | 4 / 4 | 1 | 6.2500% | 25% |
| 23 | Blue (4,1), Yellow (0,1) | 1 of 2 | 5 / 5 | 1 | 1.3889% (1/72) | 18.75% |
| 24 | Blue (2,2), Yellow (1,3) | 1 of 2 | 5 / 5 | 1 | 0.2604% (1/384) | 12.5% |
| 25 | Red (0,1), Blue (3,1), Yellow (1,0) | 2 of 3 | 6 / 6 | 2 | 2.0833% (1/48) | 9.375% |

### Method

`PuzzleBatchAnalysis` replays **actual BoardManager** transitions. It enumerates every committed action, memoizes complete dynamic piece/gate state plus remaining budget, and stops at win/loss/dead-end. Win contributes probability 1, failure 0; a decision's probability is the arithmetic mean of child probabilities. Path counts sum child counts, preserving different action sequences even when they converge to the same state. Random policy: uniformly choose among currently valid move-spending actions at every step. It is an exact finite-horizon probability, **not Monte Carlo**, not a model of human behavior and not uniform selection among complete paths. No seed or sampling confidence interval applies.

Reachable state counts (including terminal states): **8, 9, 21, 91, 32**. The 200,000-state bound is not reached; exceeding it throws rather than publishing a partial result. A separate existing BoardManager exhaustive search independently rules out every shorter solution and one/two-move wins. Certificates are bound to puzzle fingerprints; changing layout, direction, channel, initial gate state or budget invalidates them.

Baseline metrics are preserved in `batch21-old-analysis.json`; authoring results in `batch21-analysis.json`. The authoring harness compiles the unmodified BoardManager and LevelData source with Unity attribute/ScriptableObject stubs only. Final production-asset proofs are rerun inside Unity and exported to `batch21/LevelXX_…-analysis.json`.

### Recorded solutions

| Level | Taps |
|---|---|
| 21 | (0,0) → (0,1) → (1,1) |
| 22 | (1,2) → (0,2) → (1,2) → (3,2) |
| 23 | (4,1) → (0,2) → (3,1) → (1,2) → (3,2) |
| 24 | (2,2) → (2,4) → (2,3) → (2,2) → (2,1) |
| 25 | (3,1) → (0,1) → (1,1) → (2,2) → (2,3) → (3,4) |

Level 25 also permits swapping its first two taps. Both routes are six moves; the authored route presents parking setup before changing the first gate. Subsequent preparation cannot be skipped.

## H. Important wrong choices and consequences

| Level | Wrong choice | Visible consequence / tested result |
|---|---|---|
| 21 | Yellow (2,0) first | Yellow turns upward into the elbow before a red-only exit. It cannot enter that exit; it occupies Red's approach. Zero winning continuations. |
| 22 | Red (0,2) first | Red pushes Blue onto the arrow, turning it right before a still-closed gate. Blue can no longer reach its pad. Zero winning continuations. |
| 23 | Blue (4,1), then Blue (3,1) | The second pad closes the first gate while Red remains before it. Both opposite gate states visibly reverse. Zero winning continuations. |
| 23 | Blue, Red, then Red again before Blue's second press | Red spends a move stopping at the arrow before the closed second gate. The required toggle and remaining delivery no longer fit the budget. |
| 24 | Blue (2,2), then front Red (2,3) | Front Red moves alone; rear Red stays behind. No solution within five moves, but a six-move budget can recover. This is a visible efficiency penalty, not a hidden permanent lock. |
| 25 | Yellow (1,0) first | Yellow lifts the box into the side cell instead of clearing its east landing. The subsequent state/route preparation cannot finish in six moves. No claim of global unsolvability beyond this budget. |

Tight budgets intentionally make careless **valid** moves matter. Zero-cost blocked taps remain unchanged. Some wrong openings create immediate logical dead-ends; their color/direction/gate consequences are visible. Manual playtesting must check that players understand them without an explanation.

## I. Finishing reactions

Final `BoardManager.ReactionDepth`: **4, 2, 2, 4, 4** respectively. Level 24 and Level 25 each finish with Move → Turn → Move → Deliver. This is the existing game's event-depth metric, not “four pieces delivered” or recursion depth. Level 24's preceding rear push also delivers the front Red; the final turn delivers the remaining Red. No animation duration or rules were changed.

## J. Instruction changes

| Level | Old text | New text |
|---|---|---|
| 21 | Enter the diamond pad to toggle its matching gate. | A circle entering a diamond pad toggles its matching gate. |
| 22 | Yellow can open the route for Red. | A push changes a circle's direction. Make space before advancing. |
| 23 | Two entries toggle twice. Prepare the open gate before Red enters its pad. | Every press changes the state. |
| 24 | Closed gates block movement. Activate Yellow first. | Only one order keeps the passage clear. |
| 25 | Plan the gate state before spending moves along the lane. | Prepare before you commit. |

Levels 23–25 contain no named-piece move sequence. Levels 1–20 tutorial text remains byte-identical.

## K. Files changed

Production changes under `Assets/_Game`:

- `Data/Levels/Level21_FirstSwitch.asset` through `Level25_BeforeYouGo.asset` only; existing filenames and metas/GUIDs retained.
- `Resources/LevelDesignCatalog.asset`: only entries 21–25; sculpted topology, reasoning labels and payoff metadata match these designs.
- `Scripts/Levels/VerifiedOptimality.cs`: five additional exact-fingerprint certificates; existing certificate mappings retained.
- New `Editor/PuzzleBatchAnalysis.cs` and `Editor/Batch21Review.cs`; menu **SHIFT → Analyze Levels 21–25**. No runtime solver or game-rule dependency on analysis.
- New `Tests/Editor/Batch21Tests.cs`, `Batch21FrozenFiles.txt`, `Batch21CatalogFrozen.txt` and corresponding metas.
- Existing `Sprint5FrozenFiles.txt`, `BoardVisualFrozenFiles.txt`, `IdentityFrozenFiles.txt`: update only explicitly authorized five-level/certificate/catalog snapshots, retain every protected digest and assertion.
- README batch review notes.

Validation authoring/proof files: `author_batch21.py`, `batch21-harness.ps1`, `batch21-unity-stubs.cs`, candidates/old-level JSON, before snapshot, analysis JSON, report, Unity log/results and five screenshots. The old whole-chapter authoring script is historical; do not run it to apply this batch. The new authoring script writes only 21–25 and their metadata entries.

## L. Tests/results

Final full Unity suite: **163/163 passed, 0 failed, 0 skipped**, with **0 C# compiler errors or warnings**. The 13 new test cases cover five deterministic known solutions, exhaustive optimum/probability/path/opening assertions, independent no-shorter-route checks, failed-bound behavior, concrete wrong-order board states, recoverability of the inefficient shared-passage order, protected content hashes, and actual 20→21→…→25→26 progression with screenshots. The final run used Unity 6000.3.23f1 and completed on 2026-09-21.

Evidence: [full results](batch21-final-results.xml), [Unity log](batch21-final-unity.log), [delivery preservation check](batch21/preservation.json). All tested production assets/code match the delivered project. All 85 protected file hashes match; no out-of-scope level asset changed. Only documentation and copied validation outputs were finalized after this test run.

Protected scope: 85 source/assets, including all 35 out-of-scope level assets, scene, BoardManager, progression/save/telemetry, current UI and board art. Unchanged catalog entries are individually frozen. Existing 40-unique-ordered-reference and restoration tests are retained.

## M. Screenshots

Five actual Unity portrait renders were captured and visually reviewed at **1080×1920**, using the current final identity. They show each level's starting state; both gates in Level 23 are visible. These are game captures, not illustrations or mockups.

- [Level 21](batch21/batch21-level21.png)
- [Level 22](batch21/batch21-level22.png)
- [Level 23](batch21/batch21-level23.png)
- [Level 24](batch21/batch21-level24.png)
- [Level 25](batch21/batch21-level25.png)

## N. Recommendation before Levels 26–30

**Stop at this batch and manually playtest it.** Do not redesign 26–30 yet. Suggested short protocol: give players no verbal solution, record first tap, retries, where they pause, and ask them to explain the last mistake. Check whether 21–22 teach direction/state clearly; whether players delay the second press in 23; whether shared pushing in 24 is discovered; and whether 25 feels planned rather than guessed.

The lowest random probability (Level 24) does not automatically imply the best puzzle. Repeated decoys, strict budget, the dark blocked topology and early irreversible choices all need human review. If the second decision in Level 22 feels trivial, revise this batch rather than treating the state-space count as evidence of retention. Confirm read/tap comfort on a physical phone; Editor screenshots and deterministic solver proofs are not device or player evidence.
